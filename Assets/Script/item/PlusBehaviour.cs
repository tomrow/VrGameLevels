using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlusBehaviour : MonoBehaviour
{
    public bool dontMoveObjUp;
    public int collected = 0;
    // Start is called before the first frame update.
    void Start()
    {
        if (!dontMoveObjUp)
        {
            transform.Translate(Vector3.up * 5);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (collected > 0)
        {
            collected += 1;
        }
        if (collected > 10)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gameObject.GetComponent<Collider>().enabled = false;
            //increment health by 1
            collision.gameObject.GetComponent<GameStateVariables>().health += 1;
            collision.gameObject.GetComponent<GameStateVariables>().score += 1;
            //if health bigger than maxhealth, then set the health to the max value, cancelling out the increase
            if (collision.gameObject.GetComponent<GameStateVariables>().health > collision.gameObject.GetComponent<GameStateVariables>().maxHealth) { collision.gameObject.GetComponent<GameStateVariables>().health = collision.gameObject.GetComponent<GameStateVariables>().maxHealth; }

            transform.localScale = Vector3.zero;
            collected += 1;
            //play the sound effect
            collision.gameObject.transform.Find("Data").Find("Sound").Find("PlusCollect").gameObject.GetComponent<AudioSource>().Play();
            // Here should increase the score and health
            //Destroy(gameObject);
        }
    }
}
