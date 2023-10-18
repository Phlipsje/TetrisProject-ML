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
        tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode, 0));
        tetrisGames.Add(new TetrisGame(this, settings, controls[1], gameMode, 1));

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

    public override void LineCleared()
    {
        
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