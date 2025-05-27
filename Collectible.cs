// File: Collectible.cs
using System;
using System.Numerics;
using System.Media;

namespace TheAdventure
{
    public class Collectible
    {
        public Vector2 Position { get; }
        public float Radius { get; } = 0.2f;
        public bool Collected { get; private set; }

        public Collectible(Vector2 pos)
        {
            Position = pos;
            Collected = false;
        }

        public void Draw(Action<Vector2, float> draw)
        {
            if (!Collected)
                draw(Position, Radius);
        }

        public void Collect()
        {
            Collected = true;
            using var sp = new SoundPlayer("Assets/coin.wav");
            sp.Play();
        }
    }
}
