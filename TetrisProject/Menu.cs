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
    
    //Variables
    private MenuState menuState; //The currently active menu
    private byte menuIndex; //What is selected in the menu
    
    //Visual variables
    private Vector2 topLeftTopButtonPosition;
    private Vector2 buttonVerticalOffset;

    public Menu(Main main, SpriteBatch spriteBatch)
    {
        this.main = main;
        this.spriteBatch = spriteBatch;
    }
    
    public void LoadContent(ContentManager content)
    {
        buttonBegin = content.Load<Texture2D>("Big Button Begin");
        buttonMiddle = content.Load<Texture2D>("Big Button Middle");
        buttonEnd = content.Load<Texture2D>("Big Button End");
        font = content.Load<SpriteFont>("Font");
    }

    public void Instantiate()
    {
        menuState = MenuState.MainMenu;
        menuIndex = 0;
        topLeftTopButtonPosition = new Vector2(50, 50);
        buttonVerticalOffset = new Vector2(0, 30);
    }
    
    public void Update(GameTime gameTime)
    {
        MenuMovement();
    }

    public void Draw()
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                DrawButton("Test", 0);
                DrawButton("Extended test", 1);
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
                menuIndex = (byte)GetMenuLength();
            }
        }
        
        if (Util.GetKeyPressed(Keys.Down))
        {
            menuIndex--;
            
            if (menuIndex == GetMenuLength()) //Loop around
            {
                menuIndex = (byte)GetMenuLength();
            }
        }
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

    //Gets the width of a letter, used to determine width of buttons at runtime
    private int GetLetterWidth(char letter)
    {
        //TODO add other symbols
        //Default size is 3 pixels wide, switch statement contains all exceptions
        switch (letter.ToString().ToLower())
        {
            case "a":
                return 4;
            case "b":
                return 4;
            case "d":
                return 4;
            case "g":
                return 4;
            case "h":
                return 4;
            case "m":
                return 5;
            case "n":
                return 4;
            case "o":
                return 4;
            case "p":
                return 4;
            case "q":
                return 4;
            case "r":
                return 4;
            case "s":
                return 4;
            case "t":
                return 5;
            case "v":
                return 5;
            case "w":
                return 5;
        }

        return 3;
    }

    private int GetStringWidth(string text)
    {
        int count = 0;
        
        for (int i = 0; i < text.Length; i++)
        {
            count += GetLetterWidth(text[i]) + 1; //Add 1 for spacing
        }

        return count;
    }

    private Color GetButtonColor(int index)
    {
        if (menuIndex == index)
        {
            return Color.Yellow;
        }

        return Color.White;
    }

    private void DrawButton(string text, int index)
    {
        int buttonMiddleWidth = GetStringWidth(text);
        spriteBatch.Draw(buttonBegin, topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.Draw(buttonMiddle, new Vector2(26,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, null, GetButtonColor(index), 0f, Vector2.Zero, new Vector2(buttonMiddleWidth, 1), SpriteEffects.None, 0f);
        spriteBatch.Draw(buttonEnd, new Vector2(26 + buttonMiddleWidth*2,0) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
        spriteBatch.DrawString(font, text, new Vector2(26,13) + topLeftTopButtonPosition + buttonVerticalOffset * index, GetButtonColor(index));
    }
}

public enum MenuState
{
    MainMenu,
}

public enum MainMenu
{
    Start,
    Quit,
}