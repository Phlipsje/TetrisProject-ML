using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

public struct Settings
{
    //All settings that can be adjusted

    //Volumes
    public int masterVolume;
    public int soundEffectVolume;
    public int musicVolume;

    public GameRules game;
    public Controls controls;

    public Settings()
    {
        masterVolume = 100;
        soundEffectVolume = 100;
        musicVolume = 100;

        game = new GameRules();
        controls = new Controls();
    }
}

//All game settings
public struct GameRules
{
    public int startingLevel;
    public double gravityMultiplier;

    public GameRules()
    {
        startingLevel = 1;
        gravityMultiplier = 1;
    }
}

public struct Controls
{
    public string controlName; //Maybe save this somewhere else
    public Keys[] leftKey;
    public Keys[] rightKey;
    public Keys[] softDropKey;
    public Keys[] hardDropKey;
    public Keys[] rotateClockWiseKey;
    public Keys[] rotateCounterClockWiseKey;
    public Keys[] holdKey;
    
    public Controls()
    {
        controlName = "Default";
        leftKey = new [] {Keys.A, Keys.Left};
        rightKey = new [] {Keys.D, Keys.Right};
        softDropKey = new [] {Keys.S, Keys.Down};
        hardDropKey = new [] {Keys.Space};
        rotateClockWiseKey = new[] { Keys.X, Keys.E, Keys.Up };
        rotateCounterClockWiseKey = new [] { Keys.Z, Keys.Q };
        holdKey = new[] { Keys.LeftShift, Keys.RightShift };
    }
}