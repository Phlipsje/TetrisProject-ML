using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class TetrisGame
{
    //The in-match game logic
    private Field field;
    
    //Sprites
    public Texture2D blockTexture; //Texture of a single block in a piece
    public Texture2D squareTexture; //Used for drawing rectangles with a single color
    
    //File locations
    private const string blockTextureFileName = "BaseBlock";
    private const string squareTextureFileName = "Square";

    public void Instantiate()
    {
        field = new Field(this);
    }

    public void LoadContent(ContentManager content)
    {
        blockTexture = content.Load<Texture2D>(blockTextureFileName);
        squareTexture = content.Load<Texture2D>(squareTextureFileName);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        field.Draw(spriteBatch);
    }
}