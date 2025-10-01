using System.Collections.Generic;

namespace TetrisProject;

public struct TetrisGameState
{
    public Field Field { get; private set; }
    public Piece Piece { get; private set; }
    public Piece HeldPiece { get; private set; }
    public List<Pieces> PieceQueue { get; private set; }
    public int Score { get; private set; }

    public TetrisGameState(Field field, Piece piece, Piece heldPiece, List<Pieces> pieceQueue, int score)
    {
        Field = field;
        Piece = piece;
        HeldPiece = heldPiece;
        PieceQueue = pieceQueue;
        Score = score;
    }
}