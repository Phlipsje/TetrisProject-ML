using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class TetrisGame
{
    //The in-match game logic
    private Field field; //The field in which the game is being played
    public Field Field => field;
    private Piece activePiece; //The currently being controlled piece
    private List<Pieces> pieceQueue = new(); //Which pieces come next
    private int nextPieceLength = 5; //The amount of pieces shown in the next piece line
    private double nextPieceWaitTime;
    private readonly double nextPieceWaitTimeMax = 0.2; //When the next piece appears after the previous one is locked in place
    private Piece holdPiece;
    private bool holdUsed; //Can only hold a piece if has been placed since last time hold has been used
    public bool isGameOver; //Check if game is finished
    public bool isInStress; //Check if a player is close to a game over
    public int score;
    public int level = 1;
    private int levelLastFrame = 1; //Used for level up check
    public int clearedLines;
    private const double lineClearTextTimeMax = 1.4f;
    private double lineClearTextTime; //Amount of time a line clear text is shown on screen
    private string lineClearType = "";  //What type of line clear to show on screen
    private bool backToBack; //Check if a backToBack sequence is active
    private float multiplier = 1; //Based on backToBack (nothing or +0.5x total)
    public double gravityMultipler; //Can be changed in setting
    private GameMode gameMode;
    private int instance; //If this is player 0 or 1 (only relevant in multiplayer modes)
    private readonly bool drawHighScore;
    public int blocksBeingAdded = 0;
    private Settings settings;

    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    public Texture2D[] explosionTextures;
    public Texture2D coverLeftTexture;
    public Texture2D coverMiddleTexture;
    public Texture2D coverRightTexture;
    public Texture2D coverReceiveBarTexture;
    private SpriteFont font;
    
    //File locations
    private const string blockTextureFileName = "Bigger Base Block";
    private const string squareTextureFileName = "Square";
    private const string coverLeftFileName = "Tetris Cover Left";
    private const string coverMiddleFileName = "Tetris Cover Middle";
    private const string coverRightFileName = "Tetris Cover Right";
    private const string coverReceiveBarFileName = "Tetris Cover Receive Bar";
    
    public Controls controls;
    public GameHandler gameHandler;

    public TetrisGame(GameHandler gameHandler, Settings settings, Controls controls, GameMode gameMode = GameMode.Standard, int instance = 0, bool drawHighScore = true)
    {
        this.settings = settings;
        this.gameHandler = gameHandler;
        this.controls = controls;
        gravityMultipler = settings.game.gravityMultiplier;
        this.gameMode = gameMode;
        this.instance = instance;
        this.drawHighScore = drawHighScore;
    }
    public void Instantiate(int level)
    {
        switch (gameMode)
        {
            //Different positions for games in different gamemodes
            default:
                field = new Field(this, Color.Red, settings.game.width, startY: 200);
                break;
            case GameMode.TugOfWar:
                if (instance == 1)
                    field = new Field(this, Color.Red, settings.game.width, startX: 320 + 50, startY: 200);
                else
                    field = new Field(this, Color.Blue, settings.game.width, startX: 1280 - 50, startY: 200);
                break;
            case GameMode.Versus:
                if (instance == 1)
                    field = new Field(this, Color.Red, settings.game.width, startX: 320 + 50, startY: 200, true);
                else
                    field = new Field(this, Color.Blue, settings.game.width, startX: 1280 - 50, startY: 200, true);
                break;
        }
        
        this.level = level;
        FillQueue();
        NextPiece();
        score = 0;
        clearedLines = GetClearedLines(level);

        levelLastFrame = level;
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
        font = content.Load<SpriteFont>("Font");
        explosionTextures = new Texture2D[17];
        coverLeftTexture = content.Load<Texture2D>(coverLeftFileName);
        coverMiddleTexture = content.Load<Texture2D>(coverMiddleFileName);
        coverRightTexture = content.Load<Texture2D>(coverRightFileName);
        coverReceiveBarTexture = content.Load<Texture2D>(coverReceiveBarFileName);
        for (int i = 0; i < 17; i++)
        {
            explosionTextures[i] = content.Load<Texture2D>($"eEffect/explosion{i}");
        }
    }

    public void Update(GameTime gameTime)
    {
        lineClearTextTime -= gameTime.ElapsedGameTime.TotalSeconds;
        
        if (isGameOver)
            return;
        
        if(activePiece == null)
        {
            //Create piece is non exists
            GenerationPhase(gameTime.ElapsedGameTime.TotalSeconds);
        }
        else
        {
            activePiece.Update(gameTime);
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        //Draw field (including cover around it)
        field.Draw(spriteBatch);
        
        //Draw game over screen
        if (isGameOver)
            DrawGameOver(spriteBatch);
        
        //Draw piece
        if (activePiece != null && !isGameOver)
        {
            field.DrawPiece(activePiece, spriteBatch); 
        }

        //Draw next pieces
        Point nextPieceTopLeft = new Point(field.fieldX + field.fieldPixelWidth + 12, field.fieldY - 12);
        spriteBatch.DrawString(font, "NEXT", new Vector2(nextPieceTopLeft.X, field.fieldY-field.fieldHeightOffset), Color.White);
        for (int i = 0; i < 5; i++)
        {
            DrawPiece(GetNextPiece(pieceQueue[i]), spriteBatch, nextPieceTopLeft + new Point(0, (i+1)* (field.blockSize*2+24)));
        }

        //Draw hold piece
        if (holdPiece == null)
        {
            //Hold text in place of hold piece when nothing has been held yet
            spriteBatch.DrawString(font, "HOLD", new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 26,field.fieldY -field.fieldHeightOffset + 20), Color.White);
        }
        else //A piece is being held
        {
            Point holdPosition = new Point(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 12,field.fieldY + 18);

            //Color the held piece gray until a piece has been locked down (holdUsed reset on lock down)
            if (holdUsed)
            {
                DrawPiece(holdPiece, spriteBatch, holdPosition, true);
            }
            else
            {
                DrawPiece(holdPiece, spriteBatch, holdPosition);
            }
        }

        int textVerticalSpacing = 40;
        
        //Draw high score
        if (drawHighScore)
        {
            spriteBatch.DrawString(font, "BEST", new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing), Color.White);
            spriteBatch.DrawString(font, gameHandler.SettingsStruct.highScore.ToString(), new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*2), Color.White);
        }
        
        //Draw score
        spriteBatch.DrawString(font, "SCORE", new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*3.5f), Color.White);
        spriteBatch.DrawString(font, score.ToString(), new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*4.5f), Color.White);

        //Draw level
        spriteBatch.DrawString(font, "LEVEL", new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*6), Color.White);
        spriteBatch.DrawString(font, level.ToString(), new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*7), Color.White);
        
        //Draw cleared lines
        spriteBatch.DrawString(font, "LINES", new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*8.5f), Color.White);
        spriteBatch.DrawString(font, clearedLines.ToString(), new Vector2(field.fieldX-field.fieldCoverSideWidth-field.fieldReceiveWidth + 10,field.fieldY + textVerticalSpacing*9.5f), Color.White);

        //Draw line clear popup
        if (lineClearTextTime > 0 && lineClearType != null && lineClearType != "B2B ")
        {
            spriteBatch.DrawString(font, lineClearType, new Vector2(field.fieldX+field.blockSize * 2,field.fieldY + field.blockSize * 5), Color.Yellow);
        }
    }
    
    private void DrawPiece(Piece piece, SpriteBatch spriteBatch, Point position, bool greyOut = false)
    {
        //GreyOut is used to show that hold cannot be used
        Color pieceColor = piece.Color;

        if (greyOut)
        {
            pieceColor = Color.Gray;
        }
        
        //Use 4x4 matrix and draw out each block with true in that part of matrix
        for (int y = 0; y < Piece.hitboxSize; y++)
        {
            for (int x =  0; x < Piece.hitboxSize; x++)
            {
                //Check if the is a block in that part of the piece (in the 4x4 matrix of possible hitbox points)
                if (!piece.Hitbox[x, y])
                    continue;
                
                //Draw individual block of a piece
                Rectangle blockRectangle =
                    new Rectangle(position.X + field.blockSize * x, position.Y + field.blockSize * -y, field.blockSize, field.blockSize);
                spriteBatch.Draw(blockTexture, blockRectangle, pieceColor);
            }
        }
    }

    private void DrawGameOver(SpriteBatch spriteBatch)
    {
        string gameOverString = "Press enter";
        Vector2 stringSize = font.MeasureString(gameOverString);
        Vector2 stringPosition = new Vector2(field.fieldX, field.fieldY);
        stringPosition.X += (field.blockSize * field.Width - stringSize.X) / 2;
        stringPosition.Y += (field.blockSize * field.Height) * 0.75f;
        spriteBatch.DrawString(font, gameOverString, stringPosition, Color.White);
    }

    //Get amount of clearedLines if you start at a certain level
    private int GetClearedLines(int level)
    {
        int lines = 0;

        for (int i = 1; i <= level; i++)
        {
            lines += (i-1) * 5;
        }

        return lines;
    }

    //What happens when you press the hold key button
    public void HoldPiece(Piece piece)
    {
        if (holdPiece == null)
        {
            piece.RotationIndex = 0;
            //If the hold piece function is used for the first time a new piece needs to be spawned instead of grabbing the previous one
            RequestPiece();
            holdPiece = piece;
            holdUsed = true;
        }
        else if(!holdUsed)
        {
            piece.RotationIndex = 0;
            //Take hold piece out and make it the active piece
            activePiece = holdPiece;
            activePiece.Position = activePiece.startingPosition;
            activePiece.RotationIndex = 0;
            activePiece.Reset();
            
            //Have the old active piece become the new held piece
            holdPiece = piece;
            holdUsed = true;
        }
    }
    
    //Check if a new piece should be spawned in
    private void GenerationPhase(double timeElapsed)
    {
        //Check if the next piece should be spawned in
        if (nextPieceWaitTime > 0) 
        {
            nextPieceWaitTime -= timeElapsed;
        }
        else
        {
            gameHandler.PiecePlaced(instance);
            NextPiece();
        }
    }

    //Called by Piece.cs to start the process of creating the next piece
    public void RequestPiece()
    {
        nextPieceWaitTime = nextPieceWaitTimeMax;
        activePiece = null;
        holdUsed = false;
    }
    
    //Adds new pieces to the list of pieces the player has to use
    private void FillQueue()
    {
        //Create 1 of every type of piece and shuffles the order, this is pseudo random while still feeling random and not resulting in 'bad luck'
        Pieces[] pieceOrder = { Pieces.Block, Pieces.Line, Pieces.T, Pieces.S, Pieces.Z, Pieces.L, Pieces.J };
        
        //Shuffle the array
        pieceOrder = Util.ShuffleArray(pieceOrder); 

        foreach (var pieceByteValue in pieceOrder)
        {
            pieceQueue.Add(pieceByteValue);
        }
    }

    //Get the next piece
    private void NextPiece()
    {
        activePiece = GetNextPiece(pieceQueue[0]);
        pieceQueue.RemoveAt(0);

        //Fill queue if end of queue would be in sight in next pieces list
        if (pieceQueue.Count < nextPieceLength+1)
        {
            FillQueue();
        }
    }
    
    //Gets the value of the next piece and creates the corresponding object
    private Piece GetNextPiece(Pieces pieceInQueue)
    {
        Piece blockType;
        switch (pieceInQueue)
        {
            case Pieces.Block:
                blockType = new BlockPiece(field, this, controls);
                break;
            case Pieces.Line:
                blockType = new LinePiece(field, this, controls);
                break;
            case Pieces.T:
                blockType = new TPiece(field, this, controls);
                break;
            case Pieces.S:
                blockType = new SPiece(field, this, controls);
                break;
            case Pieces.Z:
                blockType = new ZPiece(field, this, controls);
                break;
            case Pieces.L:
                blockType = new LPiece(field, this, controls);
                break;
            case Pieces.J:
                blockType = new JPiece(field, this, controls);
                break;
            default:
                throw new Exception("blockType not specified");
        }

        return blockType;
    }

    public void GameOver()
    {
        isGameOver = true;
        field.PlayGameOverAnimation();
        field.Empty();
        
        //Draw the L on screen
        Point positionOfL = new Point((field.Width - 4) / 2 , 8);
        field.SetBlock(positionOfL.X, positionOfL.Y, Pieces.Ghost);
        field.SetBlock(positionOfL.X, positionOfL.Y + 1, Pieces.Ghost);
        field.SetBlock(positionOfL.X, positionOfL.Y + 2, Pieces.Ghost);
        field.SetBlock(positionOfL.X, positionOfL.Y + 3, Pieces.Ghost);
        field.SetBlock(positionOfL.X + 1, positionOfL.Y + 3, Pieces.Ghost);
        field.SetBlock(positionOfL.X + 2, positionOfL.Y + 3, Pieces.Ghost);
    }

    //Only used in multiplayer modes
    public void Win()
    {
        isGameOver = true;
        field.PlayGameOverAnimation();
        field.Empty();
        
        //Can't be drawn if field is too thin
        if (field.Width < 5)
            return;
        
        //Draw the W on screen
        Point positionOfW = new Point((field.Width - 5) / 2, 8);
        
        field.SetBlock(positionOfW.X, positionOfW.Y, Pieces.Ghost);
        field.SetBlock(positionOfW.X, positionOfW.Y + 1, Pieces.Ghost);
        field.SetBlock(positionOfW.X, positionOfW.Y + 2, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 1, positionOfW.Y + 3, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 2, positionOfW.Y + 2, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 2, positionOfW.Y + 1, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 2, positionOfW.Y, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 3, positionOfW.Y + 3, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 4, positionOfW.Y + 2, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 4, positionOfW.Y + 1, Pieces.Ghost);
        field.SetBlock(positionOfW.X + 4, positionOfW.Y, Pieces.Ghost);
    }

    //Updating scoring and lines cleared (according to variable goal system)
    public void HandleScore(int rowsCleared)
    {
        //In multiplayer the amount of cleared lines awarded differs
        int multiplayerClearedLines = 0;
        
        //Used to check how many lines have been added
        int previousLinesCleared = clearedLines;
        
        //Update time to show clear text on screen
        lineClearTextTime = lineClearTextTimeMax;
        
        if (backToBack && rowsCleared > 0)
        {
            lineClearType = "B2B ";
            multiplayerClearedLines = 1;
        }
        else
        {
            lineClearType = "";
        }
        
        #region Scoring table
        //Decide how many points to award
        switch (rowsCleared)
        {
            case 0:
                if (field.miniTSpin)
                {
                    score += 100 * level;
                    lineClearType = "Mini-T-Spin";
                    clearedLines += (int)(1 * multiplier);
                }
                else if (field.tSpin)
                {
                    score += 400 * level;
                    lineClearType = "T-Spin";
                    clearedLines += (int)(4 * multiplier);
                }
                else
                {
                    //Break back-to-back combo
                    backToBack = false;
                    multiplier = 1;
                }
                break;
            
            case 1:
                if (field.miniTSpin)
                {
                    score += (int)(200 * level * multiplier);
                    lineClearType += "Mini-T-Spin Single";
                    clearedLines += (int)(2 * multiplier);
                }
                else if (field.tSpin)
                {
                    multiplayerClearedLines += 2;
                    score += 800 * level;
                    lineClearType += "T-Spin Single";
                    clearedLines += (int)(8 * multiplier);
                }
                else
                {
                    score += 100 * level;
                    lineClearType = "Single";
                    clearedLines += 1;
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 2:
                
                if(field.tSpin || field.miniTSpin) //The conditions for a mini-t-spin double count as a t-spin double
                {
                    multiplayerClearedLines += 4;
                    score += (int)(1200 * level * multiplier);
                    lineClearType += "T-Spin Double";
                    clearedLines += (int)(12 * multiplier);
                }
                else
                {
                    multiplayerClearedLines += 1;
                    score += (int)(300 * level * multiplier);
                    lineClearType += "Double";
                    clearedLines += 3;
                    
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 3:
                if (!field.tSpin)
                {
                    multiplayerClearedLines += 2;
                    score += (int)(500 * level * multiplier);
                    lineClearType += "Triple";
                    clearedLines += 5;
                }
                else
                {
                    multiplayerClearedLines += 6;
                    score += (int)(1600 * level * multiplier);
                    lineClearType += "T-Spin Triple";
                    clearedLines += (int)(16 * multiplier);
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 4:
                //T-spins not possible with 4 lines cleared
                multiplayerClearedLines += 4;
                score += (int)(800 * level * multiplier);
                lineClearType += "Tetris";
                clearedLines += (int)(8 * multiplier);
                
                backToBack = true;
                multiplier = 1.5f;
                break;
        }

        //Extra bonus if player clears the entire field
        if (field.allClear)
        {
            multiplayerClearedLines += 5;
            score += 1000 * level;
            lineClearType = "All clear!";
            clearedLines += 10;
        }
        #endregion
        
        //Update level
        level = CalculateLevel();
        
        //Flash screen on level up
        if (level != levelLastFrame)
            gameHandler.ScreenFlash();
        levelLastFrame = level;
        
        //Call event in game handler
        gameHandler.LineCleared(clearedLines-previousLinesCleared, multiplayerClearedLines, instance);
    }

    private int CalculateLevel()
    {
        //Check if over the max level
        if (clearedLines >= 525) 
        {
            return 15;
        }
        int clearedLinesRemaining = clearedLines;

        //Calculate what level player is at based on lines cleared
        for (int i = 1; i <= 15; i++)
        {
            if (clearedLinesRemaining >= i * 5)
            {
                clearedLinesRemaining -= i * 5;
            }
            else
            {
                return i;
            }
        }

        return 0;
    }
}

public enum GameMode
{
    Standard,
    TugOfWar,
    Versus,
}