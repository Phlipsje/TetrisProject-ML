using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace TetrisProject;

public static class MusicManager
{
    private static SoundEffectInstance currentSong;
    private static bool isRepeating;
    private static float targetPitch;
    private static float originalPitch = 0;
    private static double targetPitchChangeTime = 1; // the time when the pitch should be fully change
    private static double PitchChangeStartTime = 1; // the time when the pitch started to change
    public static SoundEffect ClassicTheme;
    public static SoundEffect ModernTheme;
    private static Settings settings;
    
    public static void Load(ContentManager content)
    {
        ClassicTheme = content.Load<SoundEffect>("music/TetrisTheme");
        ModernTheme = content.Load<SoundEffect>("music/modern");
    }

    public static void Initialize(Settings settingsStruct)
    {
        settings = settingsStruct;
    }
    
    public static void PlaySong(SoundEffect song, bool songIsRepeating = true)
    {
        currentSong = song.CreateInstance();
        currentSong.Play();
        currentSong.Pitch = 0;
        targetPitch = 0;
        originalPitch = 0;
        
        isRepeating = songIsRepeating;
    }

    public static void Update(GameTime gameTime)
    {
        if (currentSong == null)
            return;
        if (currentSong.State == SoundState.Stopped && isRepeating)
            currentSong.Play();
        currentSong.Volume = (float)settings.musicVolume/100;
        if (gameTime.TotalGameTime.TotalMilliseconds == 0)
            return; // this could crash if this is 0 as some point so I'l do this just to be safe
        if (gameTime.TotalGameTime.TotalMilliseconds < targetPitchChangeTime)
        {
            double timeElapsed = gameTime.TotalGameTime.TotalMilliseconds - PitchChangeStartTime;
            double totalTime = targetPitchChangeTime - PitchChangeStartTime;
            currentSong.Pitch = (float)((targetPitch - originalPitch) * (timeElapsed / totalTime) + originalPitch);
        }
        else
        {
            currentSong.Pitch = targetPitch;
            Debug.WriteLine("normal");
        }
    }

    public static void Stop(GameTime gameTime)
    {
        currentSong.Stop();
        isRepeating = false;
    }
    
    public static void Pause()
    {
        currentSong.Pause();
    }

    public static void Resume()
    {
        currentSong.Resume();
    }

    public static void SetPitch(GameTime gameTime,  float pitch = -1, double delay = 500)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (pitch == targetPitch && delay != 0)
            return;
        targetPitch = pitch;
        originalPitch = currentSong.Pitch;
        PitchChangeStartTime = gameTime.TotalGameTime.TotalMilliseconds;
        targetPitchChangeTime = gameTime.TotalGameTime.TotalMilliseconds + delay;
    }

    public static void Normal(GameTime gameTime, double delay = 1000)
    {
        targetPitch = 0;
        originalPitch = currentSong.Pitch;
        PitchChangeStartTime = gameTime.TotalGameTime.TotalMilliseconds;
        targetPitchChangeTime = gameTime.TotalGameTime.TotalMilliseconds + delay;
    }
}