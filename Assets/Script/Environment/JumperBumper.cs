using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperBumper : MonoBehaviour
{
    public float power;
    bool animate;
    float animCounter;
    public float counterSpeed;
    float animatorSize = 82;
    Transform animator;

    // Start is called before the first frame update
    void Start()
    {
        if (counterSpeed == 0f) { counterSpeed = 8f; }
        animator = transform.Find("hose");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (animate)
        {
            animCounter+= counterSpeed;
            animator.localScale = new Vector3(animatorSize,animatorSize,animatorSize*(1+Mathf.Sin(Mathf.Deg2Rad * animCounter)));
            if (animCounter > 180)
            {
                animate = false;
                animCounter= 0f;
                animator.localScale = new Vector3(animatorSize, animatorSize, animatorSize);
            }

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerGroundProbe")
        {
            other.gameObject.transform.parent.parent.GetComponent<PlayerMovement>().vspeed = power;
            other.gameObject.transform.parent.parent.GetComponent<PlayerMovement>().speed2 *= 0.5f;

            other.gameObject.transform.parent.parent.transform.Translate(Vector3.up*transform.lossyScale.y);
            animate = true; animCounter = 0f;
        }
    }
}
