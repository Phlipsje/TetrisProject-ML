using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public class TetrisGame
{
    //The in-match game logic
    private Field field;
    private Piece testPiece;
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";

    public void Instantiate()
    {
        field = new Field(this);
        testPiece = new LinePiece(field);
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
            testPiece.Rotate();
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        field.Draw(spriteBatch);
        field.DrawPiece(testPiece, spriteBatch);
    }


}