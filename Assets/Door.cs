using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject Gate;
    public bool Open = false;
    public DoorButton[] Button;
    private void Start()
    {

    }
    private void FixedUpdate()
    {
        if (CheckOpen())
        {
            Open = true;
            Gate.SetActive(false);
        }
    }
    private bool CheckOpen()
    {
        foreach (DoorButton db in Button)
            if (!db.Toggled)
                return false;
        return true;
    }
}
