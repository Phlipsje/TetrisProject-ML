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
        buttonBegin = content.Load<Texture2D>("Button Begin");
        buttonMiddle = content.Load<Texture2D>("Button Middle");
        buttonEnd = content.Load<Texture2D>("Button End");
        font = content.Load<SpriteFont>("Font");
    }

    public void Instantiate()
    {
        menuState = MenuState.MainMenu;
        menuIndex = 0;
        topLeftTopButtonPosition = new Vector2(50, 50);
        buttonVerticalOffset = new Vector2(0, 90);
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
                DrawButton("Test", 0, null);
                DrawButton("Extended test", 1, null);
                DrawButton("this doesn't fuck it up", 1, "Extended test");
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
    private void DrawButton(string text, int index, string previousString)
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