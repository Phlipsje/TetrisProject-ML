using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public class TetrisGame
{
    //The in-match game logic
    private Field field; //The field in which the game is being played
    private Piece activePiece; //The currently being controlled piece
    private List<Pieces> pieceQueue = new(); //Which pieces come next
    private int nextPieceLength = 5; //The amount of pieces shown in the next piece line
    private double nextPieceWaitTime;
    private readonly double nextPieceWaitTimeMax = 0.2; //When the next piece appears after the previous one is locked in place
    private Piece holdPiece;
    private bool holdUsed; //Can only hold a piece if has been placed since last time hold has been used
    public bool IsGameOver;
    public int Score;
    public int level = 1;
    public const int maxLevel = 15;
    private int clearedLines;
    private const double lineClearTextTimeMax = 1.4f;
    private double lineClearTextTime; //Amount of time a line clear text is shown on screen
    private string lineClearType = "";  //What type of line clear to show on screen
    private bool backToBack; //Check if a backToBack sequence is active
    private float multiplier = 1; //Based on backToBack (nothing or +0.5x total)

    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    public Texture2D[] explosionTextures;
    private SpriteFont font;
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";
    
    public Point WindowSize
    {
        get { return new Point(main.graphics.PreferredBackBufferWidth, main.graphics.PreferredBackBufferHeight); }
    }
    
    private Main main;

    public TetrisGame(Main main)
    {
        this.main = main;
    }
    public void Instantiate(int level)
    {
        this.level = level;
        field = new Field(this);
        FillQueue();
        NextPiece();
        Score = 0;
        clearedLines = GetClearedLines(level);
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
        font = content.Load<SpriteFont>("Font");
        explosionTextures = new Texture2D[17];
        for (int i = 0; i < 17; i++)
        {
            explosionTextures[i] = content.Load<Texture2D>($"eEffect/explosion{i}");
        }
    }

    public void Update(GameTime gameTime)
    {
        lineClearTextTime -= gameTime.ElapsedGameTime.TotalSeconds;
        
        if (IsGameOver)
            return;
        if(activePiece == null)
        {
            GenerationPhase(gameTime.ElapsedGameTime.TotalSeconds);
        }
        else
        {
            activePiece.Update(gameTime);
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        field.Draw(spriteBatch);
        if (activePiece != null && !IsGameOver)
        {
            field.DrawPiece(activePiece, spriteBatch); 
        }

        //Draw next pieces
        Point nextPieceTopLeft = new Point(field.fieldX + field.fieldPixelWidth + field.blockSize, field.fieldY);
        spriteBatch.DrawString(font, "NEXT", new Vector2(nextPieceTopLeft.X, nextPieceTopLeft.Y), Color.White);
        for (int i = 0; i < 5; i++)
        {
            DrawPiece(GetNextPiece(pieceQueue[i]), spriteBatch, nextPieceTopLeft + new Point(0, (i+1)*field.blockSize*4));
        }

        //Draw hold piece
        spriteBatch.DrawString(font, "HOLD", new Vector2(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize ), Color.White);
        if (holdPiece != null)
        {
            DrawPiece(holdPiece, spriteBatch, new Point(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize * 5));
        }
        
        //Draw score
        spriteBatch.DrawString(font, "SCORE", new Vector2(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize * 6), Color.White);
        spriteBatch.DrawString(font, Score.ToString(), new Vector2(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize * 7), Color.White);
        
        //Draw level
        spriteBatch.DrawString(font, "LEVEL", new Vector2(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize * 10), Color.White);
        spriteBatch.DrawString(font, level.ToString(), new Vector2(field.fieldX-field.blockSize * 4,field.fieldY + field.blockSize * 11), Color.White);
        
        //Draw line clear popup
        if (lineClearTextTime > 0 && lineClearType != null && lineClearType != "B2B ")
        {
            spriteBatch.DrawString(font, lineClearType, new Vector2(field.fieldX+field.blockSize * 2,field.fieldY + field.blockSize * 5), Color.Yellow);
        }
    }
    
    public void DrawPiece(Piece piece, SpriteBatch spriteBatch, Point position)
    {
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
                spriteBatch.Draw(blockTexture, blockRectangle, piece.Color);
            }
        }
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
            activePiece.Position = new Point(3, 0);
            activePiece.RotationIndex = 0;
            
            //Have the old active piece become the new held piece
            holdPiece = piece;
            holdUsed = true;
        }
    }
    
    #region Phases

    private void GenerationPhase(double timeElapsed)
    {
        if (nextPieceWaitTime > 0) //Check if the next piece should be spawned in
        {
            nextPieceWaitTime -= timeElapsed;
        }
        else
        {
            NextPiece();
        }
    }
    #endregion

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
        Pieces[] pieceOrder = { Pieces.Block, Pieces.Line, Pieces.T, Pieces.S, Pieces.Z, Pieces.L, Pieces.J };
        pieceOrder = Util.ShuffleArray(pieceOrder); //Shuffles the array

        foreach (var pieceByteValue in pieceOrder)
        {
            pieceQueue.Add(pieceByteValue);
        }
    }

    private void NextPiece()
    {
        activePiece = GetNextPiece(pieceQueue[0]);
        pieceQueue.RemoveAt(0);

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
                blockType = new BlockPiece(field, this);
                break;
            case Pieces.Line:
                blockType = new LinePiece(field, this);
                break;
            case Pieces.T:
                blockType = new TPiece(field, this);
                break;
            case Pieces.S:
                blockType = new SPiece(field, this);
                break;
            case Pieces.Z:
                blockType = new ZPiece(field, this);
                break;
            case Pieces.L:
                blockType = new LPiece(field, this);
                break;
            case Pieces.J:
                blockType = new JPiece(field, this);
                break;
            default:
                throw new Exception("blockType not specified");
        }

        return blockType;
    }

    public void GameOver()
    {
        IsGameOver = true;
        field.PlayGameOverAnimation();
        field = new Field(this);
        Point lpos = new Point(3, 8);
        field.SetBlock(lpos.X, lpos.Y, Pieces.Ghost);
        field.SetBlock(lpos.X, lpos.Y + 1, Pieces.Ghost);
        field.SetBlock(lpos.X, lpos.Y + 2, Pieces.Ghost);
        field.SetBlock(lpos.X, lpos.Y + 3, Pieces.Ghost);
        field.SetBlock(lpos.X + 1, lpos.Y + 3, Pieces.Ghost);
        field.SetBlock(lpos.X + 2, lpos.Y + 3, Pieces.Ghost);
    }

    public void HandleScore(int rowsCleared)
    {
        //Update time to show clear text on screen
        lineClearTextTime = lineClearTextTimeMax;
        
        if (backToBack)
        {
            lineClearType = "B2B ";
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
                    Score += 100 * level;
                    lineClearType = "Mini-T-Spin";
                    clearedLines += (int)(1 * multiplier);
                }
                else if (field.tSpin)
                {
                    Score += 400 * level;
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
                    Score += (int)(200 * level * multiplier);
                    lineClearType += "Mini-T-Spin Single";
                    clearedLines += (int)(2 * multiplier);
                }
                else if (field.tSpin)
                {
                    Score += 800 * level;
                    lineClearType += "T-Spin Single";
                    clearedLines += (int)(8 * multiplier);
                }
                else
                {
                    Score += (int)(100 * level * multiplier);
                    lineClearType += "Single";
                    clearedLines += (int)(1 * multiplier);
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 2:
                
                if(field.tSpin || field.miniTSpin) //The conditions for a mini-t-spin double count as a t-spin double
                {
                    Score += (int)(1200 * level * multiplier);
                    lineClearType += "T-Spin Double";
                    clearedLines += (int)(12 * multiplier);
                }
                else
                {  
                    Score += (int)(300 * level * multiplier);
                    lineClearType += "Double";
                    clearedLines += (int)(3 * multiplier);
                    
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 3:
                if (!field.tSpin)
                {
                    Score += (int)(500 * level * multiplier);
                    lineClearType += "Triple";
                    clearedLines += (int)(5 * multiplier);
                }
                else
                {
                    Score += (int)(1600 * level * multiplier);
                    lineClearType += "T-Spin Triple";
                    clearedLines += (int)(16 * multiplier);
                }
                
                backToBack = true;
                multiplier = 1.5f;
                break;
            
            case 4:
                Score += (int)(800 * level * multiplier);
                lineClearType += "Tetris";
                clearedLines += (int)(8 * multiplier);
                
                backToBack = true;
                multiplier = 1.5f;
                break;
        }
        #endregion
        
        //Update level
        level = CalculateLevel();
    }

    private int CalculateLevel()
    {
        if (clearedLines >= 600) //Over max level
        {
            return 15;
        }
        int clearedLinesRemaining = clearedLines;

        for (int i = 1; i < 15; i++)
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