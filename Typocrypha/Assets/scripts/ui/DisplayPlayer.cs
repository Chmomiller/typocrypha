﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// displays player's stats in appropiate areas
public class DisplayPlayer : MonoBehaviour {
	public Text health_text; // display's player's health
	public Text shield_text; // display's player's shield
	public Text cast_text; // display's what player is casting
	public BarMeter health_bar; // health bar
	public BarMeter shield_bar; // shield bar

	void Update () {
		health_text.text = Player.main.Curr_hp.ToString ();
		shield_text.text = Player.main.Shield.ToString ();
		cast_text.text = Player.main.Last_cast.Replace(" ", "-").ToUpper();
		health_bar.setValue ((float)Player.main.Curr_hp/Player.main.Max_hp);
		shield_bar.setValue ((float)Player.main.Shield/Player.main.Max_shield);
	}
}
