using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetrisProject;

public static class AnimationManager
{
    private static List<Animation> animations = new List<Animation>();

    public static void Reset()
    {
        animations = new List<Animation>();
    }

    public static void PlayAnimation(Animation animation)
    {
        animations.Add(animation);
    }

    public static void Update(GameTime gameTime)
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].Update(gameTime);
            if (animations[i].CanBeDestroyed)
            {
                animations.RemoveAt(i);
                i--;
            }
        }
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        foreach (Animation animation in animations)
        {
            animation.Draw(spriteBatch);
        }
    }
}

public abstract class Animation
{
    public bool CanBeDestroyed;
    protected Vector2 position;
    public Vector2 Position => position;
    protected TetrisGame tetrisGame;

    protected Animation(Vector2 startPosition, TetrisGame tetrisGame)
    {
        this.position = startPosition;
        this.tetrisGame = tetrisGame;
    }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}

public class FallingBlockAnimation : Animation
{
    private Vector2 velocity;
    private float gravity;
    private Vector2 size;
    private Texture2D texture;
    private Color color;
    private float rotation;
    private float rotationSpeed;
    
    public FallingBlockAnimation(Vector2 startPosition, TetrisGame tetrisGame, Vector2 startVelocity, Texture2D texture,
        float rotationSpeed, Color? color = null, Vector2? size = null, float gravity = 2000) : base(startPosition, tetrisGame)
    {
        velocity = startVelocity;
        // The syntax of this line: the value before the ?? if it is not null, else the value after the ??
        this.size = size ?? new Vector2(texture.Width, texture.Height);
        this.color = color ?? Color.White;
        this.gravity = gravity;
        this.texture = texture;
        this.rotationSpeed = rotationSpeed;
    }

    public override void Update(GameTime gameTime)
    {
        position = position + velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity.Y += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        rotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (position.Y > Main.WindowHeight)
            CanBeDestroyed = true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle drawRect = new Rectangle(Util.GetFullscreenSafePosition(position, tetrisGame).ToPoint(), 
            Util.GetFullscreenSafePosition(size, tetrisGame).ToPoint());
        spriteBatch.Draw(texture, drawRect, null, color, rotation, size / 2, SpriteEffects.None, 1);
    }
}