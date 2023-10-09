using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public static class Util
{
    //A class with extra utility functions shared with shared use cases among multiple classes
    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;
    private static Keys[] leftRightKeys = { Keys.Left, Keys.Right };

    private static Keys lastMovementKeyPressed;
    public static Keys LastMovementKeyPressed
    {
        get => lastMovementKeyPressed;
    }

    public static void Update()
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
        foreach (Keys key in currentKeyboardState.GetPressedKeys())
        {
            if (leftRightKeys.Contains(key) && GetKeyPressed(key))
                lastMovementKeyPressed = key;
        }

        if (currentKeyboardState.IsKeyDown(leftRightKeys[0]) && !currentKeyboardState.IsKeyDown(leftRightKeys[1]))
            lastMovementKeyPressed = leftRightKeys[0];
        else if (currentKeyboardState.IsKeyDown(leftRightKeys[1]) && !currentKeyboardState.IsKeyDown(leftRightKeys[0]))
            lastMovementKeyPressed = leftRightKeys[1];

    }
    
    //Check if key is only pressed on the current frame
    public static bool GetKeyPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
    }

    public static bool GetKeyLetGo(Keys key)
    {
        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
    }

    public static bool GetKeyHeld(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key);
    }

    //Shuffles the order of an array
    public static Pieces[] ShuffleArray(Pieces[] array)
    {
        Random random = new Random();
        for (int i = 0; i < array.Length; i++) //Shuffle amount
        {
            int randomIndex = random.Next(array.Length); //Get random number
            (array[0], array[randomIndex]) = (array[randomIndex], array[0]); //Swap indices of 2 values in array
        }

        return array;
    }
}