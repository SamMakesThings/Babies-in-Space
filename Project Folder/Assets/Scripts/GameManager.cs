using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject player0Prefab;
    public GameObject player1Prefab;
    public GameObject p0;
    public GameObject p1;
    public MouseOrbitImproved cam0;
    public MouseOrbitImproved cam1;
    public Vector3 spawnPos0;
    public Vector3 spawnPos1;
    public Vector2 score = Vector2.zero;
    public int goal = 5;
    public Text p0Score;
    public Text p1Score;
    public Text title;
    public bool winScreen;
    public Terrain terrain;
    public AudioSource audioroll;
    public aplay_mb apmb;
    public string fire0;
    public string fire1;
    public float roundEndTime;
    public float roundEndDelay = 2f;
    public bool roundEnded;

	// Use this for initialization
	void Start () {
        score = Vector2.zero;
        UpdateScore();
        ResetGame();
    }
	
	// Update is called once per frame
	void Update () {

        if (winScreen)
        {
            if (Input.GetButtonDown(fire0) || Input.GetButtonDown(fire1))
            {
                winScreen = false;
                title.text = "BALLS ALL THE WAY DOWN";
                score = Vector2.zero;
                UpdateScore();
                ResetGame();
            }
        }
        else if (roundEnded && roundEndTime + roundEndDelay < Time.time)
        {
            ResetGame();
            roundEnded = false;
        }
        {

        }
	}

    public void PlayerDeath(bool mip1)
    {
        if (mip1)
        {
            score.y += 1;
            UpdateScore();
            if (score.y >= goal)
            {
                title.text = "RED WINS!";
                winScreen = true;
            }
            else
            {
                roundEndTime = Time.time;
                roundEnded = true;
            }
        }
        else
        {
            score.x += 1;
            UpdateScore();
            if (score.x >= goal)
            {
                title.text = "GREEN WINS!";
                winScreen = true;
            }
            else
            {
                roundEndTime = Time.time;
                roundEnded = true;
            }
        }
        
    }
    
    void ResetGame()
    {
        if (p0)
        {
            Destroy(p0);
            p0 = null;
        }
        if (p1)
        {
            Destroy(p1);
            p1 = null;
        }

        p0 = Instantiate(player0Prefab,spawnPos0,Quaternion.identity);
        cam0.target = p0.transform;
        var bc0 = p0.GetComponent<BoatController>();
        bc0.gm = this;
        bc0.terrain = terrain;
        bc0.cam = cam0.gameObject;
        bc0.apmb = apmb;
        fire0 = bc0.inputFire;

        p1 = Instantiate(player1Prefab, spawnPos1, Quaternion.identity);
        cam1.target = p1.transform;
        var bc1 = p1.GetComponent<BoatController>();
        bc1.gm = this;
        bc1.terrain = terrain;
        bc1.cam = cam1.gameObject;
        bc1.mip1 = true;
        bc1.apmb = apmb;
        fire1 = bc1.inputFire;
    }

    void UpdateScore()
    {
        p0Score.text = "Score: " + score.x;
        p1Score.text = "Score: " + score.y;
    }
}
