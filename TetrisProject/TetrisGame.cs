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
    private List<byte> pieceQueue = new(); //Which pieces come next
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
        activePiece = new LinePiece(field, this); //Only called to avoid error, not actual first piece
        NextPiece();
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
    }

    public void Update(GameTime gameTime)
    {
        if (activePiece != null) //Check if there is an active piece
        {
            activePiece.Update(gameTime);
        }
        else if (nextPieceWaitTime > 0) //Check if the next piece should be spawned in
        {
            nextPieceWaitTime -= gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            NextPiece();
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

    public void RequestPiece()
    {
        nextPieceWaitTime = nextPieceWaitTimeMax;
        activePiece = null;
    }
    
    //Adds new pieces to the list of pieces the player has to use
    private void FillQueue()
    {
        byte[] pieceOrder = { 0, 1, 2, 3, 4, 5, 6 };
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
    private Piece GetNextPiece(byte pieceInQueue)
    {
        Piece blockType;
        switch (pieceInQueue)
        {
            case (byte)Pieces.Block:
                blockType = new BlockPiece(field, this);
                break;
            case (byte)Pieces.Line:
                blockType = new LinePiece(field, this);
                break;
            case (byte)Pieces.T:
                blockType = new TPiece(field, this);
                break;
            case (byte)Pieces.S:
                blockType = new SPiece(field, this);
                break;
            case (byte)Pieces.Z:
                blockType = new ZPiece(field, this);
                break;
            case (byte)Pieces.L:
                blockType = new LPiece(field, this);
                break;
            case (byte)Pieces.J:
                blockType = new JPiece(field, this);
                break;
            default:
                throw new Exception("blockType not specified");
        }

        return blockType;
    }
}