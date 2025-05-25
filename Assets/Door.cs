using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject Gate;
    public bool Open = false;
    public DoorButton[] Button;
    public SpriteRenderer[] Light;
    public bool BossDoor = false;   
    private void Start()
    {

    }
    private void FixedUpdate()
    {
        Open = BossDoor ? !BiggieCheese.FightInitiated : CheckOpen();
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
        if(!BossDoor)
        {
            for (int i = 0; i < Button.Length; ++i)
                if (Button[i].Toggled)
                    TurnOnLight(i);
                else
                    TurnOffLight(i);
        }
        else
        {
            if(!Open)
                TurnOnLight(0);
            else
                TurnOffLight(0);
        }
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
