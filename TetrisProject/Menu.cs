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
    private Settings settings;

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
    private int selectedHorizontalOffsetTotal; //Applied to the button that is selected
    private float[] selectedHorizontalOffsets; //Offset of each individual button
    private float buttonAnimationTimeMax;
    

    public Menu(Main main, SpriteBatch spriteBatch, Settings settings)
    {
        this.main = main;
        this.spriteBatch = spriteBatch;
        this.settings = settings;
        menuState = MenuState.MainMenu;
        menuIndex = 0;
        buttonAnimationTimeMax = 0.3f;
        topLeftTopButtonPosition = new Vector2(50, 50);
        buttonVerticalOffset = new Vector2(0, 90);
        selectedHorizontalOffsetTotal = 80;
    }
    
    public void LoadContent(ContentManager content)
    {
        buttonBegin = content.Load<Texture2D>("Button Begin");
        buttonMiddle = content.Load<Texture2D>("Button Middle");
        buttonEnd = content.Load<Texture2D>("Button End");
        font = content.Load<SpriteFont>("Font");
        tile = content.Load<Texture2D>("Square");
        
        GoToMenu(MenuState.MainMenu);
    }

    public void Update(GameTime gameTime)
    {
        MenuMovement();

        AnimateMenu(gameTime.ElapsedGameTime.TotalSeconds);
    }

    //Update offsets of buttons that are animated based on selected button
    private void AnimateMenu(double deltaTime)
    {
        for (int i = 0; i < GetMenuLength(); i++)
        {
            if (i == menuIndex)
            {
                selectedHorizontalOffsets[i] += selectedHorizontalOffsetTotal/buttonAnimationTimeMax * (float)deltaTime;
            }
            else
            {
                selectedHorizontalOffsets[i] -= selectedHorizontalOffsetTotal/buttonAnimationTimeMax * (float)deltaTime;
            }

            selectedHorizontalOffsets[i] = MathHelper.Clamp(selectedHorizontalOffsets[i], 0, selectedHorizontalOffsetTotal);
        }
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
                DrawButton($"{settings.masterVolume}%", 0, "Master Volume");
                DrawButton("Sfx Volume", 1);
                DrawButton($"{settings.soundEffectVolume}%", 1, "Sfx Volume");
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
                    case (byte)SettingsMenu.MasterVolume:
                        if (inputType == InputType.Select) { settings.masterVolume = Increment(settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { settings.masterVolume = Increment(settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { settings.masterVolume = Increment(settings.masterVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.SfxVolume:
                        if (inputType == InputType.Select) { settings.soundEffectVolume = Increment(settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { settings.soundEffectVolume = Increment(settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { settings.soundEffectVolume = Increment(settings.soundEffectVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.Back:
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
        selectedHorizontalOffsets = new float[GetMenuLength()];
        selectedHorizontalOffsets[menuIndex] = selectedHorizontalOffsetTotal;
    }
    
    //Toggle to next in list
    private byte ToggleNext(int[] array, byte index)
    {
        if (index == array.Length - 1)
        {
            return 0;
        }

        return (byte)(index + 1);
    }
    
    //Toggle to previous in list
    private byte TogglePrevious(int[] array, byte index)
    {
        if (index == 0)
        {
            return (byte)(array.Length - 1);
        }

        return (byte)(index - 1);
    }

    //Increase / decrease by a fixed amount
    private int Increment(int value, int increment, int min, int max)
    {
        value += increment;
        return MathHelper.Clamp(value, min, max);
    }

    private int GetMenuLength()
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                return Enum.GetNames(typeof(MainMenu)).Length;
            case MenuState.Settings:
                return Enum.GetNames(typeof(SettingsMenu)).Length;
            default: //Error value
                return 0;
        }
    }

    private Color GetButtonColor(int index)
    {
        if (menuIndex == index)
        {
            switch (index % 7)
            {
                case 0:
                    return Color.LightBlue;
                case 1:
                    return Color.Blue;
                case 2:
                    return Color.Orange;
                case 3:
                    return Color.Yellow;
                case 4:
                    return Color.Red;
                case 5:
                    return Color.Purple;
                case 6:
                    return Color.LightGreen;
            }
        }

        //Error value
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

        //Extra horizontal offset if button is selected
        horizontalOffset += (int)selectedHorizontalOffsets[index];

        //Extra spacing so buttons aren't glued together
        if (previousString != null)
        {
            horizontalOffset += 100;
        }

        //Creates a button with a dynamic length based on the length of the string
        spriteBatch.Draw(buttonBegin, new Vector2(horizontalOffset, 0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.Draw(buttonMiddle, new Vector2(horizontalOffset + 80,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, null, GetButtonColor(index), 0f, Vector2.Zero, new Vector2(font.MeasureString(text).X, 1), SpriteEffects.None, 0f);
        spriteBatch.Draw(buttonEnd, new Vector2( horizontalOffset + 80 + font.MeasureString(text).X,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.DrawString(font, text, new Vector2(horizontalOffset + 80,10) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
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

public enum SettingsMenu
{
    MasterVolume,
    SfxVolume,
    Back,
}