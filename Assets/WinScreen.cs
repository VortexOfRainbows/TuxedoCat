using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    public Image bg;
    public GameObject homeScreen;
    public GameObject TaiyakiUI;
    public TextMeshProUGUI TaiyakiText;
    public float FadeIn = 0;
    void FixedUpdate()
    {
        FadeIn += Time.fixedDeltaTime;
        homeScreen.SetActive(FadeIn > 4f);
        bg.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), Mathf.Min(1, FadeIn / 4f));
        TaiyakiUI.SetActive(Player.TaiyakiCollected > 0);
        TaiyakiText.text = $"{Player.TaiyakiCollected}/{Player.TaiyakiPossible}";
    }
}
