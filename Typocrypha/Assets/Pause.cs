﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour {
    
    bool gamePause;
    Color dimCol;
    string pauseKey = "escape";
    GameObject[] children;

    void Start () {
        gamePause = false;
        children = new GameObject[2];
        children[0] = transform.GetChild(0).gameObject;
        children[1] = transform.GetChild(1).gameObject;

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(pauseKey))
        {
            gamePause = !gamePause;

            if (gamePause)
            {
                Time.timeScale = 0;
                BattleManager.main.pause = true;
                dimCol = BattleEffects.main.dimmer.color;
                BattleEffects.main.setDim(true);
                AudioPlayer.main.pauseSFX();
                foreach (GameObject g in children)
                    g.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                BattleManager.main.pause = false;
                BattleEffects.main.dimmer.color = dimCol;
                AudioPlayer.main.unpauseSFX();
                foreach (GameObject g in children)
                    g.SetActive(false);
            }
        }
	}
}