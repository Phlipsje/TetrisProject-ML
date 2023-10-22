using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

//Part of main menu, is running while the player is remapping a key of an input
public class SelectingKeys
{
    private Main main;
    private Menu menu;
    private ControlsMenu edittingInputType; //What type of input is being mapped
    private int profileIndex;
    private List<Keys> mappedKeys = new();
    private bool firstFrame = true;

    public SelectingKeys(Main main, Menu menu, int profileIndex, ControlsMenu inputType)
    {
        this.main = main;
        this.menu = menu;
        this.profileIndex = profileIndex;
        edittingInputType = inputType;
    }
    
    //This class is used when keys are being assigned to keybinds
    public void Update()
    {
        //Return on first frame, because enter will otherwise instantly be pressed for the first frame and result in exiting this class
        if (firstFrame)
        {
            firstFrame = false;
            return;
        }
        
        //Check if should exit mapping keys
        if (Util.GetKeyPressed(Keys.Enter))
        {
            Controls editingProfile = main.settings.controlProfiles[profileIndex];
            switch (edittingInputType)
            {
                case ControlsMenu.MoveLeft:
                    editingProfile.leftKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.MoveRight:
                    editingProfile.rightKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.SoftDrop:
                    editingProfile.softDropKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.HardDrop:
                    editingProfile.hardDropKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.RotateClockWise:
                    editingProfile.rotateClockWiseKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.RotateCounterClockWise:
                    editingProfile.rotateCounterClockWiseKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
                case ControlsMenu.Hold:
                    editingProfile.holdKey = mappedKeys.ToArray();
                    main.settings.controlProfiles[profileIndex] = editingProfile;
                    break;
            }

            //Go back to overview of all controls in a profile
            menu.menuState = MenuState.Controls;
            menu.selectingKeys = null; //Dispose of this class
            return;
        }

        //Return if no keys pressed, or enter is still held down
        Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
        if (pressedKeys == null || pressedKeys.Contains(Keys.Enter))
        {
            return;
        }
        
        //Add key to list of keys mapped to input
        foreach (var key in pressedKeys)
        {
            if (!mappedKeys.Contains(key))
            {
                mappedKeys.Add(key);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D tile, SpriteFont font)
    {
        //Background
        spriteBatch.Draw(tile, new Rectangle(400, 200, 1120, 680), Color.Gray);
        
        //Draw text showing what input type is being edited
        spriteBatch.DrawString(font, InputTypeToString(edittingInputType), new Vector2(500, 300), Color.White);

        //Draw list of keys being added to an input
        spriteBatch.DrawString(font, menu.ArrayListedAsString(mappedKeys.ToArray()), new Vector2(500, 380), Color.White);
        
        //Draw tooltip
        spriteBatch.DrawString(font, "Press enter to confirm", new Vector2(500, 460), Color.White);
    }

    //Converts an input type to text (adds spaces)
    private string InputTypeToString(ControlsMenu inputType)
    {
        switch (inputType)
        {
            case ControlsMenu.MoveLeft:
                return "Move Left";
            case ControlsMenu.MoveRight:
                return "Move Right";
            case ControlsMenu.SoftDrop:
                return "Soft Drop";
            case ControlsMenu.HardDrop:
                return "Hard Drop";
            case ControlsMenu.RotateClockWise:
                return "Rotate Clockwise";
            case ControlsMenu.RotateCounterClockWise:
                return "Rotate Counterclockwise";
            case ControlsMenu.Hold:
                return "Hold";
            default:
                return "";
        }
    }
}