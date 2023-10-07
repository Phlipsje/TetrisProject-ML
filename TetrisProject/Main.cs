using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject
{
    public class Main : Game
    {
        public GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private TetrisGame tetrisGame;
        private Menu menu;
        private RenderTarget2D renderTarget;
        public int WindowWidth = 800;
        public int WindowHeight = 450;
        public const int WorldWidth = 1920;
        public const int WorldHeight = 1080;
        private GameState gameState;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.ApplyChanges();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, WorldWidth, WorldHeight);
            menu = new Menu(this, spriteBatch);
            tetrisGame = new TetrisGame(this);

            gameState = GameState.Playing; //Change this to decide how the game starts
            
            menu.Instantiate();
            tetrisGame.Instantiate();

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
            tetrisGame.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Util.GetKeyPressed(Keys.F11))
                ToggleFullScreen();
            
            Util.Update();

            if (gameState == GameState.Menu)
            {
                menu.Update(gameTime);
            }
            else
            {
                tetrisGame.Update(gameTime);
            }
            
            AnimationManager.Update(gameTime);
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
                menu.Draw();
            }
            else
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
    }
}

public enum GameState
{
    Menu,
    Playing,
}