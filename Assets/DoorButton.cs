using UnityEngine;

public class DoorButton : MonoBehaviour
{
    public Door Parent;
    public GameObject PressurePlate;
    public bool ResetsWhenNotActive = false;
    public bool Toggled = false;
    public bool DoorOpened => Parent.Open;
    public void FixedUpdate()
    {
        if(Toggled)
        {
            PressurePlate.transform.localPosition = PressurePlate.transform.localPosition.Lerp(new Vector3(0, -0.125f, 0), 0.14f);
        }
        else
        {
            PressurePlate.transform.localPosition = PressurePlate.transform.localPosition.Lerp(Vector3.zero, 0.14f);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Spider"))
            Toggled = true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
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
