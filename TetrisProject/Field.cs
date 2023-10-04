using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class Field //The field in which the pieces can be placed
{
    //References
    private TetrisGame tetrisGame;
    
    //Data variables
    private const byte width = 10;
    private const byte height = 16;
    public byte[][] blockArray; //Value in array is between 0 and 6 depending on which type of piece it is from so different colors can be used
    
    //Visual variables
    private int blockSize; //How large a block is 
    private int fieldPixelWidth; //How many pixels wide
    private int fieldPixelHeight; //How many pixels high
    private int fieldX; //X value of top left of field
    private int fieldY; //Y value of top left of field
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
        blockArray = new byte[height][];
        for (int i = 0; i < height; i++)
        {
            blockArray[i] = new byte[width];
        }
        
        //Visual setup
        SetFieldPixelSizeByWindowHeight(80);
        drawGrid = false; //Adjust in settings later
    }

    private void SetFieldPixelSizeByWindowHeight(int percentage)
    {
        fieldPixelHeight = (int)Math.Round(tetrisGame.WindowSize.Y * (percentage / 100.0));
        fieldPixelWidth = (int)Math.Round((double)fieldPixelHeight / height * width);
        blockSize = (int)Math.Round((double)fieldPixelHeight / Height);
        fieldX = (tetrisGame.WindowSize.X - fieldPixelWidth) / 2;
        fieldY = (tetrisGame.WindowSize.Y - fieldPixelHeight) / 2;
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
        
        SetFieldPixelSizeByWindowHeight(80);
        spriteBatch.Draw(tetrisGame.squareTexture, new Rectangle(fieldX, fieldY, fieldPixelWidth, fieldPixelHeight), Color.LightGray * 0.5f); //Temp values
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
                    case 0:
                        blockColor = Color.Transparent;
                        break;
                    case (byte)Pieces.Block+1:
                        blockColor = Color.Yellow;
                        break;
                    case (byte)Pieces.Line+1:
                        blockColor = Color.LightBlue;
                        break;
                    case (byte)Pieces.T+1:
                        blockColor = Color.Purple;
                        break;
                    case (byte)Pieces.S+1:
                        blockColor = Color.LightGreen;
                        break;
                    case (byte)Pieces.Z+1:
                        blockColor = Color.Red;
                        break;
                    case (byte)Pieces.L+1:
                        blockColor = Color.Orange;
                        break;
                    case (byte)Pieces.J+1:
                        blockColor = Color.Blue;
                        break;
                    default:
                        blockColor = Color.Green;
                        break;
                }

                Rectangle blockRectangle =
                    new Rectangle(fieldX + blockSize * j, fieldY + blockSize * i, blockSize, blockSize);
                
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
    
    public void DrawPiece(Piece piece, SpriteBatch spriteBatch)
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
                    new Rectangle(fieldX + blockSize * (x + piece.Position.X), fieldY + blockSize * (y + piece.Position.Y), blockSize, blockSize);
                spriteBatch.Draw(tetrisGame.blockTexture, blockRectangle, piece.Color);
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
    
    public bool CollidesVertical(bool[,] hitbox, Vector2Int position)
    {
        for (int y = 0; y < hitbox.GetLength(1); y++)
        {
            for (int x = 0; x < hitbox.GetLength(0); x++)
            {
                if (hitbox[x, y] && (y + position.Y >= Height || GetBlock(x + position.X, y + position.Y) != 0))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool CollidesHorizontal(bool[,] hitbox, Vector2Int position)
    {
        for (int y = 0; y < hitbox.GetLength(1); y++)
        {
            for (int x = 0; x < hitbox.GetLength(0); x++)
            {
                if (hitbox[x, y] && (x >= Width || x < 0 || GetBlock(x + position.X, y + position.Y) != 0))
                {
                    return true;
                }
            }
        }
        return false;
    }
}