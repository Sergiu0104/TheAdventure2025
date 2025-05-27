namespace TheAdventure;

public enum GameObjectType
{
    Player,
    Enemy
}

public class GameObject
{
    public GameObjectType Type { get; set; }
    public int Health { get; set; } = 100;
    public bool IsAlive => Health > 0;
}
