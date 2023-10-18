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

    private enum Direction
    {
        Left,
        Right
    }

    private double autoRepeatStartDelay; //The wait time before auto repeat starts
    private double autoRepeatStartTimer;
    private double autoRepeatDelay; //The wait time between the piece moving one grid cell while holding down left/right
    private double autoRepeatTimer;
    private Direction autoRepeatDirection; 
    private double dropTimer; //The timer counting down checked by nextDropTime
    private double lockDownMaxTime; //The time before a piece is locked into place
    private double lockDownTimer;
    private double softDropMaxTime; //The time for a piece to move down by one cell when holding down
    private double softDropTimer;
    private bool softDropped; //Check if soft dropped this frame
    private bool hardDropped; //Check if piece is being hard dropped
    private int maxMovementCounter;
    private int remainingMovementCounter; //Counts the amount of actions you can perform to extend the timer of the lock down phase
    private int highestHeight; //Check to see if remainingMovementCounter needs to be reset (when reaching new highest height (higher is lower on field))
    private Point previousPosition; //Checks if position of piece changed to decide if lock down timer should actually be reset
    private int previousRotationIndex; //Checks if rotation of piece changed to decide if lock down timer should actually be reset
    private bool firstFrame = true;
    private double[] dropTimes = {1, 0.793, 0.618, 0.473, 0.355, 0.262, 0.190, 0.135, 0.094, 0.064, 0.043, 0.028, 0.018, 0.011, 0.007 };
    private bool lastActionIsRotation; //Check if the last action before locking in is a rotation (to signal the possibility of a t-spin/mini-t-spin)
    private bool lockedDown;
    private Input lastMovementInputPressed;
    
    private Field fieldReference;
    private TetrisGame tetrisGameReference;
    private Controls controls;
    
    #region Rotation types
    private Point[,] normalWallKickLeft = new[,]
    {
        {new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) },
        {new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) },
        {new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, -2) },
        {new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) }
    };
    private Point[,] normalWallKickRight = new[,]
    {
        {new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2) },
        {new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2) },
        {new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2) },
        {new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, -2) }
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

    protected Piece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls)
    {
        this.fieldReference = fieldReference;
        this.tetrisGameReference = tetrisGameReference;
        this.controls = controls;
        position = new Point(3, 0);
        hitboxes = new bool[4][,];
        rotationIndex = 0;
        lockDownMaxTime = 0.5;
        lockDownTimer = lockDownMaxTime;
        autoRepeatDelay = 0.5/fieldReference.Width;
        autoRepeatStartDelay = 0.25;
        maxMovementCounter = 15;
        remainingMovementCounter = maxMovementCounter;
        dropTimer = dropTimes[this.tetrisGameReference.level-1] / tetrisGameReference.gravityMultipler;
        highestHeight = -20;
        
        softDropMaxTime = dropTimer / 20;
    }

    public void Update(GameTime gameTime)
    {
        //Get time to load previous frame in seconds
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

        //Check if player has lost
        if (firstFrame && fieldReference.CollidesVertical(Hitbox, position))
        {
            //If block spawn in an occupied space, game over
            fieldReference.GameOver(); 
        }
        //It is not the first frame that a block has spawned in anymore
        firstFrame = false;

        //All the piece logic
        PieceControlFlow();

        //Update timers and checks
        previousPosition = position;
        previousRotationIndex = rotationIndex;
        softDropped = false;
        dropTimer -= deltaTime; //Time for piece to fall down 1 row by itself due to gravity
        softDropTimer -= deltaTime; //Time between dropping down another line while holding down the soft drop button
        lockDownTimer -= deltaTime; //To check if a piece should be locked into place
        autoRepeatStartTimer -= deltaTime; //For moving left and right
        if (autoRepeatStartTimer <= 0)
        {
            autoRepeatTimer -= deltaTime;
        }
    }

    private void PieceControlFlow()
    {
        //Check if piece needs to be locked down
        if (lockDownTimer <= 0)
        {
            CheckForLockDown();
        }

        //Piece is now inactive
        if (lockedDown)
        {
            return;
        }
        
        //Falling Phase
        CheckInput();
        
        //Check if the last input was a legal rotation
        if (position != previousPosition)
        {
            lastActionIsRotation = false;
        }
        if(rotationIndex != previousRotationIndex)
        {
            lastActionIsRotation = true;
        }

        //Lock Phase (That half a second before piece is fully in place)
        if (dropTimer <= 0 && !softDropped && !hardDropped) //Piece drops down 1 line
        {
            dropTimer = dropTimes[tetrisGameReference.level-1];
            softDropMaxTime = dropTimer / 20;
            MoveDown();
        }
        
        //Checking timer resets and going to Pattern Phase done in MoveDown method
    }

    private void CheckInput()
    {
        //Update lastMovementInputPressed
        if(Util.GetKeyPressed(Input.Left, controls))
            lastMovementInputPressed = Input.Left;
        else if (Util.GetKeyPressed(Input.Right, controls))
            lastMovementInputPressed = Input.Right;

        if (Util.GetKeyHeld(Input.Left, controls) && !Util.GetKeyHeld(Input.Right, controls))
            lastMovementInputPressed = Input.Left;
        else if (Util.GetKeyHeld(Input.Right, controls) && !Util.GetKeyHeld(Input.Left, controls))
            lastMovementInputPressed = Input.Right;
        
        //Hold piece
        if (Util.GetKeyPressed(Input.Hold, controls))
        {
            fieldReference.HoldPiece(this);
        }
        
        //Move left
        if (Util.GetKeyHeld(Input.Left, controls) && lastMovementInputPressed == Input.Left)
        {
            if (autoRepeatDirection == Direction.Right)
            {
                autoRepeatDirection = Direction.Left;
                autoRepeatTimer = autoRepeatDelay;
                autoRepeatStartTimer = autoRepeatStartDelay;
                MoveLeft();
                ResetLockDownTimer();
            }
            else if ((autoRepeatTimer <= 0 && autoRepeatStartTimer <= 0)  || Util.GetKeyPressed(Input.Left, controls))
            {
                MoveLeft();
                ResetLockDownTimer();
                autoRepeatTimer = autoRepeatDelay;
            }
        }
        
        //Move right
        if (Util.GetKeyHeld(Input.Right, controls) && lastMovementInputPressed == Input.Right)
        {
            if (autoRepeatDirection == Direction.Left)
            {
                autoRepeatDirection = Direction.Right;
                autoRepeatTimer = autoRepeatDelay;
                autoRepeatStartTimer = autoRepeatStartDelay;
                MoveRight();
                ResetLockDownTimer();
            }
            else if ((autoRepeatTimer <= 0 && autoRepeatStartTimer <= 0) || Util.GetKeyPressed(Input.Right, controls))
            {
                MoveRight();
                ResetLockDownTimer();
                autoRepeatTimer = autoRepeatDelay;
            }
        }
        
        //Soft drop
        if (Util.GetKeyHeld(Input.SoftDrop, controls) && softDropTimer <= 0)
        {
            softDropTimer = softDropMaxTime;
            softDropped = true; 
            MoveDown();
        }
        
        //Counterclockwise rotation
        if (Util.GetKeyPressed(Input.RotateCounterClockWise, controls))
        {
            RotateCounterClockWise();
            ResetLockDownTimer();
        }
        
        //Clockwise rotation
        if (Util.GetKeyPressed(Input.RotateClockWise, controls))
        {
            RotateClockWise();
            ResetLockDownTimer();
        }
        
        //Check for hard drop
        if (Util.GetKeyPressed(Input.HardDrop, controls))
        {
            HardDrop();
            return;
        }

        //Check if counter movement is given in the horizontal axis
        if ((Util.GetKeyLetGo(Input.Left, controls) && !Util.GetKeyHeld(Input.Right, controls)) || (Util.GetKeyLetGo(Input.Right, controls) && !Util.GetKeyHeld(Input.Left, controls)))
        {
            autoRepeatStartTimer = autoRepeatStartDelay;
        }

        //Check if started moving in the horizontal axis
        if (Util.GetKeyPressed(Input.Left, controls) || Util.GetKeyPressed(Input.Right, controls))
        {
            autoRepeatStartTimer = autoRepeatStartDelay;
        }
    }

    //Check if lock down is in effect due to 0.5 seconds on delay being over
    private void CheckForLockDown()
    {
        //Move piece down by one to check for possible collisions
        position.Y++;
        
        //Check if piece is in lock phase
        if (fieldReference.CollidesVertical(Hitbox, position))
        {
            LockPiece();
            lockedDown = true;
        }
        else //There is still room for piece to fall
        {
            //Revert to original position
            position.Y--;
        }
    }

    private void HardDrop()
    {
        //Iterate down until the place to place the piece is found
        hardDropped = true;
        lastActionIsRotation = false;
        while (!fieldReference.CollidesVertical(Hitbox, Position))
        {
            position.Y++;
            tetrisGameReference.score += 2; //Increase score by 2 per grid line that is dropped by hard dropping
        }

        //Correct for piece now being stuck in block
        position.Y--;
        
        //Final move down to trigger lock piece
        MoveDown();
    }

    private void LockPiece()
    {
        lockedDown = true;
        bool anyBlockBelowSkyline = false;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (Hitbox[x, y])
                {
                    fieldReference.SetBlock(position.X + x,position.Y - y - 1,pieceType);
                    if (position.Y - y - 1 >= 0)
                        anyBlockBelowSkyline = true;
                }
            }
        }

        if (!anyBlockBelowSkyline)
        {
            fieldReference.GameOver();
            return;
        }

        SfxManager.Play(SfxManager.LockPiece);

        //Check for T-spins
        if (pieceType == Pieces.T && lastActionIsRotation)
        {
            //A, B, C and D are corners around t-piece, visualization can be found in Tetris Guidelines
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
        if (remainingMovementCounter > 0 && (previousPosition != position || rotationIndex != previousRotationIndex))
        {
            remainingMovementCounter--;
            lockDownTimer = lockDownMaxTime;
        }
    }

    //Used to get the point closest to the bottom of the grid (used for lock down timer reset calculation)
    private int GetHighestYPoint()
    {
        int highestY = 0;
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (Hitbox[i, j] && j > highestY)
                {
                    highestY = j;
                }
            }
        }

        return position.Y - highestY;
    }
    
    private void MoveDown()
    {
        position.Y++;
        
        //Check if new highest position to reset remaining movement counter
        if (GetHighestYPoint() > highestHeight)
        {
            highestHeight = GetHighestYPoint();
            remainingMovementCounter = maxMovementCounter;
            lockDownTimer = lockDownMaxTime;
        }
        
        //Check if the piece has entered an illegal position
        if (fieldReference.CollidesVertical(Hitbox, position))
        {
            //Check if lock Phase has ended
            if (lockDownTimer <= 0 || hardDropped) 
            {
                LockPiece();
                return;
            }

            //Move the piece back if it landed in a block
            position.Y--;
        }
        
        //Only updates if this was not the frame that the piece was locked (otherwise this check is always false)
        lastActionIsRotation = false; 
        
        //Increase score by 1 for each grid line dropped by soft dropping
        if (softDropped && GetHighestYPoint() == highestHeight)
        {
            tetrisGameReference.score++;
        }
    }

    private void MoveLeft()
    {
        position.X--;
        if (fieldReference.CollidesHorizontal(Hitbox, position))
            position.X++;
    }

    private void MoveRight()
    {
        position.X++;
        if (fieldReference.CollidesHorizontal(Hitbox, position))
            position.X--;
    }
    
    //Rotate a piece clockwise
    private void RotateClockWise()
    {
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
    private void RotateCounterClockWise()
    {
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
    Ghost,
    Garbage
}

#region Piece types
public class BlockPiece : Piece
{
    //All hitbox points for every rotation
    public BlockPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public LinePiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public TPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public SPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public ZPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public LPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public JPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls) : base(fieldReference, tetrisGameReference, controls)
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
    public GhostPiece(Field fieldReference, TetrisGame tetrisGameReference, Controls controls, Piece piece) : base(fieldReference, tetrisGameReference, controls)
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