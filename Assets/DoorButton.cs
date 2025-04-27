using UnityEngine;

public class DoorButton : MonoBehaviour
{
    public Door Parent;
    public bool ResetsWhenNotActive = false;
    public bool Toggled = false;
    public bool DoorOpened => Parent.Open;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Spider"))
            Toggled = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Spider"))
        {
            if (ResetsWhenNotActive && !DoorOpened)
            {
                Toggled = false;
            }
        }
    }
}
