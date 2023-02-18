using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public RateGame rateGame;



	void Start () {
        Init();
    }
	

    public void Init()
    {
        // increase game open counter
        int gameOpenCounter = PlayerPrefs.GetInt("gameOpenCounter", 0) + 1;
        PlayerPrefs.SetInt("gameOpenCounter", gameOpenCounter);

        // Initialization game rating script
        rateGame.Init(gameOpenCounter);
    }

}
