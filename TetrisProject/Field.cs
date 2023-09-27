using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class Field //The field in which the pieces can be placed
{
    //References
    private TetrisGame tetrisGame;
    
    //Data variables
    private byte width;
    private byte height;
    public byte[][] blockArray; //Value in array is between 0 and 6 depending on which type of piece it is from so different colors can be used
    
    //Visual variables
    private const int blockSize = 16; //How large a block is
    private readonly int fieldWidth; //How many pixels wide
    private readonly int fieldHeight; //How many pixels high
    private readonly int fieldX; //X value of top left of field
    private readonly int fieldY; //Y value of top left of field
    private bool drawGrid;

    public byte Width
    {
        get { return width; }
    }
    
    public byte Height
    {
        get { return height; }
    }

    //Prepare the field to be usable
    public Field(TetrisGame tetrisGameReference)
    {
        tetrisGame = tetrisGameReference;
        
        //Data setup
        width = 10; //Adjust in settings later
        height = 16; //Adjust in settings later
        blockArray = new byte[height][];
        for (int i = 0; i < height; i++)
        {
            blockArray[i] = new byte[width];
        }
        
        //Visual setup
        fieldWidth = blockSize * width;
        fieldHeight = blockSize * height;
        fieldX = 50; //Adjust in settings later
        fieldY = 20; //Adjust in settings later
        drawGrid = false; //Adjust in settings later
    }

    //Handles clearing multiple lines at once
    public void ClearLines(byte[] lines)
    {
        //Make sure lines are sorted top to bottom
        
        foreach (byte line in lines)
        {
            ClearSingleLine(line);
        }
    }

    //Handles clearing a line
    public void ClearSingleLine(byte line)
    {
        //Move all rows down one
        for (int i = line; i < height; i++)
        {
            if (i == height)
            {
                //If at max height no row can fall down
                blockArray[i] = new byte[width];
                continue;
            }
                
            //Replace row with row above it
            blockArray[i] = blockArray[i++];
        }
    }

    //Draw all the already placed pieces
    public void Draw(SpriteBatch spriteBatch)
    {
        //Draw field
        spriteBatch.Draw(tetrisGame.squareTexture, new Rectangle(fieldX, fieldY, fieldWidth, fieldHeight), Color.LightGray); //Temp values
        
        //Draw blocks
        //For loops for getting blocks in sequence
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //Get block color
                Color blockColor;
                switch (blockArray[i][j])
                {
                    //TODO Assign colors correctly
                    case 0:
                        blockColor = Color.Green;
                        break;
                    
                    default:
                        blockColor = Color.White;
                        break;
                }

                Rectangle blockRectangle =
                    new Rectangle(fieldX + blockSize * j, fieldY + blockSize * (height - i), blockSize, blockSize);
                
                if (drawGrid)
                {
                    //Draws grid cells to make movement and position more clear
                    spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, Color.DarkGray); //TODO Change texture
                }
                
                //Draw block
                spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, blockColor);
            }
        }
    }

    //Used to get a block in a more intuitive manner
    public byte GetBlock(int x, int y)
    {
        return blockArray[y][x];
    }

    //Used to set a block in a more intuitive manner
    public void SetBlock(int x, int y, byte value)
    {
        blockArray[y][x] = value;
    }
}