﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// manages title screen input and animations
public class TitleScreen : MonoBehaviour {
    public GameObject title_menu; // contains menu sprites
	public GameObject title_menu_list;
    public SpriteRenderer highlight_sprite;
    public int num_elements;
	public TextScroll text_scroll; // scrolls text
	public SpriteRenderer title_sprite; // title screen sprite
	public SpriteRenderer dimmer;
	public Color text_highlight_color;
	public GameObject load_text;

    private bool isActive = false;
    private int index = 0;
    private const float y_offset = 0.62f;

    private void Update()
    {
		if (Pause.main.isPaused ()) return;
		if (!isActive) {
			title_menu.SetActive (false);
			return;
		}  
        // go to next item if down is pressed
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(index + 1 < num_elements)
            {
                ++index;
                Vector3 move = new Vector3(0, -y_offset, 0);
                highlight_sprite.transform.position += move;
                AudioPlayer.main.playSFX("sfx_enemy_select");
            }
        }
        // go to last item if up is pressed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (index - 1 >= 0)
            {
                --index;
                Vector3 move = new Vector3(0, y_offset, 0);
                highlight_sprite.transform.position += move;
                AudioPlayer.main.playSFX("sfx_enemy_select");
            }
        }
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            switch (index)
            {
				case 0: // Start Demo
					Debug.Log ("starting new game");
					transitionToStart ();
					isActive = false;
					Pause.main.title = false;
					break;
                case 1: // Quit game
					Application.Quit();
					break;
				case 2: // Settings
					Debug.Log("settings");
					Pause.main.titleMenu ();
					break;
				/*
                case 3: //new game button
                    Debug.Log("starting new game");
                    transitionToStart();
					isActive = false;
                    break;
                case 4:
                    Application.Quit();
					isActive = false;
                    break;
                */
                default:
                    throw new System.NotImplementedException("no such menu index: " + index);
            }
            AudioPlayer.main.playSFX("sfx_enter");
        }
		setTitleText ();
    }

	// Sets title menu text
	private void setTitleText() {
		Text[] title_options = title_menu_list.GetComponentsInChildren<Text> ();
		string selected_option_text = "";
		int i = 0;
		foreach (Text option in title_options) {
			switch (i) {
			case 0:
				selected_option_text = "BEGIN DEMO";
				break;
			case 1:
				selected_option_text = "QUIT";
				break;
			case 2:
				selected_option_text = "SETTINGS";
				break;
				/*
				case 3:
					selected_option_text = "NEW GAME";
					break;
				case 4:
					selected_option_text = "QUIT";
					break;
				*/
			default:
				selected_option_text = "this text should not be seen";
				break;
			}
			if (option == title_options[index]) {
				option.text = "> " + selected_option_text + " <";
				option.color = text_highlight_color;
			} else {
				option.text = selected_option_text;
				option.color = Color.white;
			}
			++i;
		}
	}

    // starts title screen music/ui/animations/etc
    public void startTitle() {
		title_menu.SetActive (true);
        isActive = true;
		AudioPlayer.main.playMusic ("bgm_title_loop");
	}

	// called to transition to MainScene (new game) when start is pressed
	void transitionToStart() {
		
		load_text.SetActive (true);

		AsyncOperation load_op = SceneManager.LoadSceneAsync ("MainScene", LoadSceneMode.Additive);
		StartCoroutine (loadMainScene (load_op));
	}

	// spawns loading screen, waits for main scene to load, and then transitions
	IEnumerator loadMainScene(AsyncOperation load_op) {
		Debug.Log ("loading main scene...");
		//new_file_button.interactable = false;
		//new_file_button.gameObject.GetComponentInChildren<Text>().text = "Loading...";
		//StartCoroutine (fadeAndEnd());
		yield return new WaitUntil (() => load_op.isDone);
		GameObject.Find ("Main Camera").GetComponent<AudioListener>().enabled = false; // to avoid multiple listeners
		StartCoroutine (fadeAndStart ());
	}

	// fades title screen to transparent and start main scene
	IEnumerator fadeAndStart() {
		Debug.Log ("fadeAndStart");
		float alpha = 0;
		while (alpha <= 1) {
			dimmer.color = new Color (dimmer.color.r, dimmer.color.g, dimmer.color.b, alpha);
			alpha += 0.01f;
			yield return new WaitForEndOfFrame ();
		}
		yield return new WaitForSeconds (2f);
		SceneManager.UnloadSceneAsync ("TitleScene");
	}

	IEnumerator fadeAndEnd(){
		float alpha = 1.0f;
		while (alpha > 0) {
			dimmer.color = new Color (dimmer.color.r, dimmer.color.g, dimmer.color.b, alpha);
			alpha -= 0.01f;
			yield return new WaitForEndOfFrame ();
		}
	}
}
