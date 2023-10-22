using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace TetrisProject;

public static class SfxManager
{
    private static List<SoundEffectInstance> playingSoundEffects = new ();
    public static SoundEffect Explosion;
    public static SoundEffect LockPiece;
    public static void Load(ContentManager content)
    {
        Explosion = content.Load<SoundEffect>("sfx/explosion");
        LockPiece = content.Load<SoundEffect>("sfx/LockPiece");
    }

    public static void Play(SoundEffect soundEffect)
    {
        SoundEffectInstance sound = soundEffect.CreateInstance();
        sound.Play();
        
        //Keep track of what sound effects are being played
        playingSoundEffects.Add(sound);
    }

    public static void Update()
    {
        //Update list of playing sound effects
        for (int i = 0; i < playingSoundEffects.Count; i++)
        {
            if (playingSoundEffects[i].IsDisposed)
            {
                playingSoundEffects.RemoveAt(i);
                i--;
            }
        }
    }

    
    //Cancel all sounds
    public static void StopAllSoundEffects()
    {
        foreach (SoundEffectInstance sound in playingSoundEffects)
        {
            sound.Stop();
        }

        playingSoundEffects = new List<SoundEffectInstance>();
    }
}