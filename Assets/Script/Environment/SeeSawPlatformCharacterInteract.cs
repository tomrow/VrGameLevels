using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeSawPlatformCharacterInteract : MonoBehaviour
{
    GameObject playerCharacter;
    float playerMass;
    float playerFallVel;
    // Start is called before the first frame update
    void Start()
    {
        playerMass = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.name == "PlayerGroundProbe")
        {
            playerCharacter = other.gameObject.transform.parent.parent.gameObject; //This should be the actual Player gameobject rather than the playergroundprobe
            //playerFallVel =  playerobject y vel during falling mode;
            //rigidbody add force;

        }
    }
}
