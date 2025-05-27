using System.Numerics;
using System;
namespace TheAdventure
{
    public class Particle
    {
        public Vector2 Position;
        private Vector2 _velocity;
        private float _life;

        public Particle(Vector2 position, Vector2 velocity)
        {
            Position = position;
            _velocity = velocity;
            _life = 0.5f;
        }

        public bool Update(float deltaTime)
        {
            Position += _velocity * deltaTime;
            _life -= deltaTime;
            return _life > 0;
        }

        public void Draw(GameRenderer renderer)
        {
            renderer.DrawCircle(Position, 0.05f, new Vector3(1f, 1f, 1f));
        }
    }
}
