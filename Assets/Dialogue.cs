using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal.Internal;

public class Dialogue : MonoBehaviour
{
    public static Dialogue Instance;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    [TextArea(3, 10)]
    public string[] SpeakerName;
    public string[] dialogue;
    public float textSpeed;
    private int index;
    public Vector2 cameraCenter;
    private void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    public static void Reset(string[] names, string[] dialogue, Vector2 camera)
    {
        Instance.gameObject.SetActive(true);
        Instance.index = 0;
        Instance.SpeakerName = names;
        Instance.dialogue = dialogue;
        Instance.nameText.text = names[0];
        Instance.dialogueText.text = string.Empty;
        Instance.cameraCenter = camera;
        Instance.StartDialogue();
    }
    private void Update()
    {
        Instance = this;
        if(gameObject.activeSelf)
        {
            Debug.Log(.05f * Time.unscaledDeltaTime * 100f);
            Camera.main.transform.position = Camera.main.transform.position.Lerp(new Vector3(cameraCenter.x, cameraCenter.y, Camera.main.transform.position.z), 0.05f * Time.unscaledDeltaTime * 100f);
            Time.timeScale = 0f;
            if (Input.GetMouseButtonDown(0))
            {
                if (dialogueText.text == dialogue[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    dialogueText.text = dialogue[index];
                }
            }
        }
    }
    private void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }
    private IEnumerator TypeLine()
    {
        foreach(char c in dialogue[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
        }
    }
    private void NextLine()
    {
        if(index < dialogue.Length -1)
        {
            index++;
            nameText.text = SpeakerName[index];
            dialogueText.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
