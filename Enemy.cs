using System;
using System.Numerics;

namespace TheAdventure
{
    public class Enemy
    {
        public Vector2 Position { get; private set; }
        private Vector2 _velocity;
        private float _leftBound, _rightBound;
        private const float Speed = 1.5f;
        private const float DetectionRadius = 3f;
        private const float Radius = 0.2f;

        public Enemy(Vector2 position, float leftBound, float rightBound)
        {
            Position = position;
            _leftBound = leftBound;
            _rightBound = rightBound;
            _velocity = new Vector2(Speed, 0);
        }

        public void Update(float deltaTime, Vector2 playerPos)
        {
            if (Vector2.Distance(playerPos, Position) < DetectionRadius)
            {
                var dir = Vector2.Normalize(playerPos - Position);
                _velocity = dir * Speed;
            }
            else
            {
                if (Position.X <= _leftBound) _velocity = new Vector2(Speed, 0);
                if (Position.X >= _rightBound) _velocity = new Vector2(-Speed, 0);
            }

            Position += _velocity * deltaTime;
        }

        public void Draw(GameRenderer renderer)
        {
            renderer.DrawCircle(Position, Radius, new Vector3(1f, 0f, 0f));
        }
    }
}
