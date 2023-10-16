using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace TetrisProject;

[Serializable]
public struct Settings
{
    //All settings that can be adjusted

    //Volumes
    public int masterVolume { get; set; }
    public int soundEffectVolume{ get; set; }
    public int musicVolume { get; set; }
    
    public List<Controls> controlProfiles { get; set; }

    public GameRules game;
    //public List<Controls> controlProfiles = new List<Controls>();

    public Settings()
    {
        masterVolume = 100;
        soundEffectVolume = 100;
        musicVolume = 100;

        game = new GameRules();
        controlProfiles = new List<Controls>();
        controlProfiles.Add(new Controls()); //Default
        //Rest of controls get imported with json when that is added
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
    public string controlName { get; set; } //Maybe save this somewhere else
    public Keys[] leftKey { get; set; }
    public Keys[] rightKey { get; set; }
    public Keys[] softDropKey { get; set; }
    public Keys[] hardDropKey { get; set; }
    public Keys[] rotateClockWiseKey { get; set; }
    public Keys[] rotateCounterClockWiseKey { get; set; }
    public Keys[] holdKey { get; set; }
    
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