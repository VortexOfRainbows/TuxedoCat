
using UnityEngine;

public class Tutter : Goon
{
    public GameObject DialogueEventToEnableOnFree;
    public bool TiedUp = true;
    public void FreeMe()
    {
        if(TiedUp)
        {
            Color c = new Color(0.5411765f, 0.4666667f, 0.372549f);
            for (int i = 0; i < 50; ++i)
            {
                ParticleManager.NewParticle(transform.position, Utils.RandFloat(0.8f, 1.2f), Utils.RandCircle(7), 4, Utils.RandFloat(0.9f, 1.1f), 0, c);
            }
            TiedUp = false;
            anim.Head.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Tutter/TutterHead");
            anim.Body.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Tutter/TutterBody");
            anim.ArmLeft.GetComponent<SpriteRenderer>().sortingOrder = 6;
            if(DialogueEventToEnableOnFree != null)
            {
                DialogueEventToEnableOnFree.SetActive(true);
            }
        }
    }
    public new void FixedUpdate()
    {
        if (startVelo != Vector2.zero)
        {
            RB.velocity = startVelo * 0.5f;
            startVelo = Vector2.zero;
        }
        Vector3 targetPatrolPoint = Player.Position;
        Vector2 velo = RB.velocity;
        Vector2 targetVelocity = Vector2.zero;
        float topSpeed = TouchingGround ? 4f : 6f;
        float inertia = TouchingGround ? 0.06f : 0.03f;

        if (!TiedUp)
        {
            Vector3 toPatrol = targetPatrolPoint - transform.position;
            float distToTargetX = Mathf.Abs(toPatrol.x);
            if (distToTargetX > 2)
                targetVelocity.x += Mathf.Sign(toPatrol.x) * topSpeed;
            if (distToTargetX < 1)
                targetVelocity.x -= Mathf.Sign(toPatrol.x) * topSpeed;
            anim.Dir = Mathf.Sign(toPatrol.x);
        }
        else
            anim.Dir = Mathf.Sign(velo.x);

        if (TouchingGround)
            velo.x = Mathf.Lerp(velo.x, targetVelocity.x, inertia);
        else
        {
            velo += targetVelocity * inertia;
            velo.x *= 1 - inertia;
        }
        RB.velocity = velo;
        anim.eyeTargetPosition = Player.Position;
        anim.Animate();
        if(TiedUp)
            anim.ArmLeft.GetComponent<SpriteRenderer>().sortingOrder = -1;
    }
}
