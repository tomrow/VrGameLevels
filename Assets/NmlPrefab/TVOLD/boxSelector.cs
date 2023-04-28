using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class boxSelector : MonoBehaviour
{
    public float Counter;
    public float Countermax;
    bool starttimer;
    GameObject Collided;
    // Start is called before the first frame update.
    void Start()
    {
        starttimer = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (starttimer)
        {
            Counter += Time.deltaTime;
            Debug.Log(Counter);
        }
        if ((Counter >= Countermax) && starttimer)
        {
            
            SceneManager.LoadScene("Level1");
        }
            
    }
    private void OnTriggerEnter(Collider PLAYTRIGGER)
    {
        if (PLAYTRIGGER.gameObject.name == "TV_OLD")
        {
            starttimer = true;
            Debug.Log("Hit");
        }
    }
}
