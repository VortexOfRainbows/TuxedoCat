using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    public static bool InDialogue = false;
    public static Dialogue Instance;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image leftBox;
    public Image rightBox;
    public Image leftPortrait;
    public Image rightPortrait;
    public Image boxDimensions;
    public DialoguePart[] DialogueParts;
    public float textSpeed;
    public int index;
    public Vector2 cameraCenter;
    public float LoadInTime = 0;
    public bool leftIsTalking;
    public bool disengaging = false;
    public void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    public static void Reset(DialoguePart[] d, Vector2 camera)
    {
        Instance.gameObject.SetActive(true);
        Instance.index = 0;
        Instance.DialogueParts = d;
        Instance.nameText.text = d[0].Name;
        Instance.dialogueText.text = string.Empty;
        Instance.cameraCenter = camera;
        Instance.UpdateImages(0);
        Instance.StartDialogue();
    }
    public void Update()
    {
        Instance = this;
        if(gameObject.activeSelf || disengaging)
        {
            Time.timeScale = 0f;
            Camera.main.transform.position = Camera.main.transform.position.Lerp(new Vector3(cameraCenter.x, cameraCenter.y, Camera.main.transform.position.z), 0.05f * Time.unscaledDeltaTime * 100f);
            UpdatePos();
            if (Input.GetMouseButtonDown(0) && !Main.Paused)
            {
                if (dialogueText.text == DialogueParts[index].Text)
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    dialogueText.text = DialogueParts[index].Text;
                }
            }
        }
    }
    public float AnimSpeed = 0.4f;
    public void UpdatePos()
    {
        LoadInTime += Time.unscaledDeltaTime;
        if (!disengaging && LoadInTime > AnimSpeed)
            LoadInTime = AnimSpeed;
        else if(disengaging && LoadInTime > AnimSpeed * 2)
        {
            EndDialogue();
        }
        RectTransform r1 = GetComponent<RectTransform>();
        float percent = Mathf.Min(LoadInTime / AnimSpeed, 2);
        percent = Mathf.Sin(percent * Mathf.PI / 2f);
        r1.position = Vector3.Lerp(new Vector3(r1.position.x, -180, r1.position.z), new Vector3(r1.position.x, -550, r1.position.z), Mathf.Max(1 - percent, 0));
        float lerpAmt = Time.unscaledDeltaTime / Time.fixedUnscaledDeltaTime * 0.1f;
        float topPos = 350;
        float botPos = 300;
        if (leftIsTalking)
        {
            rightPortrait.color = rightBox.color = Color.Lerp(rightPortrait.color, Color.gray, lerpAmt);
            leftPortrait.color = leftBox.color = Color.Lerp(leftPortrait.color, Color.white, lerpAmt);
            rightBox.rectTransform.localPosition =
                rightBox.rectTransform.localPosition.Lerp(new Vector3(rightBox.rectTransform.localPosition.x, botPos, rightBox.rectTransform.localPosition.z), lerpAmt);
            leftBox.rectTransform.localPosition =
                leftBox.rectTransform.localPosition.Lerp(new Vector3(leftBox.rectTransform.localPosition.x, topPos, leftBox.rectTransform.localPosition.z), lerpAmt);
        }
        else
        {
            rightPortrait.color = rightBox.color = Color.Lerp(rightPortrait.color, Color.white, lerpAmt);
            leftPortrait.color = leftBox.color = Color.Lerp(leftPortrait.color, Color.gray, lerpAmt);
            rightBox.rectTransform.localPosition =
                rightBox.rectTransform.localPosition.Lerp(new Vector3(rightBox.rectTransform.localPosition.x, topPos, rightBox.rectTransform.localPosition.z), lerpAmt);
            leftBox.rectTransform.localPosition =
                leftBox.rectTransform.localPosition.Lerp(new Vector3(leftBox.rectTransform.localPosition.x, botPos, leftBox.rectTransform.localPosition.z), lerpAmt);
        }
    }
    public void EndDialogue()
    {
        Time.timeScale = 1f;
        if(!disengaging)
        {
            disengaging = true;
        }
        else
        {
            disengaging = false;
            LoadInTime = 0;
            InDialogue = false;
            gameObject.SetActive(false);
        }
    }
    public void StartDialogue()
    {
        LoadInTime = 0;
        index = 0;
        InDialogue = true;
        StartCoroutine(TypeLine());
    }
    public IEnumerator TypeLine()
    {
        foreach(char c in DialogueParts[index].Text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
            while (Main.Paused)
                yield return new WaitForSecondsRealtime(textSpeed);
        }
    }
    public void NextLine()
    {
        if (index < DialogueParts.Length - 1)
        {
            index++;
            nameText.text = DialogueParts[index].Name;
            dialogueText.text = string.Empty;
            UpdateImages(index);
            StartCoroutine(TypeLine());
        }
        else if (!disengaging)
        {
            EndDialogue();
        }
    }
    public void UpdateImage(Image i)
    {
        i.SetNativeSize();
        float ratioX = boxDimensions.rectTransform.rect.width / i.rectTransform.rect.width;
        float ratioY = boxDimensions.rectTransform.rect.height / i.rectTransform.rect.height;
        i.transform.localScale = Vector3.one * Mathf.Min(ratioX, ratioY);
    }
    public void UpdateImages(int index)
    {
        Sprite nextSpeakerSprite = DialogueParts[index].SpeakerIcon;
        leftIsTalking = DialogueParts[index].LeftSideSpeaker;
        if (nextSpeakerSprite != null)
        {
            if (leftIsTalking)
                leftPortrait.sprite = nextSpeakerSprite;
            else
                rightPortrait.sprite = nextSpeakerSprite;
        }
        UpdateImage(leftPortrait);
        UpdateImage(rightPortrait);
    }
}
