using Silk.NET.Maths;

namespace TheAdventure;

public class GameLogic
{
    private readonly List<RenderableGameObject> _gameObjects = new();
    private Vector2D<int>? _targetPosition = null;

    private int _animationFrame = 0;
    private int _frameCounter = 0;
    private const int FramesPerAnimationStep = 10;
    private const int TotalAnimationFrames = 4;
    private const int EnemyVisionDistance = 200;
    private const int EnemyStepSize = 2;

    public void InitializeGame(GameRenderer gameRenderer)
    {
        var textureId = gameRenderer.LoadTexture("image.png", out var textureInfo);
        var frameWidth = textureInfo.Width / TotalAnimationFrames;

        var player = new RenderableGameObject(textureId,
            new Rectangle<int>(0, 0, frameWidth, textureInfo.Height),
            new Rectangle<int>(new Vector2D<int>(100, 100), new Vector2D<int>(100, 100)), textureInfo)
        {
            Type = GameObjectType.Player,
            Health = 100
        };
        _gameObjects.Add(player);

        for (int i = 0; i < 3; i++)
        {
            var enemy = new RenderableGameObject(textureId,
                new Rectangle<int>(0, 0, frameWidth, textureInfo.Height),
                new Rectangle<int>(new Vector2D<int>(300 + i * 120, 300), new Vector2D<int>(100, 100)), textureInfo)
            {
                Type = GameObjectType.Enemy,
                Health = 50
            };
            _gameObjects.Add(enemy);
        }
    }

    public void ProcessFrame()
    {
        var player = _gameObjects.FirstOrDefault(o => o.Type == GameObjectType.Player);
        if (player == null || !player.IsAlive) return;

        bool isPlayerMoving = false;

        if (_targetPosition.HasValue)
        {
            var currentPos = player.TextureDestination.Origin;
            var size = player.TextureDestination.Size;
            var target = _targetPosition.Value - size / 2;

            var direction = new Vector2D<float>(target.X - currentPos.X, target.Y - currentPos.Y);
            float length = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (length > 1f)
            {
                direction /= length;
                int stepSize = 3;
                var step = new Vector2D<int>((int)(direction.X * stepSize), (int)(direction.Y * stepSize));
                player.TextureDestination = new Rectangle<int>(currentPos + step, size);
                isPlayerMoving = true;
            }
            else
            {
                player.TextureDestination = new Rectangle<int>(target, size);
                _targetPosition = null;
            }
        }

        if (isPlayerMoving)
        {
            _frameCounter++;
            if (_frameCounter >= FramesPerAnimationStep)
            {
                _frameCounter = 0;
                _animationFrame = (_animationFrame + 1) % TotalAnimationFrames;
            }
        }
        else
        {
            _animationFrame = 0;
            _frameCounter = 0;
        }

        int frameWidth = player.TextureInformation.Width / TotalAnimationFrames;

        foreach (var obj in _gameObjects)
        {
            obj.TextureSource = new Rectangle<int>(
                frameWidth * _animationFrame,
                0,
                frameWidth,
                obj.TextureInformation.Height
            );
        }

        foreach (var enemy in _gameObjects.Where(o => o.Type == GameObjectType.Enemy && o.IsAlive))
        {
            var playerCenter = player.TextureDestination.Center;
            var enemyCenter = enemy.TextureDestination.Center;
            var direction = new Vector2D<float>(playerCenter.X - enemyCenter.X, playerCenter.Y - enemyCenter.Y);
            float distance = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

            if (distance < EnemyVisionDistance)
            {
                if (distance < 100)
                {
                    direction *= -1;
                }

                direction /= MathF.Max(distance, 1f);
                var step = new Vector2D<int>((int)(direction.X * EnemyStepSize), (int)(direction.Y * EnemyStepSize));
                enemy.TextureDestination = new Rectangle<int>(enemy.TextureDestination.Origin + step, enemy.TextureDestination.Size);
            }

            if (distance < 30)
            {
                player.Health -= 1;
            }
        }

        _gameObjects.RemoveAll(o => o.Type == GameObjectType.Enemy && !o.IsAlive);
    }

    public IEnumerable<RenderableGameObject> GetRenderables()
    {
        return _gameObjects.Where(obj => obj.IsAlive);
    }

    public void MovePlayer((bool up, bool down, bool left, bool right) movementInput)
    {
        var player = _gameObjects.FirstOrDefault(o => o.Type == GameObjectType.Player);
        if (player == null || !player.IsAlive) return;

        int deltaX = 0, deltaY = 0;
        int speed = 5;

        if (movementInput.up) deltaY -= speed;
        if (movementInput.down) deltaY += speed;
        if (movementInput.left) deltaX -= speed;
        if (movementInput.right) deltaX += speed;

        var newPos = new Vector2D<int>(player.TextureDestination.Origin.X + deltaX, player.TextureDestination.Origin.Y + deltaY);
        player.TextureDestination = new Rectangle<int>(newPos, player.TextureDestination.Size);

        if (deltaX != 0 || deltaY != 0)
            _targetPosition = null;
    }

    public void MovePlayerToPosition(int mouseX, int mouseY)
    {
        _targetPosition = new Vector2D<int>(mouseX, mouseY);
    }
}
