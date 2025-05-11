using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Main : MonoBehaviour
{
    public static bool Paused => UIButtons.gamePaused;
    public static Main Instance;
    public static ColorAdjustments ColorAdjustments => Instance.c;
    public static Vignette Vignette => Instance.v;
    public Volume Volume;
    private Vignette v;
    private ColorAdjustments c;
    public void Start()
    {
        Instance = this;
        v = Volume.profile.components[0] as Vignette;
        c = Volume.profile.components[1] as ColorAdjustments;
    }
    public void Update()
    {
        Instance = this;
        v = Volume.profile.components[0] as Vignette;
        c = Volume.profile.components[1] as ColorAdjustments;
    }
    public static void SetVignetteIntensity(float i)
    {
        float value = 1 - i;
        Vignette.intensity.value = i;
        ColorAdjustments.colorFilter.value = new Color(value, value, value);
    }
}
