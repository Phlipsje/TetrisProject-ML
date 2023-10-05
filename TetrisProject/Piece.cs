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
    private byte rotationIndex; //What rotation the piece is currently on (from 0 to 3 for every piece, meaning some pieces have duplicate values)
    private Point position; //Position of top-left of hitbox position
    private Color color; //The color of the piece
    protected Pieces pieceType; //Used for setting block color when piece is locked in
    
    public const int hitboxSize = 4;

    private double nextDropMaxTime; //The time it takes until a piece moves down one line
    private double dropTimer; //The timer counting down checked by nextDropTime
    private double lockDownMaxTime; //The time before a piece is locked into place
    private double lockDownTimer;
    private double softDropMaxTime; //The time for a piece to move down by one cell when holding down
    private double softDropTimer;
    private bool softDropped; //Check if soft dropped this frame
    private bool lockDownTimerSet;
    private int maxMovementCounter;
    private int remainingMovementCounter; //Counts the amount of actions you can perform to extend the timer of the lock down phase
    private int highestHeight; //Check to see if remainingMovementCounter needs to be reset (when reaching new highest height (higher is lower on field))
    private Point previousPosition; //Checks if position of piece changed to decide if timer should actually be reset
    
    private Field fieldReference;

    public double NextDropMaxTime
    {
        get => nextDropMaxTime;
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

    public Piece(Field fieldReference)
    {
        this.fieldReference = fieldReference;
        position = new Point(3, 0);
        hitboxes = new bool[4][,];
        rotationIndex = 0;
        nextDropMaxTime = 0.5; //Test value
        lockDownMaxTime = 0.5;
        maxMovementCounter = 15;
        remainingMovementCounter = maxMovementCounter;
        dropTimer = nextDropMaxTime;
        highestHeight = position.Y;
        
        softDropMaxTime = NextDropMaxTime / 20;
    }

    public void Update(GameTime gameTime)
    {
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        
        //All the piece logic
        PieceControlFlow();

        //Update timers and checks
        previousPosition = position;
        softDropped = false;
        dropTimer -= deltaTime;
        softDropTimer -= deltaTime;
        lockDownTimer -= deltaTime;
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
            dropTimer = nextDropMaxTime;
            MoveDown();
        }
        
        //Checking timer resets and going to Pattern Phase done in MoveDown method
    }

    private void CheckInput()
    {
        if (Util.GetKeyPressed(Keys.A) || Util.GetKeyPressed(Keys.Left))
        {
            MoveLeft();
            ResetLockDownTimer();
        }
        if (Util.GetKeyPressed(Keys.D) || Util.GetKeyPressed(Keys.Right))
        {
            MoveRight();
            ResetLockDownTimer();
        }
        
        //Soft drop
        if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            if (softDropTimer <= 0)
            {
                MoveDown();
                softDropTimer = softDropMaxTime;
                softDropped = true;
            }
        }
        
        if (Util.GetKeyPressed(Keys.R))
        {
            Rotate();
            ResetLockDownTimer();
        }
    }

    private void HardDrop()
    {
        //Iterate down until the place to place the piece is found
        while (!fieldReference.CollidesVertical(Hitbox, position))
        {
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
                }
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
        }
        
        if (fieldReference.CollidesVertical(Hitbox, position))
        {
            if (lockDownTimer <= 0) //Lock Phase has ended
            {
                LockPiece();
                return;
            }

            position.Y--;
        }
    }

    public void MoveLeft()
    {
        position.X--;
        if (fieldReference.CollidesHorizontal(Hitbox, position))
            position.X++;
    }

    public void MoveRight()
    {
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
        rotationIndex++;
        if (rotationIndex == 4)
        {
            rotationIndex = 0;
        }
        
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
        
        //TODO Check if rotation is valid
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
    J
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

        this.Color = Color.Yellow;
        pieceType = Pieces.Block;
    }
}

public class LinePiece : Piece
{
    public LinePiece(Field fieldReference) : base(fieldReference)
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
    public TPiece(Field fieldReference) : base(fieldReference)
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
    public SPiece(Field fieldReference) : base(fieldReference)
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
    public ZPiece(Field fieldReference) : base(fieldReference)
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
    public LPiece(Field fieldReference) : base(fieldReference)
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
    public JPiece(Field fieldReference) : base(fieldReference)
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
#endregion