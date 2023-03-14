using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameStateVariables : MonoBehaviour
{
    public int health;
    public int maxHealth = 100;
    public int score;
    public GameObject scoreDisplay;
    public Text scoreDisplayText;
    int goalCheckCounter;

    // Start is called before the first frame update
    void Start()
    {
        goalCheckCounter = 0;
        health = maxHealth;
        scoreDisplay = GameObject.Find("ScoreDisplay");
        scoreDisplayText = scoreDisplay.GetComponent<Text>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        scoreDisplayText.text = Convert.ToString(score);
        goalCheckCounter += 1;
        goalCheckCounter = goalCheckCounter % 60;
        if (goalCheckCounter == 0)
        {
            if(GameObject.FindGameObjectsWithTag("Plus").Length < 1)
            {
                SceneManager.LoadScene("b");
            }
        }

    }
}
