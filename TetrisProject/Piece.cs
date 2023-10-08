using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public abstract class Piece
{
    //A piece that can be placed
    //All pieces are 4 blocks
    //Types: l-, r-, s-, z-, t-, block and line
    
    private bool[][,] hitboxes; //Contains the hitbox for every rotation
    protected byte rotationIndex; //What rotation the piece is currently on (from 0 to 3 for every piece, meaning some pieces have duplicate values)
    private Point position; //Position of top-left of hitbox position
    private Color color; //The color of the piece
    protected Pieces pieceType; //Used for setting block color when piece is locked in
    
    public const int hitboxSize = 4;

    private double autoRepeatStartDelay; //The wait time before auto repeat starts
    private double autoRepeatStartTimer;
    private double autoRepeatDelay; //The wait time between the piece moving one grid cell while holding down left/right
    private double autoRepeatTimer;
    private double dropTimer; //The timer counting down checked by nextDropTime
    private double lockDownMaxTime; //The time before a piece is locked into place
    private double lockDownTimer;
    private double softDropMaxTime; //The time for a piece to move down by one cell when holding down
    private double softDropTimer;
    private bool softDropped; //Check if soft dropped this frame
    private bool hardDropped; //Check if piece is being hard dropped
    private bool lockDownTimerSet;
    private int maxMovementCounter;
    private int remainingMovementCounter; //Counts the amount of actions you can perform to extend the timer of the lock down phase
    private int highestHeight; //Check to see if remainingMovementCounter needs to be reset (when reaching new highest height (higher is lower on field))
    private Point previousPosition; //Checks if position of piece changed to decide if timer should actually be reset
    private bool firstFrame = true;
    private double[] dropTimes = new []{1, 0.793, 0.618, 0.473, 0.355, 0.262, 0.190, 0.135, 0.094, 0.064, 0.043, 0.028, 0.018, 0.011, 0.007 };
    private bool lastActionIsRotation = false; //Check if the last action before locking in is a rotation (to signal the possibility of a t-spin/mini-t-spin)
    
    private Field fieldReference;
    private TetrisGame tetrisGameReference;
    
    #region Rotation types
    private Point[,] normalWallKickLeft = new[,]
    {
        {new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) },
        {new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) },
        {new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, 2) },
        {new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) }
    };
    private Point[,] normalWallKickRight = new[,]
    {
        {new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2) },
        {new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) },
        {new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) },
        {new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, 2) }
    };
    private Point[,] lineWallKickLeft = new[,]
    {
        {new Point(2, 0), new Point(-1, 0), new Point(2, 1), new Point(-1, -2) },
        {new Point(2, 0), new Point(-2, 0), new Point(1, -2), new Point(-2, 1) },
        {new Point(-2, 0), new Point(1, 0), new Point(-2, -1), new Point(1, 2) },
        {new Point(-2, 0), new Point(2, 0), new Point(-1, 2), new Point(2, -1) }
    };
    private Point[,] lineWallKickRight = new[,]
    {
        {new Point(-2, 0), new Point(1, 0), new Point(-2, -1), new Point(1, 2) },
        {new Point(-2, 0), new Point(2, 0), new Point(-1, 2), new Point(2, -1) },
        {new Point(2, 0), new Point(-1, 0), new Point(2, 1), new Point(-1, -2) },
        {new Point(2, 0), new Point(-2, 0), new Point(1, -2), new Point(-2, 1) }
    };
    #endregion

    public byte RotationIndex
    {
        get { return rotationIndex; }
        set { rotationIndex = value; }
    }
    
    public bool[,] Hitbox
    {
        get => hitboxes[rotationIndex];
    }

    public bool[][,] Hitboxes
    {
        get => hitboxes;
        set => hitboxes = value;
    }

    public Point Position
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
        position = new Point(3, 0);
        hitboxes = new bool[4][,];
        rotationIndex = 0;
        lockDownMaxTime = 0.5;
        autoRepeatDelay = 0.5/fieldReference.Width;
        autoRepeatStartDelay = 0.25;
        maxMovementCounter = 15;
        remainingMovementCounter = maxMovementCounter;
        dropTimer = dropTimes[this.tetrisGameReference.level-1];
        highestHeight = position.Y;
        
        softDropMaxTime = dropTimer / 20;
    }

    public void Update(GameTime gameTime)
    {
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        if (firstFrame && fieldReference.CollidesVertical(Hitbox, position))
            fieldReference.GameOver(); // if block spawn in an occupied space, game over
        firstFrame = false;
        
        //All the piece logic
        PieceControlFlow();

        //Update timers and checks
        previousPosition = position;
        softDropped = false;
        dropTimer -= deltaTime;
        softDropTimer -= deltaTime;
        lockDownTimer -= deltaTime;
        autoRepeatStartTimer -= deltaTime;
        if (autoRepeatStartTimer <= 0)
        {
            autoRepeatTimer -= deltaTime;
        }
    }

    private void PieceControlFlow()
    {
        //Falling Phase
        CheckInput();
        
        //Check for hard drop
        if (Util.GetKeyPressed(Keys.Space))
        {
            HardDrop();
            return;
        }
        
        //Lock Phase (That half a second before piece is fully in place)
        if (dropTimer <= 0 && !softDropped) //Piece drops down 1 line
        {
            dropTimer = dropTimes[tetrisGameReference.level-1];
            softDropMaxTime = dropTimer / 20;
            MoveDown();
        }
        
        //Checking timer resets and going to Pattern Phase done in MoveDown method
    }

    private void CheckInput()
    {
        if (Util.GetKeyPressed(Keys.LeftShift))
        {
            fieldReference.HoldPiece(this);
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            if ((autoRepeatTimer <= 0 && autoRepeatStartTimer <= 0)  || Util.GetKeyPressed(Keys.Left))
            {
                MoveLeft();
                ResetLockDownTimer();
                autoRepeatTimer = autoRepeatDelay;
            }
            
        }
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            if ((autoRepeatTimer <= 0 && autoRepeatStartTimer <= 0) || Util.GetKeyPressed(Keys.Right))
            {
                MoveRight();
                ResetLockDownTimer();
                autoRepeatTimer = autoRepeatDelay;
            }
        }
        
        //Soft drop
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            if (softDropTimer <= 0)
            {
                softDropTimer = softDropMaxTime;
                softDropped = true;
                MoveDown();
            }
        }
        
        if (Util.GetKeyPressed(Keys.Up))
        {
            Rotate();
            ResetLockDownTimer();
        }

        if ((Util.GetKeyLetGo(Keys.Left) && !Util.GetKeyHeld(Keys.Right)) || (Util.GetKeyLetGo(Keys.Right) && !Util.GetKeyHeld(Keys.Left)))
        {
            autoRepeatStartTimer = autoRepeatStartDelay;
        }

        if (Util.GetKeyPressed(Keys.Left) || Util.GetKeyPressed(Keys.Right))
        {
            autoRepeatStartTimer = autoRepeatStartDelay;
        }
    }

    private void HardDrop()
    {
        //Iterate down until the place to place the piece is found
        hardDropped = true;
        lastActionIsRotation = false;
        while (!fieldReference.CollidesVertical(Hitbox, Position))
        {
            tetrisGameReference.Score += 2; //Increase score by 2 per grid line that is dropped by hard dropping
            MoveDown();
        }
    }

    private void LockPiece()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (Hitbox[x, y])
                {
                    fieldReference.SetBlock(position.X + x,position.Y - y - 1,pieceType);
                    if (position.Y - y - 1 < 0)
                        fieldReference.GameOver(); // if block is locked down above skyline the game is over
                }
            }
        }
        SfxManager.Play(SfxManager.LockPiece);
        
        //Check for T-spins
        if (pieceType == Pieces.T && lastActionIsRotation)
        {
            bool checkA = false;
            bool checkB = false;
            bool checkC = false;
            bool checkD = false;

            switch (rotationIndex)
            {
                case 0: //Facing top
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 3)) checkA = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 3)) checkB = true;
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 1)) checkC = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 1)) checkD = true;
                    break;
                case 1: //Facing right
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 3)) checkC = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 3)) checkA = true;
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 1)) checkD = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 1)) checkB = true;
                    break;
                case 2: //Facing bottom
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 3)) checkD = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 3)) checkC = true;
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 1)) checkB = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 1)) checkA = true;
                    break;
                case 3: //Facing left
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 3)) checkB = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 3)) checkD = true;
                    if (fieldReference.TSpinCheck(position.X, Position.Y - 1)) checkA = true;
                    if (fieldReference.TSpinCheck(position.X+2, Position.Y - 1)) checkC = true;
                    break;
            }

            if (checkA && checkB && (checkC || checkD)) //T-spin requirements
            {
                fieldReference.tSpin = true;
            }
            else if ((checkA || checkB) && checkC && checkD) //Mini-T-spin requirements
            {
                fieldReference.miniTSpin = true;
            }
        }
        
        fieldReference.FieldControlFlow(); //Pattern Phase
    }

    private void ResetLockDownTimer()
    {
        if (remainingMovementCounter > 0 && previousPosition != position)
        {
            remainingMovementCounter--;
            lockDownTimer = lockDownMaxTime;
            
            if(remainingMovementCounter == 0) //Lock piece if counter can't be lowered anymore
            {
                position.Y++;
                LockPiece();
            }
        }
    }
    
    public void MoveDown()
    {
        position.Y++;
        
        if (position.Y > highestHeight) //Check if new highest position to reset remaining movement counter
        {
            highestHeight = position.Y;
            remainingMovementCounter = maxMovementCounter;
            lockDownTimer = lockDownMaxTime;
        }
        
        if (fieldReference.CollidesVertical(Hitbox, position))
        {
            if (lockDownTimer <= 0 || hardDropped) //Lock Phase has ended
            {
                LockPiece();
                return;
            }

            position.Y--; //Move the piece back if it landed in a block
        }
        
        lastActionIsRotation = false; //Only updates if this was not the frame that the piece was locked (otherwise this check is always false)
        
        //Increase score by 1 for each grid line dropped by soft dropping
        if (softDropped)
        {
            tetrisGameReference.Score++;
        }
    }

    public void MoveLeft()
    {
        lastActionIsRotation = false;
        position.X--;
        if (fieldReference.CollidesHorizontal(Hitbox, position))
            position.X++;
    }

    public void MoveRight()
    {
        lastActionIsRotation = false;
        position.X++;
        if (fieldReference.CollidesHorizontal(Hitbox, position))
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
        lastActionIsRotation = true;
        int previousRotation = rotationIndex;
        rotationIndex++;
        if (rotationIndex == 4)
        {
            rotationIndex = 0;
        }

        //Check if normal rotation is valid
        if (!fieldReference.CollidesHorizontal(Hitbox, position) && !fieldReference.CollidesVertical(Hitbox,position))
        {
            return;
        }

        if (pieceType != Pieces.Line)
        {
            //Check if there is a different valid position for the rotation according to the super rotation system
            for (int i = 0; i < 4; i++)
            {
                bool collideHorizontal = fieldReference.CollidesHorizontal(Hitbox, position + normalWallKickRight[previousRotation, i]);
                bool collideVertical = fieldReference.CollidesVertical(Hitbox, position + normalWallKickRight[previousRotation, i]);
                if (!collideHorizontal && !collideVertical)
                {
                    lastActionIsRotation = false;
                    position += normalWallKickRight[previousRotation, i];
                    return;
                }

                if (i == 3 && pieceType == Pieces.T) //Rotation through point 5 with a T-piece is a t-spin under Tetris Guidelines
                {
                    fieldReference.tSpin = true;
                }
            }
        }
        else //Different type of rotation offset if a line piece
        {
            //Check if there is a different valid position for the rotation according to the super rotation system
            for (int i = 0; i < 4; i++)
            {
                bool collideHorizontal = fieldReference.CollidesHorizontal(Hitbox, position + lineWallKickRight[previousRotation, i]);
                bool collideVertical = fieldReference.CollidesVertical(Hitbox, position + lineWallKickRight[previousRotation, i]);
                if (!collideHorizontal && !collideVertical)
                {
                    position += lineWallKickRight[previousRotation, i];
                    return;
                }
            }
        }
        
        //Rotate back to original position if there is no legal rotation
        RotateCounterClockWise();
    }

    //Rotate a piece counterclockwise
    public void RotateCounterClockWise()
    {
        lastActionIsRotation = true;
        int previousRotation = rotationIndex;
        //Needs different operation order than clockwise because rotationIndex is of type byte and can't be negative
        if (rotationIndex == 0)
        {
            rotationIndex = 4;
        }
        rotationIndex--;
        
        //Check if normal rotation is valid
        if (!fieldReference.CollidesHorizontal(Hitbox, position) && !fieldReference.CollidesVertical(Hitbox,position))
        {
            return;
        }
        
        if (pieceType != Pieces.Line)
        {
            //Check if there is a different valid position for the rotation according to the super rotation system
            for (int i = 0; i < 4; i++)
            {
                bool collideHorizontal = fieldReference.CollidesHorizontal(Hitbox, position + normalWallKickLeft[previousRotation, i]);
                bool collideVertical = fieldReference.CollidesVertical(Hitbox, position + normalWallKickLeft[previousRotation, i]);
                if (!collideHorizontal && !collideVertical)
                {
                    lastActionIsRotation = false;
                    position += normalWallKickLeft[previousRotation, i];
                    return;
                }
                
                if (i == 3 && pieceType == Pieces.T) //Rotation through point 5 with a T-piece is a t-spin under Tetris Guidelines
                {
                    fieldReference.tSpin = true;
                }
            }
        }
        else //Different type of rotation offset if a line piece
        {
            //Check if there is a different valid position for the rotation according to the super rotation system
            for (int i = 0; i < 4; i++)
            {
                bool collideHorizontal = fieldReference.CollidesHorizontal(Hitbox, position + lineWallKickLeft[previousRotation, i]);
                bool collideVertical = fieldReference.CollidesVertical(Hitbox, position + lineWallKickLeft[previousRotation, i]);
                if (!collideHorizontal && !collideVertical)
                {
                    position += lineWallKickLeft[previousRotation, i];
                    return;
                }
            }
        }
        
        //Rotate back to original position if there is no legal rotation
        RotateClockWise();
    }
}

public enum Pieces
{
    None,
    Block,
    Line,
    T,
    S,
    Z,
    L,
    J,
    Ghost
}

#region Piece types
public class BlockPiece : Piece
{
    //All hitbox points for every rotation
    public BlockPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
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
    }
}

public class LinePiece : Piece
{
    public LinePiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, true, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false },
            { false, false, true, false }
        };
        bool[,] hitbox3 =
        {
            { false, false, false, false },
            { false, false, false, false },
            { true, true, true, true },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.LightBlue;
        pieceType = Pieces.Line;
    }
}

public class TPiece : Piece
{
    public TPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { false, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { true, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, true, false, false },
            { true, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, true, false, false },
            { true, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Purple;
        pieceType = Pieces.T;
    }
}

public class SPiece : Piece
{
    public SPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
        bool[,] hitbox0 =
        {
            { false, true, false, false },
            { false, true, true, false },
            { false, false, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { false, true, true, false },
            { true, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { true, false, false, false },
            { true, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { false, true, true, false },
            { true, true, false, false },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.LightGreen;
        pieceType = Pieces.S;
    }
}

public class ZPiece : Piece
{
    public ZPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
        bool[,] hitbox0 =
        {
            { false, false, true, false },
            { false, true, true, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { true, true, false, false },
            { false, true, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, true, false, false },
            { true, true, false, false },
            { true, false, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { true, true, false, false },
            { false, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Red;
        pieceType = Pieces.Z;
    }
}

public class LPiece : Piece
{
    public LPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
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
    }
}

public class JPiece : Piece
{
    public JPiece(Field fieldReference, TetrisGame tetrisGameReference) : base(fieldReference, tetrisGameReference)
    {
        //All hitbox points for every rotation
        //A single row is the value of the 2nd index, not the first! (The structure does not indicate an x and y position)
        bool[,] hitbox0 =
        {
            { false, true, true, false },
            { false, true, false, false },
            { false, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox1 =
        {
            { false, false, false, false },
            { true, true, true, false },
            { false, false, true, false },
            { false, false, false, false }
        };
        bool[,] hitbox2 =
        {
            { false, true, false, false },
            { false, true, false, false },
            { true, true, false, false },
            { false, false, false, false }
        };
        bool[,] hitbox3 =
        {
            { true, false, false, false },
            { true, true, true, false },
            { false, false, false, false },
            { false, false, false, false }
        };

        Hitboxes = new[] { hitbox0, hitbox1, hitbox2, hitbox3 };

        this.Color = Color.Blue;
        pieceType = Pieces.J;
    }
}

public class GhostPiece : Piece
{
    public GhostPiece(Field fieldReference, TetrisGame tetrisGameReference, Piece piece) : base(fieldReference, tetrisGameReference)
    {
        Piece pieceReference = piece;
        Position = piece.Position;
        Hitboxes = pieceReference.Hitboxes;
        rotationIndex = pieceReference.RotationIndex;
        Color = pieceReference.Color * 0.25f;
        pieceType = Pieces.Ghost;
    
        while (!fieldReference.CollidesVertical(Hitbox, Position))
            Position = new Point(Position.X, Position.Y + 1);
        Position = new Point(Position.X, Position.Y - 1);
    }
}
#endregion