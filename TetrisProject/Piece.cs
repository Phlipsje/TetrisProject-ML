using System.Reflection.Metadata.Ecma335;

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
    
    public const int hitboxSize = 4;
    
    private Field _fieldReference;

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

    public Piece(Field fieldReference)
    {
        _fieldReference = fieldReference;
        position = new Vector2Int(4, 4);
        hitboxes = new bool[4][,];
    }

    public void MoveDown()
    {
        position.Y++;
        if (_fieldReference.Collides(hitbox, position))
            position.Y--;
    }

    public void MoveLeft()
    {
        position.X--;
        if (_fieldReference.Collides(hitbox, position))
            position.X++;
    }

    public void MoveRight()
    {
        position.X++;
        if (_fieldReference.Collides(hitbox, position))
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
}

#region Piece types
public class BlockPiece : Piece
{
    //All hitbox points for every rotation
    public BlockPiece(Field fieldReference) : base(fieldReference)
    {
        bool[,] hitbox =
        {
            { false, false, false, false },
            { false, true , true , false },
            { false, true , true , false },
            { false, false, false, false}
        };
        
        Hitboxes = new []{hitbox, hitbox, hitbox, hitbox};

        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}

public class LinePiece : Piece
{
    public LinePiece(Field fieldReference) : base(fieldReference)
    {
        //All hitbox points for every rotation
        bool[,] hitbox0 = new[,]
        {
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false }
        };
        bool[,] hitbox1 = new[,]
        {
            { false, false, false, false },
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false }
        };
        bool[,] hitbox2 = new[,]
        {
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false }
        };
        bool[,] hitbox3 = new[,]
        {
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        Hitbox = Hitboxes[0]; //Always start at rotationIndex = 0
    }
}
#endregion