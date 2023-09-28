using System;
using System.Collections.Generic;
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
    private List<byte> pieceQueue = new List<byte>(); //Which pieces come next
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";

    public void Instantiate()
    {
        field = new Field(this);
        activePiece = new LinePiece(field);
        FillQueue(); //Test
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
    }

    public void Update()
    {
        //Testing rotation
        if (Util.GetKeyPressed(Keys.R))
        {
            activePiece.Rotate();
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        field.Draw(spriteBatch);
        field.DrawPiece(activePiece, spriteBatch);
    }

    //Adds new pieces to the list of pieces the player has to use
    private void FillQueue() //TODO Get an error of "CreateAppHost" failed, debug later
    {
        byte[] pieceOrder = { 0, 1, 2, 3, 4, 5, 6 };
        Util.ShuffleArray(ref pieceOrder); //Shuffles the array
        Console.WriteLine(pieceOrder);

        foreach (var pieceByteValue in pieceOrder)
        {
            pieceQueue.Add(pieceByteValue);
        }
    }
}