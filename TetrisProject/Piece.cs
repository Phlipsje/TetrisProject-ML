namespace TetrisProject;

public struct Vector2int
{
    public Vector2int(int x, int y)
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

    public bool[,] Hitbox;
    public Vector2int Position;
    public const int HitboxSize = 4;
    private Field _fieldReference;

    public Piece(Field fieldReference)
    {
        _fieldReference = fieldReference;
        Position = new Vector2int(4, 4);
    }

    public void MoveDown()
    {
        Position.Y++;
        if (_fieldReference.Collides(Hitbox, Position))
            Position.Y--;
    }

    public void MoveLeft()
    {
        Position.X--;
        if (_fieldReference.Collides(Hitbox, Position))
            Position.X++;
    }

    public void MoveRight()
    {
        Position.X++;
        if (_fieldReference.Collides(Hitbox, Position))
            Position.X--;
    }

}

public class BlockPiece : Piece
{
    public BlockPiece(Field fieldReference) : base(fieldReference)
    {
        Hitbox = new bool[, ]
        {
            { false, false, false, false },
            { false, true , true , false },
            { false, true , true , false },
            { false, false, false, false}
        };
    }
}