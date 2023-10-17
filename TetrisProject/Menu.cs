using System;
using System.Collections.Generic;
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
    public SelectingKeys selectingKeys;

    //Textures
    private Texture2D buttonBegin;
    private Texture2D buttonMiddle;
    private Texture2D buttonEnd;
    public SpriteFont font;
    private Texture2D tile;
    
    //Variables
    public MenuState menuState; //The currently active menu
    public byte menuIndex; //What is selected in the menu
    private int editingControlScheme; //The control scheme that is actively being adjusted (used for changing keybinds)
    private string newProfileName = "";
    
    //Toggles
    public byte profileIndex;
    private readonly string[] gameModeNames = {"Standard", "Tug Of War"};
    
    public byte gameModeIndex;
    
    //Visual variables
    private readonly Vector2 topLeftTopButtonPosition;
    private readonly Vector2 buttonVerticalOffset;
    private readonly int selectedHorizontalOffsetTotal; //Applied to the button that is selected
    private float[] selectedHorizontalOffsets; //Offset of each individual button
    private readonly float buttonAnimationTimeMax;

    public Menu(Main main, SpriteBatch spriteBatch)
    {
        this.main = main;
        this.spriteBatch = spriteBatch;
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
        if (menuState == MenuState.MapKeys)
        {
            selectingKeys.Update();
            return;
        }
        MenuMovement();

        AnimateMenu(gameTime.ElapsedGameTime.TotalSeconds);
    }

    #region Visual Methods
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
        DrawBackground(gameTime);
        
        if (menuState == MenuState.MapKeys)
        {
            
            selectingKeys.Draw(spriteBatch, tile, font);
            return;
        }

        //Buttons
        switch (menuState)
        {
            case MenuState.MainMenu:
                DrawButton("Play", 0);
                DrawButton("Settings", 1);
                DrawButton("Quit", 2);
                break;
            case MenuState.Lobby:
                DrawButton("Start", 0);
                DrawButton("GameMode", 1);
                DrawButton(gameModeNames[gameModeIndex], 1, "GameMode");
                DrawButton("Profile", 2);
                DrawButton(main.settings.controlProfiles[profileIndex].controlName, 2, "Profile");
                DrawButton("Starting Level", 3);
                DrawButton(main.settings.game.startingLevel.ToString(), 3, "Starting Level");
                DrawButton("Gravity Multiplier", 4);
                DrawButton($"{MathF.Round((float)main.settings.game.gravityMultiplier*10)/10}x", 4, "Gravity Multiplier");
                DrawButton("Back", 5);
                break;
            
            case MenuState.Settings:
                DrawButton("Master Volume", 0);
                DrawButton($"{main.settings.masterVolume}%", 0, "Master Volume");
                DrawButton("Sfx Volume", 1);
                DrawButton($"{main.settings.soundEffectVolume}%", 1, "Sfx Volume");
                DrawButton("Controls", 2);
                DrawButton("Back", 3);
                break;
            
            case MenuState.ControlProfiles:
                //Draw all control profiles
                
                for (int i = 0; i < main.settings.controlProfiles.Count+3; i++)
                {
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count)
                    {
                        DrawButton("Create Profile", i);
                        if (newProfileName != "")
                        {
                            DrawButton(newProfileName, i, "Create Profile");
                        }
                        else
                        {
                            DrawButton("Enter name", i, "Create Profile");
                        }
                        continue;
                    }
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count+1)
                    {
                        DrawButton("Delete Profile", i);
                        DrawButton(main.settings.controlProfiles[profileIndex].controlName, i, "Delete Profile");
                        continue;
                    }
                    //Draw button at the bottom of the list
                    if (i == main.settings.controlProfiles.Count+2)
                    {
                        DrawButton("Back", i);
                        continue;
                    }
                    
                    //Otherwise draw button representing that control scheme
                    DrawButton(main.settings.controlProfiles[i].controlName, i);
                }
                break;
            
            case MenuState.Controls:
                Controls profile = main.settings.controlProfiles[editingControlScheme];
                DrawButton("Move Left", 0);
                DrawButton(ArrayListedAsString(profile.leftKey), 0, "Move Left");
                DrawButton("Move Right", 1);
                DrawButton(ArrayListedAsString(profile.rightKey), 1, "Move Right");
                DrawButton("Soft Drop", 2);
                DrawButton(ArrayListedAsString(profile.softDropKey), 2, "Soft Drop");
                DrawButton("Hard Drop", 3);
                DrawButton(ArrayListedAsString(profile.hardDropKey), 3, "Hard Drop");
                DrawButton("Rotate Clockwise", 4);
                DrawButton(ArrayListedAsString(profile.rotateClockWiseKey), 4, "Rotate Clockwise");
                DrawButton("Rotate Counterclockwise", 5);
                DrawButton(ArrayListedAsString(profile.rotateCounterClockWiseKey), 5, "Rotate Counterclockwise");
                DrawButton("Hold", 6);
                DrawButton(ArrayListedAsString(profile.holdKey), 6, "Hold");
                DrawButton("Back", 7);

                break;
        }
    }

    private void DrawBackground(GameTime gameTime)
    {
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
    }
    #endregion
    
    #region Menu Movement
    private void MenuMovement()
    {
        //Check if a key was pressed
        if (Util.GetKeysPressed().Length == 0)
        {
            return;
        }

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

            return;
        }
        
        if (Util.GetKeyPressed(Keys.Down))
        {
            menuIndex++;
            
            if (menuIndex == GetMenuLength()) //Loop around
            {
                menuIndex = 0;
            }
            
            return;
        }

        if (Util.GetKeyPressed(Keys.Enter))
        {
            MenuFunction(InputType.Select);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Left))
        {
            MenuFunction(InputType.MoveLeft);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Right))
        {
            MenuFunction(InputType.MoveRight);
            return;
        }
        
        if (Util.GetKeyPressed(Keys.Back))
        {
            MenuFunction(InputType.Back);
            return;
        }
        
        //Other key was pressed
        MenuFunction(InputType.Text);
    }
    #endregion

    #region Menu functions
    private void MenuFunction(InputType inputType)
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                switch (menuIndex)
                {
                    case (byte)MainMenu.Play:
                        if (inputType == InputType.Select) GoToMenu(MenuState.Lobby);
                        break;
                    case (byte)MainMenu.Settings:
                        if (inputType == InputType.Select) GoToMenu(MenuState.Settings);
                        break;
                    case (byte)MainMenu.Quit:
                        if (inputType == InputType.Select) main.QuitGame();
                        break;
                }
                break;
            
            case MenuState.Lobby:
                switch (menuIndex)
                {
                    case (byte)Lobby.Start:
                        if (inputType == InputType.Select) main.gameState = GameState.Playing;
                        break;
                    case (byte)Lobby.GameMode:
                        if (inputType == InputType.Select) gameModeIndex = ToggleNext(gameModeNames, gameModeIndex);
                        if (inputType == InputType.MoveRight) gameModeIndex = ToggleNext(gameModeNames, gameModeIndex);
                        if (inputType == InputType.MoveLeft) gameModeIndex = TogglePrevious(gameModeNames, gameModeIndex);
                        break;
                    case (byte)Lobby.Profile:
                        if (inputType == InputType.Select) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveRight) profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);
                        if (inputType == InputType.MoveLeft) profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);
                        break;
                    case (byte)Lobby.StartingLevel:
                        if (inputType == InputType.Select) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveRight) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, 1, 1, 15);
                        if (inputType == InputType.MoveLeft) main.settings.game.startingLevel = Increment(main.settings.game.startingLevel, -1, 1, 15);
                        break;
                    case (byte)Lobby.GravityMultiplier:
                        if (inputType == InputType.Select) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveRight) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, 0.1, 0.1, 5);
                        if (inputType == InputType.MoveLeft) main.settings.game.gravityMultiplier = Increment(main.settings.game.gravityMultiplier, -0.1, 0.1, 5);
                        break;
                    case (byte)Lobby.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
            
            case MenuState.Settings:
                switch (menuIndex)
                {
                    case (byte)SettingsMenu.MasterVolume:
                        if (inputType == InputType.Select) { main.settings.masterVolume = Increment(main.settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { main.settings.masterVolume = Increment(main.settings.masterVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { main.settings.masterVolume = Increment(main.settings.masterVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.SfxVolume:
                        if (inputType == InputType.Select) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveRight) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, 10, 0, 100); main.UpdateVolume();}
                        if (inputType == InputType.MoveLeft) { main.settings.soundEffectVolume = Increment(main.settings.soundEffectVolume, -10, 0, 100); main.UpdateVolume();}
                        break;
                    case (byte)SettingsMenu.Controls:
                        if(inputType == InputType.Select) 
                        {
                            GoToMenu(MenuState.ControlProfiles);
                            
                        }
                        break;
                    case (byte)SettingsMenu.Back:
                        if (inputType == InputType.Select) GoToMenu(MenuState.MainMenu);
                        break;
                }
                break;
            
            case MenuState.ControlProfiles:
                if (inputType == InputType.Select)
                {
                    //Create new profile button
                    if (menuIndex == GetMenuLength() - 3)
                    {
                        //Only make new control profile if name is not empty
                        if (newProfileName != "")
                        {
                            //Only make new control profile if name is not in use yet
                            foreach (var profiles in main.settings.controlProfiles)
                            {
                                if (string.Equals(profiles.controlName, newProfileName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    return;
                                }
                            }
                            
                            Controls newControls = new Controls();
                            newControls.controlName = newProfileName;
                            main.settings.controlProfiles.Add(newControls);
                            editingControlScheme = main.settings.controlProfiles.Count-1; //Add() adds to end of list, set control scheme to end of list
                            GoToMenu(MenuState.Controls);
                        }
                        break;
                    }
                    //Delete profile button
                    if (menuIndex == GetMenuLength() - 2)
                    {
                        //Can't delete default
                        if (profileIndex != 0)
                        {
                            main.settings.controlProfiles.RemoveAt(profileIndex);
                        }
                        GoToMenu(MenuState.ControlProfiles, (byte)(GetMenuLength() - 2));
                        break;
                    }
                    //Back button
                    if (menuIndex == GetMenuLength() - 1)
                    {
                        GoToMenu(MenuState.Settings);
                        break;
                    }

                    //If not back button select the control scheme and go to menu page
                    editingControlScheme = menuIndex;
                    GoToMenu(MenuState.Controls);
                }

                //If hovering over create profile and typing text
                if (inputType == InputType.Text && menuIndex == GetMenuLength() - 3)
                {
                    //Add text to string, with a limit on what characters can be user
                    newProfileName += FilterInput(Util.GetKeysPressed(), "abcdefghijklmnopqrstuvwxyz");
                }

                //If hovering over create profile and deleting text
                if (inputType == InputType.Back && menuIndex == GetMenuLength() - 3 && newProfileName.Length != 0)
                {
                    //Delete last letter of string
                    newProfileName = newProfileName.Substring(0, newProfileName.Length - 1);
                }

                if (inputType == InputType.MoveLeft && menuIndex == GetMenuLength() - 2) { profileIndex = TogglePrevious(main.settings.controlProfiles.ToArray(), profileIndex);}
                if (inputType == InputType.MoveRight && menuIndex == GetMenuLength() - 2) { profileIndex = ToggleNext(main.settings.controlProfiles.ToArray(), profileIndex);}
                break;
            
            case MenuState.Controls:
                if (inputType == InputType.Select)
                {
                    //Back button
                    if (menuIndex == (byte)ControlsMenu.Back)
                    {
                        GoToMenu(MenuState.ControlProfiles);
                        break;
                    }
                    
                    //If not back button go to selecting keybind
                    selectingKeys = new SelectingKeys(main, this, editingControlScheme, (ControlsMenu)menuIndex);
                    GoToMenu(MenuState.MapKeys);
                }
                break;
        }
    }
    #endregion
    
    #region Extra functions

    //Only parse allowed characters to string
    private string FilterInput(Keys[] keys, string filter)
    {
        string text = "";
        List<string> filterList = new List<string>();
        for (int i = 0; i < filter.Length; i++)
        {
            filterList.Add(filter[i].ToString().ToUpper()); 
        }

        foreach (var key in keys)
        {
            if (filterList.Contains(key.ToString()))
            {
                text += key.ToString();
            }
        }

        return text;
    }

    public void GoToMenu(MenuState state, byte goToIndex = 0)
    {
        if (menuState == MenuState.ControlProfiles)
        {
            newProfileName = "";
            profileIndex = 0;
        }
        else if(menuState == MenuState.Lobby)
        {
            profileIndex = 0;
        }
        
        menuState = state;
        menuIndex = goToIndex;
        selectedHorizontalOffsets = new float[GetMenuLength()];
        selectedHorizontalOffsets[menuIndex] = selectedHorizontalOffsetTotal;
    }
    
    //Toggle to next in list
    private byte ToggleNext<T>(T[] array, byte index)
    {
        if (index == array.Length - 1)
        {
            return 0;
        }

        return (byte)(index + 1);
    }

    //Toggle to previous in list
    private byte TogglePrevious<T>(T[] array, byte index)
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
    
    private double Increment(double value, double increment, double min, double max)
    {
        value += increment;
        return Util.Clamp(value, min, max);
    }

    private int GetMenuLength()
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                return Enum.GetNames(typeof(MainMenu)).Length;
            case MenuState.Lobby:
                return Enum.GetNames(typeof(Lobby)).Length;
            case MenuState.Settings:
                return Enum.GetNames(typeof(SettingsMenu)).Length;
            case MenuState.ControlProfiles:
                return main.settings.controlProfiles.Count+3; //3 extra for create new, delete, and back button
            case MenuState.Controls:
                return Enum.GetNames(typeof(ControlsMenu)).Length;
            case MenuState.MapKeys:
                return 20;
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

    //List out an entire array as a string
    public string ArrayListedAsString(Keys[] keys)
    {
        string text = "[";

        for (int i = 0; i < keys.Length; i++)
        {
            if (i > 0)
            {
                text += ", ";
            }
            text += keys[i].ToString();
        }

        text += "]";

        return text;
    }
    
    private string ArrayListedAsString(List<Controls> profiles)
    {
        string text = "[";

        for (int i = 0; i < profiles.Count; i++)
        {
            if (i > 0)
            {
                text += ", ";
            }
            text += profiles[i].controlName;
        }

        text += "]";

        return text;
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
        Back,
        Text,
    }
}

public enum MenuState
{
    MainMenu,
    Lobby,
    Settings,
    ControlProfiles,
    Controls,
    MapKeys,
}

public enum MainMenu
{
    Play,
    Settings,
    Quit,
}

public enum Lobby
{
    Start,
    GameMode,
    Profile,
    StartingLevel,
    GravityMultiplier,
    Back,
}

public enum SettingsMenu
{
    MasterVolume,
    SfxVolume,
    Controls,
    Back,
}

public enum ControlsMenu
{
    MoveLeft,
    MoveRight,
    SoftDrop,
    HardDrop,
    RotateClockWise,
    RotateCounterClockWise,
    Hold,
    Back
}