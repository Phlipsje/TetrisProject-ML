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
    public bool useClassicMusic { get; set; }

    public List<Controls> controlProfiles { get; set; }

    //GameRules are not saved upon closing the application
    public GameRules game;
    public int highScore { get; set; }
    public Settings()
    {
        masterVolume = 100;
        soundEffectVolume = 100;
        musicVolume = 70;
        useClassicMusic = false;
        highScore = 0;

        game = new GameRules();
        controlProfiles = new List<Controls>();
        controlProfiles.Add(new Controls()); //Default
    }
}

//All game settings (some gamemodes do not make use of all variables)
public struct GameRules
{
    public int startingLevel;
    public double gravityMultiplier;
    public int linesToWin;
    public double garbageMultiplier;
    public byte width;

    public GameRules()
    {
        //Default values when starting a match
        startingLevel = 1;
        gravityMultiplier = 1;
        linesToWin = 30;
        garbageMultiplier = 1;
        width = 10;
    }
}

public struct Controls
{
    public string controlName { get; set; }
    public Keys[] leftKey { get; set; }
    public Keys[] rightKey { get; set; }
    public Keys[] softDropKey { get; set; }
    public Keys[] hardDropKey { get; set; }
    public Keys[] rotateClockWiseKey { get; set; }
    public Keys[] rotateCounterClockWiseKey { get; set; }
    public Keys[] holdKey { get; set; }
    
    public Controls()
    {
        //Default control scheme
        controlName = "Default";
        leftKey = new [] {Keys.A, Keys.Left};
        rightKey = new [] {Keys.D, Keys.Right};
        softDropKey = new [] {Keys.S, Keys.Down};
        hardDropKey = new [] {Keys.Space};
        rotateClockWiseKey = new[] { Keys.W, Keys.X, Keys.E, Keys.Up };
        rotateCounterClockWiseKey = new [] { Keys.Z, Keys.Q };
        holdKey = new[] { Keys.LeftShift, Keys.RightShift };
    }

    public static Controls Empty()
    {
        Controls controls = new Controls();
        controls.controlName = "Empty";
        controls.leftKey = new [] {Keys.A, Keys.Left};
        controls.rightKey = new [] {Keys.D, Keys.Right};
        controls.softDropKey = new [] {Keys.S, Keys.Down};
        controls.hardDropKey = new [] {Keys.Space};
        controls.rotateClockWiseKey = new[] { Keys.W, Keys.X, Keys.E, Keys.Up };
        controls.rotateCounterClockWiseKey = new [] { Keys.Z, Keys.Q };
        controls.holdKey = new[] { Keys.LeftShift, Keys.RightShift };
        return controls;
    }
}