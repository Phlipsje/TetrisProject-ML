using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public struct Vector2Int
{
    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X;
    public int Y;
    public override string ToString() => $"({X}, {Y})";
}
public abstract class Piece
{
    //A piece that can be placed
    //All pieces are 4 blocks
    //Types: l-, r-, s-, z-, t-, block and line

    private bool[,] hitbox; //Current hitbox so you don't need to keep accessing arrays with indexes
    private bool[][,] hitboxes; //Contains the hitbox for every rotation
    private byte rotationIndex; //What rotation the piece is currently on (from 0 to 3 for every piece, meaning some pieces have duplicate values)
    private Vector2Int position; //Position of top-left of hitbox position
    private Color color; //The color of the piece
    protected Pieces pieceType; //Used for setting block color when piece is locked in
    
    public const int hitboxSize = 4;

    private double nextDropTime; //The time it takes until a piece moves down one line
    private double dropTimer; //The timer counting up checked by nextDropTime
    
    private Field fieldReference;
    private TetrisGame tetrisGameReference;

    public bool[,] Hitbox
    {
        get => hitbox;
        set => hitbox = value;
    }

    public bool[][,] Hitboxes
    {
        get => hitboxes;
        set => hitboxes = value;
    }

    public byte RotationIndex
    {
        get => rotationIndex;
    }

    public Vector2Int Position
    {
        get => position;
        set => position = value;
    }

    public Color Color
    {
        get => color;
        protected set => color = value;
    }

    public Piece(Field fieldReference, TetrisGame tetrisGameReference)
    {
        this.fieldReference = fieldReference;
        this.tetrisGameReference = tetrisGameReference;
        position = new Vector2Int(4, 0);
        hitboxes = new bool[4][,];
        nextDropTime = 0.5; //Test value
    }

    public void Update(GameTime gameTime)
    {
        if (Util.GetKeyPressed(Keys.R))
        {
            Rotate();
        }

        if (dropTimer > nextDropTime) //Piece drops down 1 line
        {
            dropTimer = 0;
            MoveDown();
        }

        if (fieldReference.Collides(Hitbox, position))
        {
            LockPiece(); //Change to give wiggle room later
        }

        dropTimer += gameTime.ElapsedGameTime.TotalSeconds;
    }

    private void LockPiece()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (Hitbox[x, y])
                {
                    fieldReference.SetBlock(position.X + x,position.Y + y,(byte)((int)pieceType + 1));
                }
            }
        }
        
        tetrisGameReference.NextPiece();
    }
    
    public void MoveDown()
    {
        position.Y++;
        //if (fieldReference.Collides(hitbox, position))
          //  position.Y--;
    }

    public void MoveLeft()
    {
        position.X--;
        if (fieldReference.Collides(hitbox, position))
            position.X++;
    }

    public void MoveRight()
    {
        position.X++;
        if (fieldReference.Collides(hitbox, position))
            position.X--;
    }

    //Default rotation (clockwise)
    public void Rotate()
    {
        RotateClockWise();
    }
    
    //Rotate a piece clockwise
    public void RotateClockWise()
    {
        rotationIndex++;
        if (rotationIndex == 4)
        {
            rotationIndex = 0;
        }
        Hitbox = Hitboxes[rotationIndex];
        
        //TODO Check if rotation is valid
    }

    //Rotate a piece counterclockwise
    public void RotateCounterClockWise()
    {
        //Needs different operation order than clockwise because rotationIndex is of type byte and can't be negative
        if (rotationIndex == 0)
        {
            rotationIndex = 4;
        }
        rotationIndex--;
        
        Hitbox = Hitboxes[rotationIndex];
        
        //TODO Check if rotation is valid
    }

    //Gets the value of the next piece and creates the corresponding object
    public Piece GetNextPiece(byte pieceInQueue)
    {
        Piece blockType;
        switch (pieceInQueue)
        {
            case (byte)Pieces.Block:
                blockType = new BlockPiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.Line:
                blockType = new LinePiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.T:
                blockType = new TPiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.S:
                blockType = new SPiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.Z:
                blockType = new ZPiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.L:
                blockType = new LPiece(fieldReference, tetrisGameReference);
                break;
            case (byte)Pieces.J:
                blockType = new JPiece(fieldReference, tetrisGameReference);
                break;
            default:
                throw new Exception("blockType not specified");

        }

        return blockType;
    }
}

public enum Pieces
{
    Block,
    Line,
    T,
    S,
    Z,
    L,
    J
}

#region Piece types
public class BlockPiece : Piece
{
    //All hitbox points for every rotation
    public BlockPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        bool[,] hitbox =
        {
            { false, false, false, false },
            { false, true , true , false },
            { false, true , true , false },
            { false, false, false, false}
        };
        
        Hitboxes = new []{hitbox, hitbox, hitbox, hitbox};

        this.Color = Color.Yellow;
        pieceType = Pieces.Block;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class LinePiece : Piece
{
    public LinePiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.LightBlue;
        pieceType = Pieces.Line;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class TPiece : Piece
{
    public TPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { true, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, true, false, false },
            { false, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, false, false, false },
            { true, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, true, false, false },
            { true, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Purple;
        pieceType = Pieces.T;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class SPiece : Piece
{
    public SPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { false, true, true, false },
            { true, true, false, false },
            { false, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, true, false, false },
            { false, true, true, false },
            { false, false, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, false, false, false },
            { false, true, true, false },
            { true, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { true, false, false, false },
            { true, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.LightGreen;
        pieceType = Pieces.S;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class ZPiece : Piece
{
    public ZPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { true, true, false, false },
            { false, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, true, false },
            { false, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, false, false, false },
            { true, true, false, false },
            { false, true, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, true, false, false },
            { true, true, false, false },
            { true, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Red;
        pieceType = Pieces.Z;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class LPiece : Piece
{
    public LPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { false, true, false, false },
            { false, true, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { true, true, true, false },
            { true, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { true, true, false, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, false, true, false },
            { true, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Orange;
        pieceType = Pieces.L;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class JPiece : Piece
{
    public JPiece(Field fieldReference, TetrisGame tetrisGame) : base(fieldReference, tetrisGame)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { false, true, false, false },
            { true, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { true, false, false, false },
            { true, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, true, true, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, false, false, false },
            { true, true, true, false },
            { false, false, true, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Blue;
        pieceType = Pieces.J;
        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}
#endregion