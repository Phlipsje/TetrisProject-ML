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
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";
    
    public Vector2Int WindowSize
    {
        get { return new Vector2Int(main.graphics.PreferredBackBufferWidth, main.graphics.PreferredBackBufferHeight); }
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