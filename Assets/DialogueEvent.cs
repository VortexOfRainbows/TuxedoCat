using UnityEngine;

public class DialogueEvent : MonoBehaviour
{
    public bool Triggered;
    public string[] names;
    public string[] dialogue;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Player.Instance.gameObject && !Triggered)
        {
            Dialogue.Reset(names, dialogue, transform.position);
            Triggered = true;
        }
    }
}
