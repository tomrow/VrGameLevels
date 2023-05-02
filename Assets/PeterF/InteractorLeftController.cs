using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

public class InteractorLeftController : MonoBehaviour
{
    GameObject leftController;
    // Start is called before the first frame update
    void Start()
    {
        leftController = GameObject.Find("LeftController");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Physics.Raycast(leftController.transform.position, leftController.transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity)) return;

        if (!hit.transform.TryGetComponent(out CreateObject objectSpawner)) return;

        Debug.Log("interacting");

        if (Input.GetAxis("XRI_Left_Trigger") > 0.8f)
        {
            objectSpawner.SpawnObject();
        }
    }
}
