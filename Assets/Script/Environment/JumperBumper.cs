using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperBumper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PlayerGroundProbe")
        {
            other.gameObject.transform.parent.parent.GetComponent<PlayerMovement>().vspeed = 19;
            other.gameObject.transform.parent.parent.GetComponent<PlayerMovement>().speed2 *= 0.5f;

            other.gameObject.transform.parent.parent.transform.Translate(Vector3.up*transform.lossyScale.y);
        }
    }
}
