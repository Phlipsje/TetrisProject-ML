using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class TugOfWarHandler : GameHandler
{
    private readonly int linesToWin;
    
    public TugOfWarHandler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls, Main mainReference) : base(content, gameMode, settings, controls, mainReference)
    {
        tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode, 1, false));
        tetrisGames.Add(new TetrisGame(this, settings, controls[1], gameMode, 2, false));

        linesToWin = settings.game.linesToWin;
    }

    public override void PiecePlaced(int instance)
    {
        //Check if either player has cleared an x amount of lines more than the opponent
        //Uses variable goal scoring, meaning t-spins and mini-t-spins that don't clear lines still increase the clearedLines score (and thus checked for every piece placed)
        if (tetrisGames[0].clearedLines - tetrisGames[1].clearedLines >= linesToWin)
        {
            tetrisGames[0].Win();
            tetrisGames[1].GameOver();
        }
        else if (tetrisGames[1].clearedLines - tetrisGames[0].clearedLines >= linesToWin)
        {
            tetrisGames[1].Win();
            tetrisGames[0].GameOver();
        }
    }

    public override void Update(GameTime gameTime)
    {
        //Check if either player has lost by topping out, then the other wins
        base.Update(gameTime);
        if (tetrisGames[0].isGameOver && !tetrisGames[1].isGameOver)
            tetrisGames[1].Win();
        else if (tetrisGames[1].isGameOver && !tetrisGames[0].isGameOver)
            tetrisGames[0].Win();
        ;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        int clearedLines0 = tetrisGames[0].clearedLines;
        int clearedLines1 = tetrisGames[1].clearedLines;
        int clearedLinesDifference = clearedLines0 - clearedLines1;
        int redBarWidth = 570 + clearedLinesDifference * 570 / linesToWin;

        //Red part of bar
        spriteBatch.Draw(squareTile, new Rectangle(400, 50, redBarWidth, 50), Color.Red);
        
        //Blue part of bar
        spriteBatch.Draw(squareTile, new Rectangle(400 + redBarWidth, 50, 1120 - redBarWidth, 50), Color.Blue);
    }
    
}