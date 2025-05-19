using System.Collections.Generic;
using UnityEngine;
public static class SoundID
{
    public static SoundClip Blade = new("Blade1", "Blade2", "Blade3");
    public static SoundClip Explosion = new("Explosion1", "Explosion2");
    public static SoundClip Laser = new("Laser");
}
public class SoundClip
{
    private const string audioPath = "Audio/";
    private string myPath;
    private List<AudioClip> variations = new();
    private AudioClip Fetch(string audioFileName)
    {
        return Resources.Load<AudioClip>($"{myPath}{audioFileName}");
    }
    /// <summary>
    /// Creates a new sound clip
    /// </summary>
    /// <param name="path">The path to the sound clip inside the Resources/Audio/ directory</param>
    /// <param name="args">A list of the sound clips that are considered variants of each other</param>
    public SoundClip(params string[] args)
    {
        myPath = audioPath;
        foreach(string s in args)
        {
            variations.Add(Fetch(s));
        }
    }
    public AudioClip GetRandom()
    {
        return GetVariation(Utils.RandInt(variations.Count));
    }
    public AudioClip GetVariation(int variation = -1)
    {
        if (variation <= -1)
            return GetRandom();
        return variations[variation];
    }
}
