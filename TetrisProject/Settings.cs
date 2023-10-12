namespace TetrisProject;

public struct Settings
{
    //All settings that can be adjusted

    //Volumes
    public int masterVolume;
    public int soundEffectVolume;
    public int musicVolume;

    public Settings()
    {
        masterVolume = 100;
        soundEffectVolume = 100;
        musicVolume = 100;
    }
}