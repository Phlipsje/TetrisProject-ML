using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public class Menu
{
    //The menu

    //References
    private Main main;
    private SpriteBatch spriteBatch;

    //Textures
    private Texture2D buttonBegin;
    private Texture2D buttonMiddle;
    private Texture2D buttonEnd;
    private SpriteFont font;
    private Texture2D tile;
    
    //Variables
    public MenuState menuState; //The currently active menu
    public byte menuIndex; //What is selected in the menu
    
    //Visual variables
    private Vector2 topLeftTopButtonPosition;
    private Vector2 buttonVerticalOffset;

    public Menu(Main main, SpriteBatch spriteBatch)
    {
        this.main = main;
        this.spriteBatch = spriteBatch;
        menuState = MenuState.MainMenu;
        menuIndex = 0;
        topLeftTopButtonPosition = new Vector2(50, 50);
        buttonVerticalOffset = new Vector2(0, 90);
    }
    
    public void LoadContent(ContentManager content)
    {
        buttonBegin = content.Load<Texture2D>("Button Begin");
        buttonMiddle = content.Load<Texture2D>("Button Middle");
        buttonEnd = content.Load<Texture2D>("Button End");
        font = content.Load<SpriteFont>("Font");
        tile = content.Load<Texture2D>("Square");
    }

    public void Update(GameTime gameTime)
    {
        MenuMovement();
    }

    public void Draw(GameTime gameTime)
    {
        //Background
        int tileCountHorizontal = 10; //Amount of tiles in horizontal direction on screen at once
        int tileSize = Main.WorldWidth / tileCountHorizontal;
        int aspectRatio = Main.WorldWidth / Main.WorldHeight;
        double timeFrame = gameTime.TotalGameTime.TotalSeconds % 2.5/2.5; //Used to make background move
        //Draw tiles of background
        for (int i = -1; i < tileCountHorizontal; i++)
        {
            for (int j = -1; j < tileCountHorizontal/aspectRatio + 1; j++)
            {
                //Decide color based on checkerboard pattern
                Color tileColor = Color.ForestGreen;
                if ((j * (tileCountHorizontal + 1) + i) % 2 == 0)
                {
                    tileColor = Color.DarkOliveGreen;
                }
                
                //Draw tile
                spriteBatch.Draw(tile, new Rectangle((int)((i + timeFrame)*tileSize), (int)((j + timeFrame)*tileSize), tileSize, tileSize), tileColor);
            }
        }
        
        //Buttons
        switch (menuState)
        {
            case MenuState.MainMenu:
                DrawButton("Play", 0);
                DrawButton("Settings", 1);
                DrawButton("Quit", 2);
                break;
            case MenuState.Settings:
                DrawButton("Master Volume", 0);
                DrawButton($"{main.masterVolume[main.masterVolumeIndex]}%", 0, "Master Volume");
                DrawButton("Sfx Volume", 1);
                DrawButton($"{main.soundEffectVolume[main.soundEffectVolumeIndex]}%", 1, "Sfx Volume");
                DrawButton("Back", 2);
                break;
        }
    }
    
    private void MenuMovement()
    {
        if (Util.GetKeyPressed(Keys.Up))
        {
            if (menuIndex != 0)
            {
                menuIndex--;
            }
            else //Loop around
            {
                menuIndex = (byte)(GetMenuLength() - 1);
            }
        }
        
        if (Util.GetKeyPressed(Keys.Down))
        {
            menuIndex++;
            
            if (menuIndex == GetMenuLength()) //Loop around
            {
                menuIndex = 0;
            }
        }

        if (Util.GetKeyPressed(Keys.Enter))
        {
            MenuFunction(InputType.Select);
        }
        
        if (Util.GetKeyPressed(Keys.Left))
        {
            MenuFunction(InputType.MoveLeft);
        }
        
        if (Util.GetKeyPressed(Keys.Right))
        {
            MenuFunction(InputType.MoveRight);
        }
    }

    #region Menu functions

    private void MenuFunction(InputType inputType)
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                switch (menuIndex)
                {
                    case (byte)MainMenu.Play:
                        if (inputType == InputType.Select) main.gameState = GameState.Playing;
                        break;
                    case (byte)MainMenu.Settings:
                        if (inputType == InputType.Select) GoToMenu(MenuState.Settings);
                        break;
                    case (byte)MainMenu.Quit:
                        if (inputType == InputType.Select) main.Exit();
                        break;
                }
                break;
            
            case MenuState.Settings:
                switch (menuIndex)
                {
                    case (byte)Settings.MasterVolume:
                        if (inputType == InputType.Select) {main.masterVolumeIndex = ToggleNext(main.masterVolume, main.masterVolumeIndex); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) {main.masterVolumeIndex = ToggleNext(main.masterVolume, main.masterVolumeIndex); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) {main.masterVolumeIndex = TogglePrevious(main.masterVolume, main.masterVolumeIndex); main.UpdateVolume();}
                        break;
                    case (byte)Settings.SfxVolume:
                        if (inputType == InputType.Select) {main.soundEffectVolumeIndex = ToggleNext(main.soundEffectVolume, main.soundEffectVolumeIndex); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) {main.soundEffectVolumeIndex = ToggleNext(main.soundEffectVolume, main.soundEffectVolumeIndex); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) {main.soundEffectVolumeIndex = TogglePrevious(main.soundEffectVolume, main.soundEffectVolumeIndex); main.UpdateVolume();}
                        break;
                    case (byte)Settings.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
        }
    }
    #endregion
    
    #region Extra functions

    public void GoToMenu(MenuState state)
    {
        menuState = state;
        menuIndex = 0;
    }
    private byte ToggleNext(int[] array, byte index)
    {
        if (index == array.Length - 1)
        {
            return 0;
        }

        return (byte)(index + 1);
    }
    
    private byte TogglePrevious(int[] array, byte index)
    {
        if (index == 0)
        {
            return (byte)(array.Length - 1);
        }

        return (byte)(index - 1);
    }

    private int GetMenuLength()
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                return Enum.GetNames(typeof(MainMenu)).Length;
            default: //Error value
                return 0;
        }
    }

    private Color GetButtonColor(int index)
    {
        if (menuIndex == index)
        {
            return Color.Yellow;
        }

        return Color.White;
    }

    //Gets the length of a button based on what text it holds
    private int GetButtonLength(string text)
    {
        //Assumes buttons are 80 pixels tall (and begin and end are also 80 pixels wide)
        if (text == null)
        {
            return 160;
        }
        
        return 160 + (int)font.MeasureString(text).X;
    }

    //Draw a button with a dynamic length and spacing
    private void DrawButton(string text, int index, string previousString = null)
    {
        //Get button spacing
        int horizontalOffset = GetButtonLength(previousString);
        
        //Extra spacing so buttons aren't glued together
        if (horizontalOffset != 160)
        {
            horizontalOffset += 100;
        }

        //Creates a button with a dynamic length based on the length of the string
        spriteBatch.Draw(buttonBegin, new Vector2(horizontalOffset, 0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.Draw(buttonMiddle, new Vector2(horizontalOffset + 80,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, null, GetButtonColor(index), 0f, Vector2.Zero, new Vector2(font.MeasureString(text).X, 1), SpriteEffects.None, 0f);
        spriteBatch.Draw(buttonEnd, new Vector2( horizontalOffset + 80 + font.MeasureString(text).X,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.DrawString(font, text, new Vector2(horizontalOffset + 80,40) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
    }
    #endregion
    
    //What type of input is done on a button
    private enum InputType
    {
        Select,
        MoveLeft,
        MoveRight,
    }
}

public enum MenuState
{
    MainMenu,
    Settings,
}

public enum MainMenu
{
    Play,
    Settings,
    Quit,
}

public enum Settings
{
    MasterVolume,
    SfxVolume,
    Back,
}