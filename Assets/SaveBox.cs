using UnityEngine;

public class SaveBox : MonoBehaviour
{
    public static float ActiveRadius => 3.5f;
    public GameObject Button;
    public GameObject Left;
    public GameObject Right;
    public GameObject Back;
    public bool PlayerNear;
    public bool Active;
    public int Anim = 0;
    public void FixedUpdate()
    {
        if ((Player.Instance.gameObject.transform.position - transform.position).magnitude < ActiveRadius)
            PlayerNear = true;
        else
            PlayerNear = false;
        UpdateActive();
        if(PlayerNear && Anim != 1)
        {
            Left.transform.localEulerAngles = Mathf.LerpAngle(Left.transform.localEulerAngles.z, 0, 0.1f) * Vector3.forward;
            Right.transform.localEulerAngles = Mathf.LerpAngle(Right.transform.localEulerAngles.z, 0, 0.1f) * Vector3.forward;
            Back.transform.localPosition = Back.transform.localPosition.Lerp(new Vector3(0, -0.0625f, Back.transform.localPosition.z), 0.1f);
            Button.SetActive(Anim == 0);
        }
        else
        {
            Left.transform.localEulerAngles = Mathf.LerpAngle(Left.transform.localEulerAngles.z, -90, 0.1f) * Vector3.forward;
            Right.transform.localEulerAngles = Mathf.LerpAngle(Right.transform.localEulerAngles.z, 90, 0.1f) * Vector3.forward;
            Back.transform.localPosition = Back.transform.localPosition.Lerp(new Vector3(0, -0.3125f, Back.transform.localPosition.z), 0.1f);
            Button.SetActive(false);
        }
        Active = Anim < 0 || (Player.InSaveAnimation && Anim < 50 && Anim > 0);
        if(Player.InSaveAnimation)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one , 0.25f);
        }
        int order = Active ? 10 : -20;
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.gameObject != Button && sr.gameObject != Back)
                sr.sortingOrder = order;
        }
    }
    public void EnterSaveBox()
    {
        Anim = 120;
        Player.Instance.RB.velocity += new Vector2(0, 5f);
        Player.InSaveAnimation = true;
        Player.Instance.LastUsedSaveBox = this;
    }
    public static Color PastelGradient(float radians)
    {
        float newAi = radians;
        double center = 210;
        Vector2 circlePalette = new Vector2(1, 0).RotatedBy(newAi);
        double width = 45 * circlePalette.y;
        int red = (int)(center + width);
        circlePalette = new Vector2(1, 0).RotatedBy(newAi + 120 * Mathf.Deg2Rad);
        width = 45 * circlePalette.y;
        int grn = (int)(center + width);
        circlePalette = new Vector2(1, 0).RotatedBy(newAi + 240 * Mathf.Deg2Rad);
        width = 45 * circlePalette.y;
        int blu = (int)(center + width);
        return new Color(red / 255f, grn / 255f, blu / 255f);
    }
    public void SaveAnimation()
    {
        float rand = Utils.RandFloat(Mathf.PI * 2);
        for(int j = 0; j < 2; j++)
        {
            int c = 12 + j * 12;
            for (int i = 0; i < c; ++i)
            {
                float r = Mathf.PI * i / 6f;
                ParticleManager.NewParticle(transform.position, 1 - j * 0.5f, new Vector2(9 + j * 4 * Utils.RandFloat(-1, 1), 0).RotatedBy(rand + r), 0.45f + j, Utils.RandFloat(0.9f, 1.2f), 3, PastelGradient(r));
            }
        }
        PopupText.NewPopupText(transform.position, new Vector3(0, 2, 0), new Color(1, 0.6f, 0.3f), "Saved!");
    }
    public void UpdateActive()
    {
        if (!PlayerNear && Anim <= 0)
            return;
        if (Anim < 0)
        {
            Anim++;
            Player.InSaveAnimation = false;
        }
        else if ((Player.Control.E && !Player.PrevControl.E && Anim == 0) || Anim > 0)
        {
            if (!Player.InSaveAnimation)
            {
                EnterSaveBox();
            }
            Player.Instance.RB.velocity *= 0.9825f;
            Player.Instance.RB.gravityScale = 0f;
            float percent = Anim / 120f;
            float endPercent = Mathf.Min(1, Anim / 30f);
            Vector2 targetPosition = new Vector2(Mathf.Lerp(Player.Instance.transform.position.x, transform.position.x, (1 - percent) * 0.2f + percent * 0.2f), transform.position.y + (Mathf.Sin(percent * Mathf.PI) * 2.4f + percent) * endPercent - 0.5f);
            Player.Instance.transform.position = Player.Instance.transform.position.Lerp(targetPosition, 0.1f + 0.25f * (1 - percent));
            if (Anim > 1)
            {
                if(Anim == 2 && Player.Instance.DeathAnimation <= 0)
                {
                    SaveAnimation();
                }
                Anim--;
            }
            else if (Player.Control.Up || Player.Control.E)
            {
                Player.Instance.RB.velocity += new Vector2(0, 11f);
                Player.Instance.RB.gravityScale = 1f;
                Left.transform.localEulerAngles = Mathf.LerpAngle(Left.transform.localEulerAngles.z, -90, 0.9f) * Vector3.forward;
                Right.transform.localEulerAngles = Mathf.LerpAngle(Right.transform.localEulerAngles.z, 90, 0.9f) * Vector3.forward;
                Anim = -50;
            }
            if(Anim < 36 && Anim >= 0)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (1 + 0.3f * Mathf.Sin(Anim / 36f * Mathf.PI)), 0.25f);
            }
            if (Anim < 29 && Anim >= 0)
            {
                Player.Instance.transform.localScale = Vector3.one * (0.3f + Mathf.Sqrt((Anim / 29f)) * 0.7f);
            }
        }
    }
}
