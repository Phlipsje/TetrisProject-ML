using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TetrisProject; 

public class VersusHandler : GameHandler
{
    //A garbage line is the gray line added at the bottom of the opponents field when you clear a line
    private readonly double garbageMultiplier;
    private List<Pieces[]> garbageLines0 = new ();
    private List<Pieces[]> garbageLines1 = new ();
    
    public VersusHandler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls, Main mainRefrence) : base(content, gameMode, settings, controls, mainRefrence)
    {
        tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode, 1, false));
        tetrisGames.Add(new TetrisGame(this, settings, controls[1], gameMode, 2, false));

        garbageMultiplier = settings.game.garbageMultiplier;
    }

    //Runs when one of 2 players cleared a line
    public override void LineCleared(int linesCleared, int multiplayerLinesCleared, int instance)
    {
        //If no line is cleared then return
        if (multiplayerLinesCleared == 0)
        {
            //Update receive bar values
            tetrisGames[0].blocksBeingAdded = garbageLines0.Count;
            tetrisGames[1].blocksBeingAdded = garbageLines1.Count;
            
            return;
        }

        //Multiply the lines cleared by the multiplier defined in settings
        multiplayerLinesCleared = (int)MathF.Floor(multiplayerLinesCleared * (float)garbageMultiplier);
        
        //Create garbage line
        //Same garbage line is used for all lines sent by one piece placed
        Pieces[] garbageLine = new Pieces[tetrisGames[0].Field.Width];
        garbageLine[0] = Pieces.None;
        for (int i = 1; i < garbageLine.Length; i++)
        {
            garbageLine[i] = Pieces.Garbage;
        }

        //Place the hole in the garbage line in a random spot
        garbageLine = Util.ShuffleArray(garbageLine);

        //Remove lines from your garbage lines list is you cleared a line
        if (instance == 1)
        {
            int count = multiplayerLinesCleared;
            for (int i = 0; i < count; i++)
            {
                //Only remove if you have garbage filled up
                if (garbageLines0.Count > 0)
                {
                    garbageLines0.RemoveAt(garbageLines0.Count-1);
                    
                    //Lines removed from own garbage pile are not sent to opponent
                    multiplayerLinesCleared--;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            //Only remove if you have garbage filled up
            int count = multiplayerLinesCleared;
            for (int i = 0; i < count; i++)
            {
                if (garbageLines1.Count > 0)
                {
                    garbageLines1.RemoveAt(garbageLines1.Count-1);
                    
                    //Lines removed from own garbage pile are not sent to opponent
                    multiplayerLinesCleared--;
                }
                else
                {
                    break;
                }
            }
        }

        //instance 1 targets 2 and instance 2 targets 1
        //instances listed as 0 and 1, bit of a poor choice of naming, but is accounted for when converting to target
        int target = (int)MathF.Abs(instance - 2);
        
        //Add to list of garbage lines
        if (target == 0)
        {
            for (int i = 0; i < multiplayerLinesCleared; i++)
            {
                //Can not have more than 20 lines being sent
                if (garbageLines0.Count < 20)
                {
                    garbageLines0.Add(garbageLine);
                }
            }
        }
        else
        {
            for (int i = 0; i < multiplayerLinesCleared; i++)
            {
                //Can not have more than 20 lines being sent
                if (garbageLines1.Count < 20)
                {
                    garbageLines1.Add(garbageLine);
                }
            }
        }
        
        //Update receive bar values
        tetrisGames[0].blocksBeingAdded = garbageLines0.Count;
        tetrisGames[1].blocksBeingAdded = garbageLines1.Count;
    }

    public override void PiecePlaced(int instance)
    {
        //Again bad instance naming results in changing values
        AddLine(instance-1);
    }

    //Instance is instance to send to
    private void AddLine(int instance)
    {
        //Garbage lines is equal to the garbage lines of the correct instance
        List<Pieces[]> garbageLines = instance == 0 ? garbageLines0 : garbageLines1;
        
        //Check how many lines to add (there is a maximum)
        int linesToAdd = garbageLines.Count;
        if (linesToAdd > 8)
        {
            linesToAdd = 8;
        }
        
        //Return if no lines to add
        if (linesToAdd == 0)
        {
            return;
        }
        
        //Add line itself
        for (int i = 0; i < tetrisGames[instance].Field.blockArray.Length; i++)
        {
            //Move line up by one
            if (i + linesToAdd < tetrisGames[instance].Field.blockArray.Length)
            {
                tetrisGames[instance].Field.blockArray[i] = tetrisGames[instance].Field.blockArray[i + linesToAdd];
            }
            else //Add actual garbage line
            {
                tetrisGames[instance].Field.blockArray[i] = garbageLines[^1]; //^1 is last in list
                garbageLines.RemoveAt(garbageLines.Count-1);
            }
        }

        //Update receive bar
        tetrisGames[0].blocksBeingAdded = garbageLines0.Count;
        tetrisGames[1].blocksBeingAdded = garbageLines1.Count;
    }

    public override void Update(GameTime gameTime)
    {
        //Check if either player has lost by topping out, then the other wins
        base.Update(gameTime);
        if (tetrisGames[0].isGameOver && !tetrisGames[1].isGameOver)
            tetrisGames[1].Win();
        else if (tetrisGames[1].isGameOver && !tetrisGames[0].isGameOver)
            tetrisGames[0].Win();
    }
}