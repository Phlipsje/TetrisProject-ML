using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

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
}