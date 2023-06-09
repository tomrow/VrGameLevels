using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator10T : MonoBehaviour
    
{
    //creating variables that may be used.
    Rigidbody elevator;
    public Transform buttonTrigger;
    GameObject parentButton;
    //public GameObject ButtonControls;
    //public bool parentActive;
    Button_ScrCustomObject Button_Scr;
    //[SerializeField]
    //private Button_Scr Button_Scr;
    // Start is called before the first frame update..
    Vector3 initialPosition;
    void Start()
    {
        //Calling objects that may be used.
        gameObject.AddComponent<Rigidbody>();
        elevator = GetComponent<Rigidbody>();
        elevator.isKinematic = true;
        parentButton = buttonTrigger.gameObject;
        //Calling script. This needs correction, unless duplicate scripts are a nonissue.
        //Button_Scr = FindObjectOfType <Button_Scr>();
        Button_Scr = parentButton.GetComponentInParent<Button_ScrCustomObject>();
        initialPosition = transform.position;
        transform.Translate(0, -70, 0);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if (Button_Scr.buttonDown && transform.position.y < initialPosition.y)
        {
            elevator.position += (Vector3.up * 1);

        }
    }
}
