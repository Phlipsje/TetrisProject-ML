using System;
using System.Collections.Generic;
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
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    public Texture2D[] explosionTextures;
    
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
    public void Instantiate()
    {
        field = new Field(this);
        FillQueue();
        NextPiece();
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
        explosionTextures = new Texture2D[17];
        for (int i = 0; i < 17; i++)
        {
            explosionTextures[i] = content.Load<Texture2D>($"eEffect/explosion{i}");
        }
    }

    public void Update(GameTime gameTime)
    {
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
        if (activePiece != null)
        {
            field.DrawPiece(activePiece, spriteBatch); 
        }

        //Draw next pieces
        Point nextPieceTopLeft = new Point(field.fieldX + field.fieldPixelWidth + 20, field.fieldY + 30);
        for (int i = 0; i < 5; i++)
        {
            DrawPiece(GetNextPiece(pieceQueue[i]), spriteBatch, nextPieceTopLeft + new Point(0, i*50));
        }

        //Draw hold piece
        if (holdPiece != null)
        {
            DrawPiece(holdPiece, spriteBatch, new Point(field.fieldX-80,field.fieldY+30));
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

    public void HoldPiece(Piece piece)
    {
        if (holdPiece == null)
        {
            //If the hold piece function is used for the first time a new piece needs to be spawned instead of grabbing the previous one
            RequestPiece();
            holdPiece = piece;
            holdUsed = true;
        }
        else if(!holdUsed)
        {
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
        //TODO place the active piece in the correct orientation and position, do this in Piece.cs Constructor
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
                blockType = new BlockPiece(field);
                break;
            case Pieces.Line:
                blockType = new LinePiece(field);
                break;
            case Pieces.T:
                blockType = new TPiece(field);
                break;
            case Pieces.S:
                blockType = new SPiece(field);
                break;
            case Pieces.Z:
                blockType = new ZPiece(field);
                break;
            case Pieces.L:
                blockType = new LPiece(field);
                break;
            case Pieces.J:
                blockType = new JPiece(field);
                break;
            default:
                throw new Exception("blockType not specified");
        }

        return blockType;
    }
}