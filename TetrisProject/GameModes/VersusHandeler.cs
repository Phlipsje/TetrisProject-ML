using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject; 

public class VersusHandeler : GameHandeler
{
    private readonly int linesToWin;
    
    public VersusHandeler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls) : base(content, gameMode, settings, controls)
    {
        tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode, 1, false));
        tetrisGames.Add(new TetrisGame(this, settings, controls[1], gameMode, 2, false));

        linesToWin = settings.game.linesToWin;
    }

    public override void PiecePlaced()
    {
        if (MathF.Abs(tetrisGames[0].clearedLines - tetrisGames[1].clearedLines) >= linesToWin)
        {
            foreach (var tetrisGame in tetrisGames)
            {
                tetrisGame.GameOver();
            }
        }
    }

    public override void LineCleared(int linesCleared, int instance)
    {
        //If no line is cleared then return
        if (linesCleared == 0)
        {
            return;
        }
        
        //Create garbage line
        Pieces[] garbageLine = new Pieces[tetrisGames[0].Field.Width];
        garbageLine[0] = Pieces.None;
        for (int i = 1; i < garbageLine.Length; i++)
        {
            garbageLine[i] = Pieces.Garbage;
        }

        garbageLine = Util.ShuffleArray(garbageLine);

        //instance 1 targets 2 and 2 targets 1
        instance = (int)MathF.Abs(instance - 2);
        
        //Add line to opponent
        for (int i = 0; i < tetrisGames[instance].Field.blockArray.Length; i++)
        {
            if (i + linesCleared < tetrisGames[instance].Field.blockArray.Length)
            {
                tetrisGames[instance].Field.blockArray[i] = tetrisGames[instance].Field.blockArray[i + linesCleared];
            }
            else
            {
                tetrisGames[instance].Field.blockArray[i] = garbageLine;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        int clearedLines0 = tetrisGames[0].clearedLines;
        int clearedLines1 = tetrisGames[1].clearedLines;
        int clearedLinesDifference = clearedLines0 - clearedLines1;
        int redBarWidth = 570 + clearedLinesDifference * 570 / linesToWin;
        
        //Total bar
        spriteBatch.Draw(squareTile, new Rectangle(400, 50, 1120, 50), Color.Gray);
        
        //Red part of bar
        spriteBatch.Draw(squareTile, new Rectangle(400, 50, redBarWidth, 50), Color.Red);
        
        //Blue part of bar
        spriteBatch.Draw(squareTile, new Rectangle(400 + redBarWidth, 50, 1120 - redBarWidth, 50), Color.Blue);
    }
    
}