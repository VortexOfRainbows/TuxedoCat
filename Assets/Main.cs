using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class Main : MonoBehaviour
{
    public static bool Paused => UIButtons.gamePaused;
    public static Main Instance;
    public static Tilemap World => Instance.WorldMap;
    public static ColorAdjustments ColorAdjustments => Instance.c;
    public static Vignette Vignette => Instance.v;
    public GameObject TaiyakiUI;
    public TextMeshProUGUI TaiyakiText;
    public Volume Volume;
    public Tilemap WorldMap;
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
        TaiyakiUI.SetActive(Player.TaiyakiCollected > 0);
        TaiyakiText.text = $"{Player.TaiyakiCollected}/{Player.TaiyakiPossible}";
    }
    public static void SetVignetteIntensity(float i)
    {
        float value = 1 - i;
        Vignette.intensity.value = i;
        ColorAdjustments.colorFilter.value = new Color(value, value, value);
    }
}
