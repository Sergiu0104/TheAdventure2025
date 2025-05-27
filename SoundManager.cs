// SoundManager.cs
using System.Media;

namespace TheAdventure;

public static class SoundManager
{
    private static readonly SoundPlayer player = new();

    public static void InitAudio()
    {
    }

    public static void PlaySound(string filePath)
    {
        player.SoundLocation = filePath;
        player.Load();
        player.Play();
    }
}
