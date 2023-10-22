using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class GameHandler
{
    //Holds all the active running TetrisGame(s)
    private ContentManager content;
    protected List<TetrisGame> tetrisGames = new();
    public Main mainReference;
    private Settings settings;
    public Settings SettingsStruct => settings;

    protected Texture2D squareTile; //Square tile is a 1x1 white rectangle that can be scaled and colored to draw rectangles
    private Texture2D background;
    private Rectangle backgroundRect;

    private double screenFlashTimer;
    private double totalScreenFlashTime = 500;

    public bool gameFinished; //If a player has finished the other has won, for singleplayer same implementation as in TetrisGame
    public bool playerInStress; //If a player is in stress, run the appropriate code

    public GameHandler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls, Main mainReference = null)
    {
        this.content = content;
        this.settings = settings;
        this.mainReference = mainReference;
        background = content.Load<Texture2D>("background");
        backgroundRect = new Rectangle(0, 0, Main.WorldWidth, Main.WorldHeight);
        if (gameMode == GameMode.Standard)
        {
            tetrisGames.Add(new TetrisGame(this, settings, controls[0], gameMode));
        }
    }

    public void Instantiate()
    {
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.Instantiate(settings.game.startingLevel);
        }
    }

    public void LoadContent()
    {
        squareTile = content.Load<Texture2D>("Square");
        
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.LoadContent(content);
        }
    }
    
    public virtual void Update(GameTime gameTime)
    {
        screenFlashTimer = Math.Max(screenFlashTimer - gameTime.ElapsedGameTime.TotalMilliseconds, 0.0);
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.Update(gameTime);
            if (tetrisGame.isGameOver && tetrisGames.Count == 1 && tetrisGame.Field.Width == 10 && mainReference != null)
                mainReference.SaveSettings(tetrisGame.score);
        }

        //If any of the tetrisGames is in stress / game over state, make that the value for the entire game
        if (tetrisGames.Count > 1)
        {
            gameFinished = tetrisGames[0].isGameOver || tetrisGames[1].isGameOver;
            playerInStress = tetrisGames[0].isInStress || tetrisGames[1].isInStress;
        }
        else
        {
            gameFinished = tetrisGames[0].isGameOver;
            playerInStress = tetrisGames[0].isInStress;
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        //Draw background
        spriteBatch.Draw(background, backgroundRect, Color.White);
        spriteBatch.Draw(squareTile, backgroundRect, Color.White * 0.5f * ((float)(screenFlashTimer / totalScreenFlashTime)));
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.Draw(spriteBatch);
        }
    }

    public void ScreenFlash(double length = 2000)
    {
        totalScreenFlashTime = length;
        screenFlashTimer = length;
    }

    //Use this as a general event function to add extra functionality to other game modes
    public virtual void PiecePlaced(int instance)
    {
        
    }

    //Use this as a general event function to add extra functionality to other game modes
    public virtual void LineCleared(int linesCleared, int multiplayerLinesCleared, int instance)
    {
        
    }
}