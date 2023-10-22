using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public static class Util
{
    //A class with extra utility functions shared with shared use cases among multiple classes
    private static KeyboardState currentKeyboardState;
    private static KeyboardState previousKeyboardState;

    public static void Update()
    {
        //Update keyboard states
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();
    }

    //Returns a list of types of inputs based on the players control scheme
    private static List<Input> GetInputsPressed(Controls controls)
    {
        List<Input> inputs = new List<Input>();
        Keys[] currentKeysHeld = currentKeyboardState.GetPressedKeys();
        Keys[] previousKeysHeld = previousKeyboardState.GetPressedKeys();
        List<Keys> keysPressed = new List<Keys>();

        for (int i = 0; i < currentKeysHeld.Length; i++)
        {
            if (!previousKeysHeld.Contains(currentKeysHeld[i]))
            {
                keysPressed.Add(currentKeysHeld[i]);
            }
        }

        foreach (var key in keysPressed)
        {
            if (controls.leftKey.Contains(key))
            {
                inputs.Add(Input.Left);
            }
            if (controls.rightKey.Contains(key))
            {
                inputs.Add(Input.Right);
            }
            if (controls.softDropKey.Contains(key))
            {
                inputs.Add(Input.SoftDrop);
            }
            if (controls.hardDropKey.Contains(key))
            {
                inputs.Add(Input.HardDrop);
            }
            if (controls.rotateClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateClockWise);
            }
            if (controls.rotateCounterClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateCounterClockWise);
            }
            if (controls.holdKey.Contains(key))
            {
                inputs.Add(Input.Hold);
            }
        }

        return inputs;
    }

    //Returns if a type of input is pressed
    public static bool GetKeyPressed(Input inputType, Controls controls)
    {
        if (GetInputsPressed(controls).Contains(inputType))
        {
            return true;
        }
        
        return false;
    }
    
    //Check if key is only pressed on the current frame
    public static bool GetKeyPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
    }
    
    //Returns a list of types of inputs based on the players control scheme
    private static List<Input> GetInputsHeld(Controls controls)
    {
        List<Input> inputs = new List<Input>();

        foreach (var key in currentKeyboardState.GetPressedKeys())
        {
            if (controls.leftKey.Contains(key))
            {
                inputs.Add(Input.Left);
            }
            if (controls.rightKey.Contains(key))
            {
                inputs.Add(Input.Right);
            }
            if (controls.softDropKey.Contains(key))
            {
                inputs.Add(Input.SoftDrop);
            }
            if (controls.hardDropKey.Contains(key))
            {
                inputs.Add(Input.HardDrop);
            }
            if (controls.rotateClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateClockWise);
            }
            if (controls.rotateCounterClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateCounterClockWise);
            }
            if (controls.holdKey.Contains(key))
            {
                inputs.Add(Input.Hold);
            }
        }

        return inputs;
    }
    
    //Returns if a type of input is held down
    public static bool GetKeyHeld(Input inputType, Controls controls)
    {
        if (GetInputsHeld(controls).Contains(inputType))
        {
            return true;
        }
        
        return false;
    }
    
    //Returns if a key is being held down
    public static bool GetKeyHeld(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key);
    }

    //Returns all inputs that have been let go
    private static List<Input> GetInputsLetGo(Controls controls)
    {
        List<Input> inputs = new List<Input>();
        Keys[] currentKeysHeld = currentKeyboardState.GetPressedKeys();
        Keys[] previousKeysHeld = previousKeyboardState.GetPressedKeys();
        List<Keys> keysLetGo = new List<Keys>();

        for (int i = 0; i < previousKeysHeld.Length; i++)
        {
            if (!currentKeysHeld.Contains(previousKeysHeld[i]))
            {
                keysLetGo.Add(previousKeysHeld[i]);
            }
        }

        foreach (var key in keysLetGo)
        {
            if (controls.leftKey.Contains(key))
            {
                inputs.Add(Input.Left);
            }
            if (controls.rightKey.Contains(key))
            {
                inputs.Add(Input.Right);
            }
            if (controls.softDropKey.Contains(key))
            {
                inputs.Add(Input.SoftDrop);
            }
            if (controls.hardDropKey.Contains(key))
            {
                inputs.Add(Input.HardDrop);
            }
            if (controls.rotateClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateClockWise);
            }
            if (controls.rotateCounterClockWiseKey.Contains(key))
            {
                inputs.Add(Input.RotateCounterClockWise);
            }
            if (controls.holdKey.Contains(key))
            {
                inputs.Add(Input.Hold);
            }
        }

        return inputs;
    }
    
    //Returns if a type of input is let go
    public static bool GetKeyLetGo(Input inputType, Controls controls)
    {
        if (GetInputsLetGo(controls).Contains(inputType))
        {
            return true;
        }
        
        return false;
    }
    
    //Returns if a type of key is let go
    public static bool GetKeyLetGo(Keys key)
    {
        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
    }

    //Get all keys that are pressed
    public static Keys[] GetKeysPressed()
    {
        List<Keys> keys = new List<Keys>();
        
        foreach (var key in currentKeyboardState.GetPressedKeys())
        {
            if (!previousKeyboardState.GetPressedKeys().Contains(key))
            {
                keys.Add(key);
            }
        }

        return keys.ToArray();
    }

    //Shuffles the order of an array
    public static Pieces[] ShuffleArray(Pieces[] array)
    {
        Random random = new Random();
        
        //Shuffle amount is equal to array length
        for (int i = 0; i < array.Length; i++) 
        {
            //Get random number
            int randomIndex = random.Next(array.Length); 
            
            //Swap indices of 2 values in array
            (array[0], array[randomIndex]) = (array[randomIndex], array[0]); 
        }

        return array;
    }

    //MathHelper.Clamp but for doubles
    public static double Clamp(double value, double min, double max)
    {
        if (value <= min)
        {
            return min;
        }

        if (value >= max)
        {
            return max;
        }

        return value;
    }

    public static void SaveSettingsToFile(Settings obj, string path)
    {
        string serializedText = JsonSerializer.Serialize<Settings>(obj);
        File.WriteAllText(path, serializedText);
    }

    public static Settings LoadSettingsFromFile(string filename)
    {
        string serializedText = File.ReadAllText(filename);
        try
        {
            return JsonSerializer.Deserialize<Settings>(serializedText);
        }
        catch (JsonException)
        {
            return new Settings();
        }
    }

}

public enum Input
{
    Left,
    Right,
    SoftDrop,
    HardDrop,
    RotateClockWise,
    RotateCounterClockWise,
    Hold
}