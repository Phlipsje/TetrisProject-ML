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
    public Controls()
    {
        
    }
}