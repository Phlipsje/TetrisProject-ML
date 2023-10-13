using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TetrisProject
{
    public class Main : Game
    {
        public GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private TetrisGame tetrisGame;
        private Menu menu;
        private RenderTarget2D renderTarget;
        public Settings settings;
        public int WindowWidth = 800;
        public int WindowHeight = 450;
        public const int WorldWidth = 1920;
        public const int WorldHeight = 1080;
        public GameState gameState;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.ApplyChanges();
            settings = new Settings();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, WorldWidth, WorldHeight);
            menu = new Menu(this, spriteBatch);
            tetrisGame = null;

            gameState = GameState.Menu;
            
            UpdateVolume();

            base.Initialize();
        }

        public void EnterFullScreen()
        {
            WindowWidth = 1920;
            WindowHeight = 1080;
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.ApplyChanges();
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
        }
        
        public void ExitFullScreen()
        {
            WindowWidth = 800;
            WindowHeight = 450;
            
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.ApplyChanges();
        }

        public void ToggleFullScreen()
        {
            if(graphics.IsFullScreen)
                ExitFullScreen();
            else
                EnterFullScreen();
        }

        protected override void LoadContent()
        {
            menu.LoadContent(Content);
            SfxManager.Load(Content);
            MusicManager.Initialize(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Util.GetKeyPressed(Keys.Escape))
            {
                switch (gameState)
                {
                    case GameState.Menu:
                        switch (menu.menuState)
                        {
                            case MenuState.MainMenu:
                                Exit();
                                break;
                            case MenuState.Lobby:
                                menu.GoToMenu(MenuState.MainMenu);
                                break;
                            case MenuState.Settings:
                                menu.GoToMenu(MenuState.MainMenu);
                                break;
                            case MenuState.ControlProfiles:
                                menu.GoToMenu(MenuState.Settings);
                                break;
                            case MenuState.Controls:
                                menu.GoToMenu(MenuState.ControlProfiles);
                                break;
                        }
                        break;
                    case GameState.Playing:
                        gameState = GameState.Pause;
                        MusicManager.SetPitch(gameTime);
                        break;
                    case GameState.Pause:
                        gameState = GameState.Playing;
                        MusicManager.Normal(gameTime);
                        break;
                    default:
                        Exit();
                        break;
                }
            }
            
            if (Util.GetKeyPressed(Keys.F11))
                ToggleFullScreen();
            
            Util.Update();

            if (gameState == GameState.Menu)
            {
                menu.Update(gameTime);
            }
            else if(gameState == GameState.Playing)
            {
                //Create a new game when play is pressed
                if (tetrisGame == null)
                {
                    tetrisGame = new TetrisGame(this, settings, settings.controlProfiles[0]);
                    tetrisGame.Instantiate(settings.game.startingLevel);
                    tetrisGame.LoadContent(Content);
                    MusicManager.PlaySong(MusicManager.ClassicTheme);
                }
                tetrisGame.Update(gameTime);
            }
            else if (gameState == GameState.Pause)
            {
                if (Util.GetKeyPressed(Keys.Back))
                {
                    menu.menuState = MenuState.MainMenu;
                    menu.menuIndex = 0;
                    gameState = GameState.Menu;
                    tetrisGame = null;
                    MusicManager.Stop(gameTime);
                }
            }
            
            AnimationManager.Update(gameTime);
            SfxManager.Update();
            MusicManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Tell the GraphicsDevice that the spriteBatch is targeted at renderTarget
            GraphicsDevice.SetRenderTarget(renderTarget);
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw what is on screen
            spriteBatch.Begin();
            
            if (gameState == GameState.Menu)
            {
                menu.Draw(gameTime);
            }
            else if (gameState == GameState.Playing && tetrisGame != null)
            {
                tetrisGame.Draw(spriteBatch);
            }
            
            //Play animations
            AnimationManager.Draw(spriteBatch);
            
            //Stop drawing what is on screen
            spriteBatch.End();
            
            //Tell the GraphicsDevice that spriteBatch will now target the application window
            GraphicsDevice.SetRenderTarget(null);

            //Resize renderTarget to the application window
            spriteBatch.Begin();
            Rectangle destinationRectangle = new Rectangle(0, 0, WindowWidth, WindowHeight);
            spriteBatch.Draw(renderTarget, destinationRectangle, Color.White);
            spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        public void UpdateVolume()
        {
            SoundEffect.MasterVolume = (float)settings.masterVolume / 100 * settings.soundEffectVolume/100;
        }
    }
}

public enum GameState
{
    Menu,
    Playing,
    Pause
}