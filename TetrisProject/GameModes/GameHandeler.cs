using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public class GameHandeler
{
    //Holds all the active running TetrisGame(s)
    private ContentManager content;
    protected List<TetrisGame> tetrisGames = new();
    private Settings settings;

    protected Texture2D squareTile;

    public bool gameFinished; //If a player has finished the other has won, for singleplayer same implementation as in TetrisGame
    public bool playerInStress; //If a player is in stress, run the appropriate code

    public GameHandeler(ContentManager content, GameMode gameMode, Settings settings, List<Controls> controls)
    {
        this.content = content;
        this.settings = settings;

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
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.Update(gameTime);
        }

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
        foreach (var tetrisGame in tetrisGames)
        {
            tetrisGame.Draw(spriteBatch);
        }
    }

    //Use this as a general event function to add extra functionality to other game modes
    public virtual void PiecePlaced()
    {
        
    }

    public virtual void LineCleared()
    {
        
    }
}