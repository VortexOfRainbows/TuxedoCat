using UnityEngine;

public class CheesusAnimator : MonoBehaviour
{
    public float Counter = 0;
    public void FixedUpdate()
    {
        Counter += 0.6f;
        float sin = Mathf.Sin(Counter * 0.1f);
        float cos = Mathf.Cos(Counter * 0.075f);
        transform.localScale = new Vector3(sin, cos, 1) * 0.5f;
        transform.localEulerAngles = new Vector3(Counter, Counter * 2, Counter * 3);
    }
}
