using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public static float MasterVolume = 1;
    public TextMeshProUGUI VolumePercentText;
    public AudioMixer Mixer;
    public Slider AudioSlider;
    public void SetVolume(float f)
    {
        if (f <= 0)
            f = 0.00001f;
        MasterVolume = f;
        Mixer.SetFloat("masterVol", Mathf.Log10(f) * 20);
        int p = (int)(f * 100);
        string s = p == 0 ? "0" : p.ToString("###");
        VolumePercentText.text = s + "%";

        AudioSlider.SetValueWithoutNotify(f);
    }
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
    public Image CheeseBarImage;
    public GameObject CheeseBarParent;
    public AudioSource MusicSource;
    private bool PrevFightInit = false;
    private float MusicSwitchTimer = 0;
    public bool IsMainMenu = false;
    public void Start()
    {
        SetVolume(MasterVolume);
        if (IsMainMenu)
            return;
        Instance = this;
        v = Volume.profile.components[0] as Vignette;
        c = Volume.profile.components[1] as ColorAdjustments;
    }
    public void Update()
    {
        if (IsMainMenu)
            return;
        Instance = this;
        v = Volume.profile.components[0] as Vignette;
        c = Volume.profile.components[1] as ColorAdjustments;
        TaiyakiUI.SetActive(Player.TaiyakiCollected > 0);
        TaiyakiText.text = $"{Player.TaiyakiCollected}/{Player.TaiyakiPossible}";
    }
    public void FixedUpdate()
    {
        if (IsMainMenu)
            return;
        if (PrevFightInit == BiggieCheese.FightInitiated)
        {
            MusicSwitchTimer += Time.fixedDeltaTime * 0.5f;
            MusicSource.volume = Mathf.Min(MusicSwitchTimer, 1);
        }
        else if(MusicSwitchTimer >= 1)
        {
            MusicSwitchTimer = 0;
        }
        if (BiggieCheese.FightInitiated)
        {
            CheeseBarParent.transform.localPosition = CheeseBarParent.transform.localPosition.Lerp(Vector3.up * 540, 0.05f);
            if(PrevFightInit != BiggieCheese.FightInitiated)
            {
                MusicSwitchTimer += Time.fixedDeltaTime * 0.5f;
                MusicSource.volume = Mathf.Max(1 - MusicSwitchTimer, 0);
                if (MusicSwitchTimer > 1)
                {
                    MusicSource.Stop();
                    MusicSource.clip = Resources.Load<AudioClip>("Audio/BossFight");
                    MusicSource.Play();
                    PrevFightInit = BiggieCheese.FightInitiated;
                    MusicSwitchTimer = 0;
                }
            }
        }
        else
        {
            CheeseBarParent.transform.localPosition = CheeseBarParent.transform.localPosition.Lerp(Vector3.up * 740, 0.05f);
            if (PrevFightInit != BiggieCheese.FightInitiated)
            {
                MusicSwitchTimer += Time.fixedDeltaTime * 0.5f;
                MusicSource.volume = Mathf.Max(1 - MusicSwitchTimer, 0);
                if (MusicSwitchTimer > 1)
                {
                    MusicSource.Stop();
                    MusicSource.clip = Resources.Load<AudioClip>("Audio/SuspensefulInvestigation");
                    MusicSource.Play();
                    PrevFightInit = BiggieCheese.FightInitiated;
                    MusicSwitchTimer = 0;
                }
            }
        }
    }
    public static void SetVignetteIntensity(float i)
    {
        float value = 1 - i;
        Vignette.intensity.value = i;
        ColorAdjustments.colorFilter.value = new Color(value, value, value);
    }
}
