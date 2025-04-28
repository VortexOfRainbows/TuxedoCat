using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject Gate;
    public bool Open = false;
    public DoorButton[] Button;
    public SpriteRenderer[] Light;
    private void Start()
    {

    }
    private void FixedUpdate()
    {
        Open = CheckOpen();
        if(Open)
        {
            Gate.transform.localPosition = Gate.transform.localPosition.Lerp(new Vector3(0, 1.5f, 0), 0.025f);
            if(Gate.transform.localPosition.y < 0.5f)
                Gate.transform.localPosition += new Vector3(0, 0.01f);
        }
        else {
            Gate.transform.localPosition = Gate.transform.localPosition.Lerp(new Vector3(0, -1.5f, 0), 0.025f);
            if(Gate.transform.localPosition.y > -1.5f)
                Gate.transform.localPosition -= new Vector3(0, 0.01f);
        }
        for (int i = 0; i < Button.Length; ++i)
            if (Button[i].Toggled)
                TurnOnLight(i);
            else
                TurnOffLight(i);
    }
    private void TurnOnLight(int i)
    {
        Light[i].color = Color.Lerp(Light[i].color, Color.white * 1.5f, 0.1f);
    }
    private void TurnOffLight(int i)
    {
        Light[i].color = Color.Lerp(Light[i].color, Color.black, 0.1f);
    }
    private bool CheckOpen()
    {
        foreach (DoorButton db in Button)
            if (!db.Toggled)
                return false;
        return true;
    }
}
