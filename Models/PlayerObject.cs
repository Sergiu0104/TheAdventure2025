using System;
using System.Numerics;

namespace TheAdventure
{
    public class PlayerObject
    {
        private readonly GameRenderer _renderer;
        public int X { get; private set; }
        public int Y { get; private set; }

        private float _velY;
        private bool _isGrounded;
        private const float Gravity = -20f;
        private const float JumpVelocity = 8f;
        private const float MoveSpeed = 100f;
        private const int GroundY = 0;

        public PlayerObject(GameRenderer renderer)
        {
            _renderer = renderer;
            X = 0; Y = GroundY;
            _velY = 0;
            _isGrounded = true;
        }

        public void ResetPosition()
        {
            X = 0; Y = GroundY;
            _velY = 0; _isGrounded = true;
        }

        public void UpdatePosition(double up, double down, double left, double right, int dt)
        {
            float delta = dt / 1000f;
            X = (int)(X + ((float)right - (float)left) * MoveSpeed * delta);
            if (up > 0 && _isGrounded)
            {
                _velY = JumpVelocity;
                _isGrounded = false;
            }
            _velY += Gravity * delta;
            Y = (int)(Y + _velY * delta);
            if (Y < GroundY)
            {
                Y = GroundY;
                _velY = 0;
                _isGrounded = true;
            }
        }

        public void Render(GameRenderer renderer)
        {
            var tilePos = new Vector2(X / 32f, Y / 32f);
            renderer.DrawCircle(tilePos, 0.4f, new Vector3(0f, 1f, 0f));
        }
    }
}
