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
}