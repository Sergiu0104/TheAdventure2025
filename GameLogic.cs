// File: GameLogic.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using Silk.NET.Maths;
using TheAdventure.Models;
using TheAdventure.Models.Data;

namespace TheAdventure
{
    public class GameLogic
    {
        private readonly GameRenderer _renderer;
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<int, Tile> _tileIdMap = new();
        private Level _currentLevel = null!;
        private PlayerObject? _player;
        private DateTimeOffset _lastRenderTime;
        private readonly List<Particle> _particles = new();
        private DateTime _lastSpawn;
        private readonly Random _rand = new();

        public List<Collectible> collectibles { get; } = new();

        public GameLogic(GameRenderer renderer)
        {
            _renderer = renderer;
            _lastRenderTime = DateTimeOffset.Now;
            _lastSpawn = DateTime.Now;
        }

        public void InitializeGame()
        {
            _player = new PlayerObject(_renderer);

            var levelText = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));
            _currentLevel = JsonSerializer.Deserialize<Level>(levelText)
                ?? throw new Exception("Failed to parse level");

            foreach (var tsRef in _currentLevel.TileSets)
            {
                var tsText = File.ReadAllText(Path.Combine("Assets", tsRef.Source));
                var tileSet = JsonSerializer.Deserialize<TileSet>(tsText)
                    ?? throw new Exception("Failed to parse tileset");

                foreach (var tile in tileSet.Tiles)
                {
                    tile.TextureId = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                    _tileIdMap[tile.Id!.Value] = tile;
                }
            }

            if (!_currentLevel.Width.HasValue ||
                !_currentLevel.Height.HasValue ||
                !_currentLevel.TileWidth.HasValue ||
                !_currentLevel.TileHeight.HasValue)
                throw new Exception("Level dimensions missing");

            int worldW = _currentLevel.Width.Value * _currentLevel.TileWidth.Value;
            int worldH = _currentLevel.Height.Value * _currentLevel.TileHeight.Value;
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, worldW, worldH));

            collectibles.Add(new Collectible(new Vector2(0f, 1.5f)));
            collectibles.Add(new Collectible(new Vector2(1f, 1.5f)));
            collectibles.Add(new Collectible(new Vector2(-1f, 1.5f)));
        }

        public void ProcessFrame()
        {
            if (_player == null) return;

            float tileSize = 32f;
            var playerPos = new Vector2(_player.X / tileSize, _player.Y / tileSize);
            const float playerRadius = 1f;

            foreach (var c in collectibles)
            {
                if (!c.Collected)
                {
                    float d = Vector2.Distance(c.Position, playerPos);
                    if (d < c.Radius + playerRadius)
                    {
                        c.Collect();
                        for (int i = 0; i < 10; i++)
                        {
                            float ang = i * (2f * MathF.PI / 10f);
                            var vel = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 2f;
                            _particles.Add(new Particle(c.Position, vel));
                        }
                    }
                }
            }

            var now = DateTimeOffset.Now;
            float dt = (float)(now - _lastRenderTime).TotalSeconds;
            _lastRenderTime = now;
            _particles.RemoveAll(p => !p.Update(dt));

            if ((DateTime.Now - _lastSpawn).TotalSeconds >= 5 && collectibles.Count < 10)
            {
                float x = _rand.Next(-10, 10);
                float y = _rand.Next(-5, 5);
                collectibles.Add(new Collectible(new Vector2(x, y)));
                _lastSpawn = DateTime.Now;
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            if (_player != null)
                _renderer.CameraLookAt(_player.X, _player.Y);

            RenderTerrain();
            RenderAll();

            _renderer.PresentFrame();
        }

        private void RenderTerrain()
        {
            int w = _currentLevel.Width!.Value;
            int h = _currentLevel.Height!.Value;

            foreach (var layer in _currentLevel.Layers)
            {
                for (int i = 0; i < w; i++)
                    for (int j = 0; j < h; j++)
                    {
                        int idx = j * layer.Width + i;
                        var gidN = layer.Data[idx];
                        if (!gidN.HasValue) continue;
                        int gid = gidN.Value - 1;
                        if (!_tileIdMap.TryGetValue(gid, out var tile)) continue;

                        int tw = tile.ImageWidth ?? 0;
                        int th = tile.ImageHeight ?? 0;
                        var src = new Rectangle<int>(0, 0, tw, th);
                        var dst = new Rectangle<int>(i * tw, j * th, tw, th);
                        _renderer.RenderTexture(tile.TextureId, src, dst);
                    }
            }
        }

        private void RenderAll()
        {
            foreach (var go in _gameObjects.Values.OfType<RenderableGameObject>())
                go.Render(_renderer);

            _player?.Render(_renderer);

            foreach (var c in collectibles)
                c.Draw((p, r) => _renderer.DrawCircle(p, r, new Vector3(1f, 0.84f, 0f)));

            foreach (var p in _particles)
                p.Draw((p, r, col) => _renderer.DrawCircle(p, r, col));
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int ms)
            => _player?.UpdatePosition(up, down, left, right, ms);

        public void AddBomb(int sx, int sy)
        {
            var w = _renderer.ToWorldCoordinates(sx, sy);
            var bomb = new AnimatedGameObject(
                Path.Combine("Assets", "BombExploding.png"),
                _renderer, 2, 13, 13, 1, w.X, w.Y);
            _gameObjects[bomb.Id] = bomb;
        }
    }
}
