using System;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public static class Util
{
    //A class with extra utility functions shared with shared use cases among multiple classes
    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;

    public static void Update()
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
    }
    
    //Check if key is only pressed on the current frame
    public static bool GetKeyPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
    }

    //Shuffles the order of an array
    public static byte[] ShuffleArray(byte[] array)
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