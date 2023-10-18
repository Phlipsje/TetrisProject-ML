using System;
using System.Collections.Generic;
using System.IO;
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
        private GameHandeler gameHandeler;
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

            //For anti aliasing
            graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
            graphics.ApplyChanges();
        }

        //Anti aliasing (X8)
        private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            graphics.PreferMultiSampling = true;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }
        
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.ApplyChanges();
            if (File.Exists("Settings.conf"))
            {
                settings = Util.LoadSettingsFromFile("Settings.conf");
            }
            else
            {
                settings = new Settings();
            }
            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, WorldWidth, WorldHeight);
            menu = new Menu(this, spriteBatch);
            gameHandeler = null;

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
            MusicManager.Load(Content);
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
                                QuitGame();
                                break;
                            case MenuState.LobbyStandard:
                                menu.GoToMenu(MenuState.MainMenu);
                                break;
                            case MenuState.LobbyTugOfWar:
                                menu.GoToMenu(MenuState.MainMenu);
                                break;
                            case MenuState.LobbyVersus:
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
                        if (gameHandeler.gameFinished)
                        {

                            break;
                        }
                        gameState = GameState.Pause;
                        MusicManager.SetPitch(gameTime);
                        break;
                    case GameState.Pause:
                        gameState = GameState.Playing;
                        MusicManager.Normal(gameTime);
                        break;
                    default:
                        QuitGame();
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
                if (gameHandeler == null)
                {
                    List<Controls> selectedControls = new ();
                    switch ((GameMode)menu.gameModeIndex)
                    {
                        default: //GameMode.Standard
                            selectedControls.Add(settings.controlProfiles[menu.profileIndex]);
                            gameHandeler = new GameHandeler(Content, (GameMode)menu.gameModeIndex, settings, selectedControls, this);
                            break;
                        case GameMode.TugOfWar:
                            selectedControls.Add(settings.controlProfiles[menu.profileIndex]);
                            selectedControls.Add(settings.controlProfiles[menu.profileIndex2]);
                            gameHandeler = new TugOfWarHandeler(Content, (GameMode)menu.gameModeIndex, settings, selectedControls);
                            break;
                        case GameMode.Versus:
                            selectedControls.Add(settings.controlProfiles[menu.profileIndex]);
                            selectedControls.Add(settings.controlProfiles[menu.profileIndex2]);
                            gameHandeler = new VersusHandeler(Content, (GameMode)menu.gameModeIndex, settings, selectedControls);
                            break;
                    }
                    
                    gameHandeler.Instantiate();
                    gameHandeler.LoadContent();
                    MusicManager.PlaySong(MusicManager.ClassicTheme);
                }
                if (Util.GetKeyPressed(Keys.Enter) && gameHandeler.gameFinished)
                {
                    menu.menuState = MenuState.MainMenu;
                    menu.menuIndex = 0;
                    gameState = GameState.Menu;
                    gameHandeler = null;
                    MusicManager.Stop(gameTime);
                    return;
                }
                gameHandeler.Update(gameTime);
                if (gameHandeler.gameFinished)
                    MusicManager.SetPitch(gameTime, -1, 4000);
                else if (gameHandeler.playerInStress && gameState != GameState.Pause)
                {
                    MusicManager.SetPitch(gameTime, 0.5f, 250);
                }
                else
                {
                    MusicManager.SetPitch(gameTime, 0, 5000);
                }
            }
            else if (gameState == GameState.Pause)
            {
                if (Util.GetKeyPressed(Keys.Back))
                {
                    menu.menuState = MenuState.MainMenu;
                    menu.menuIndex = 0;
                    gameState = GameState.Menu;
                    gameHandeler = null;
                    MusicManager.Stop(gameTime);
                }
            }
            
            AnimationManager.Update(gameTime);
            SfxManager.Update();
            MusicManager.Update(gameTime);
            base.Update(gameTime);
        }
        
        public void SaveSettings(int newScore = 0)
        {
            settings.highScore = MathHelper.Max(settings.highScore, newScore);
            Util.SaveSettingsToFile(settings, "Settings.conf");
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
            else if (gameState == GameState.Playing && gameHandeler != null)
            {
                gameHandeler.Draw(spriteBatch);
            }
            else if (gameState == GameState.Pause)
            {
                drawPauseScreen();
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

        private void drawPauseScreen()
        {
            string pauseString = "PAUSED";
            string pauseInfoString = "press backspace to quit";
            Vector2 pauseStringSize = menu.font.MeasureString(pauseString);
            Vector2 pauseInfoStringSize = menu.font.MeasureString(pauseInfoString);
            Vector2 pauseStringPosition = new Vector2(WorldWidth - pauseStringSize.X,
                WorldHeight - pauseStringSize.Y - pauseInfoStringSize.Y) / 2;
            Vector2 pauseInfoStringPosition = new Vector2((WorldWidth - pauseInfoStringSize.X) / 2,
                pauseStringPosition.Y + pauseInfoStringSize.Y);
            spriteBatch.DrawString(menu.font, pauseString, pauseStringPosition, Color.White);
            spriteBatch.DrawString(menu.font, pauseInfoString, pauseInfoStringPosition, Color.White);
        }
        
        public void UpdateVolume()
        {
            SoundEffect.MasterVolume = (float)settings.masterVolume / 100;
            MusicManager.Initialize(settings);
        }

        public void QuitGame()
        {
            Util.SaveSettingsToFile(settings, "Settings.conf");
            Exit();
        }
    }
}

public enum GameState
{
    Menu,
    Playing,
    Pause
}