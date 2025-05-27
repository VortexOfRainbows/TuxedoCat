using UnityEngine;

public class DialogueEvent : MonoBehaviour
{
    public bool Triggered;
    public DialoguePart[] DialogueParts;
    public bool EndGameAtTheEnd = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Player.Instance.gameObject && !Triggered)
        {
            Dialogue.Reset(DialogueParts, transform.position);
            Dialogue.Instance.endGameAtEnd = EndGameAtTheEnd;
            Triggered = true;
        }
    }
}
