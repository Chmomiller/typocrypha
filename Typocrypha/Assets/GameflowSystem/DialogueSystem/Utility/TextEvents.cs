﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
// edits by James Iwamasa

// represents an event to be played during text dialogue scrolling
public delegate IEnumerator TextEventDel(string[] opt);

// struct to contain event text
public struct TextEvent {
	public string evt;
	public string[] opt;
	public TextEvent(string evt, string[] opt) {
		this.evt = evt;
		this.opt = opt;
	}
}

// event class for events during text dialogue
public class TextEvents : MonoBehaviour {
	public static TextEvents main = null;
	public Dictionary<string, TextEventDel> text_event_map;
	[HideInInspector] public bool is_prompt; // are we prompting input?
	[HideInInspector] public string prompt_input; // input from prompt

	public SpriteRenderer dimmer; // sprite used to cover screen for fade/dim effects
	public GameObject center_text; // for showing floating text in the center
	public Camera main_camera; // main camera object
	public GameObject dialogue_box; // dialogue box object
	public GameObject glitch_effect; // sprite used for glitch effect
	public GameObject screen_frame; // screenframe object

	void Awake() {
		main = this;
		text_event_map = new Dictionary<string, TextEventDel> {
			{"screen-shake", screenShake},
			{"block", block},
			{"pause", pause},
			{"fade", fade},
			{"next", next},
			{"next-delay", nextDelay},
			{"center-text-scroll", centerTextScroll},
			{"center-text-fade", centerTextFade},
			{"play-sfx", playSFX},
			{"play-music", playMusic},
			{"play-bgm", playMusic},
			{"stop-music", stopMusic},
			{"stop-bgm", stopMusic},
			{"set-scroll-delay", setScrollDelay},
			{"set-bg", setBG},
			{"hide-text-box", hideTextBox},
			{"set-talk-sfx", setTalkSFX},
			{"highlight-character", highlightCharacter},
			{"remove-character", removeCharacter},
			{"remove-all-character", removeAllCharacter},
            {"evil-eye", evilEye},
            {"prompt", prompt},
			{"glitch", glitch},
			{"set-name", setName},
			{"frame", frame},
			{"heal-player", healPlayer}
		};
		is_prompt = false;
	}

	// plays the event 'evt', returning the coroutine created (null if event doesnt exist)
	public Coroutine playEvent(string evt, string[] opt) {
		TextEventDel text_event;
		if (!text_event_map.TryGetValue (evt, out text_event))
			Debug.LogException (new System.Exception("Bad text event parameters:" + evt));
		return StartCoroutine(text_event (opt));
	}

	// resets all the parameters that might have been changed
	public void reset() {
		main_camera.transform.position = new Vector3 (0,0,-10);
		DialogueManager.main.pause_scroll = false;
		DialogueManager.main.block_input = false;
	}

	// finishes up persistent events that might have been skipped (like removing a character)
	public void finishUp(List<TextEvent>[] text_events) {
		foreach (List<TextEvent> text_event_pos in text_events) {
			if (text_event_pos == null) continue;
			foreach (TextEvent text_event in text_event_pos) {
				switch (text_event.evt) {
				case "next":
				case "play-music":
				case "play-bgm":
				case "set-scroll-delay":
				case "set-bg":
				case "remove-character":
				case "remove-all-character":
				case "hide-text-box":
					playEvent (text_event.evt, text_event.opt);
					break;
				default:
					break;
				}
			}
		}
	}

/**************************** TEXT EVENTS *****************************/

	// shakes the screen
	// input: [0]: float, length of shake
	//        [1]: float, intensity of shake
	IEnumerator screenShake(string[] opt) {
		Transform cam_tr = main_camera.transform;
		Vector3 old_cam_pos = cam_tr.position;
		float[] opts = opt.Select((string str) => float.Parse(str)).ToArray();
		float curr_time = 0;
		float wait_time = opts [0] / 4;
		while (curr_time < wait_time) {
			Vector3 new_pos = Random.insideUnitCircle * opts [1];
			new_pos.z = old_cam_pos.z;
			cam_tr.position = new_pos;
			yield return new WaitForSeconds (0.06f);
			curr_time += 0.06f;
		}
		cam_tr.position = new Vector3(0,0,-10);
	}
		
	// blocks or unblocks input
	// input: [t|f], 't' blocks input until 'f' unblocks
	IEnumerator block(string[] opt) {
		if (opt [0].CompareTo ("t") == 0)
			 DialogueManager.main.block_input = true;
		else DialogueManager.main.block_input = false;
		yield return true;
	}

	// causes dialogue and cutscene to pause: it is recommended to also block
	// input: [0]: float, length of pause
	IEnumerator pause(string[] opt) {
		DialogueManager.main.pause_scroll = true; // pause text scroll
		yield return new WaitForSeconds (float.Parse (opt [0]));
		DialogueManager.main.pause_scroll = false;
	}

	// fades the screen in or out
	// input: [0]: [in|out], 'in' re-reveals screen, 'out' hides it
	//        [1]: float, length of fade in seconds
	//        [2]: float, red color component
	//        [3]: float, green color component
	//        [4]: float, blue color component
	IEnumerator fade(string[] opt) {
		float fade_time = float.Parse (opt [1]);
		float r = float.Parse (opt [2]);
		float g = float.Parse (opt [3]);
		float b = float.Parse (opt [4]);
		float alpha;
		float a_step = 1f * Time.deltaTime / fade_time; // amount of change each frame
		if (a_step > 1) a_step = 1;
		alpha = dimmer.color.a;
		if (opt [0].CompareTo ("out") == 0) { // hide screen
			while (alpha < 1f) {
				yield return null;
				alpha += a_step;
				dimmer.color = new Color (r, g, b, alpha);
			}
		} else { // show screen
			while (alpha > 0f) {
				yield return null;;
				alpha -= a_step;
				dimmer.color = new Color (r, g, b, alpha);
			}
		}
	}

	// forces next line of dialogue (SHOULD BE PLACED AT END OF LINE)
	// input: none
	IEnumerator next(string[] opt) {
		DialogueManager.main.forceNextLine ();
		yield return true;
	}

	// forces next line of dialogue after a delay
	// input: [0]: float, delay time in seconds
	IEnumerator nextDelay(string[] opt) {
		Debug.Log ("nextDelay:" + opt[0]);
		yield return new WaitForSeconds (float.Parse (opt [0]));
		DialogueManager.main.forceNextLine ();
	}

	// NON_OPERATIONAL
	// scrolls floating text center aligned in the center of the screen (can also be used to immediately show text)
	// input: [0]: float, delay time in seconds
	//        [1]: float, red color
	//        [2]: float, green color
	//        [3]: float, blue color
	//        [4]: string, text to show
	IEnumerator centerTextScroll(string[] opt) {
		Text txt = center_text.GetComponent<Text> ();
		float delay = float.Parse (opt [0]);
		float r = float.Parse (opt [1]);
		float g = float.Parse (opt [2]);
		float b = float.Parse (opt [3]);
		float a = txt.color.a;
		string txt_str = opt [4];
		txt.color = new Color (r, g, b, a);
		if (delay == 0) { // show text immediately
			txt.text = txt_str;
		} else { // scroll text character by character
			txt.text = "";
			int txt_pos = 0;
			while (txt_pos < txt_str.Length) {
				txt.text += txt_str [txt_pos++];
				yield return new WaitForSeconds (delay);
			}
		}
	}

	// NON_OPERATIONAL
	// fades center text in or out
	// input: [0]: [in|out], 'in' re-reveals screen, 'out' hides it
	//        [1]: float, length of fade in seconds
	IEnumerator centerTextFade(string[] opt) {
		Text txt = center_text.GetComponent<Text> ();
		float fade_time = float.Parse (opt [1]);
		float r = txt.color.r;
		float g = txt.color.g;
		float b = txt.color.b;
		float alpha = txt.color.a;
		float a_step = 1f * 0.017f / fade_time; // amount of change each frame
		if (a_step > 1) a_step = 1;
		if (opt [0].CompareTo ("out") == 0) { // hide text
			while (alpha > 0f) {
				yield return new WaitForSeconds(0.017f);
				alpha -= a_step;
				txt.color = new Color (r, g, b, alpha);
			}
		} else { // show screen
			while (alpha < 1f) {
				yield return new WaitForSeconds(0.017f);
				alpha += a_step;
				txt.color = new Color (r, g, b, alpha);
			}
		}
	}

	// plays the specified sfx
	// input: [0]: string, sfx filename
	IEnumerator playSFX(string[] opt) {
		AudioPlayer.main.playSFX (opt[0]);
		yield return true;
	}

	// plays specified music
	// input: [0]: string, music filename
	IEnumerator playMusic(string[] opt) {
		AudioPlayer.main.playMusic (opt [0]);
		yield return true;
	}

	// stops music
	IEnumerator stopMusic(string[] opt){
		AudioPlayer.main.stopMusic ();
		yield return true;
	}

	// sets scroll delay of main dialogue text scroll
	// input: [0]: float, new delay amount in seconds
	IEnumerator setScrollDelay(string[] opt) {
		DialogueManager.main.setScrollDelay(float.Parse (opt [0]));
		yield return true;
	}
		
	// sets background image from sprite name
	// input: [0]: string, name of image file
	IEnumerator setBG(string[] opt) {
		BackgroundEffects.main.setSpriteBG (opt [0]);
		yield return true;
	}
		
	// hides/shows dialogue box (NOTE: text is STILL GOING when hidden)
	// typically, should block when hiding to avoid skipping reshow event
	// input: [0]: [t|n], hides text box if 't', shows if 'f'
	IEnumerator hideTextBox(string[] opt) {
		dialogue_box.SetActive (opt[0].CompareTo("f") == 0);
		yield return true;
	}
		
	// sets the talking sfx
	// input: [0]: string, name of audio file
	IEnumerator setTalkSFX(string[] opt) {
		AudioPlayer.main.setSFX (3, opt[0]); // put sfx in channel 3
		yield return true;
	}

	// DEPRECATED
	// prompts player to enter something into TYPORCYPHA; resumes on enter
	// input: [0]: string, type of prompt
	IEnumerator prompt(string[] opt) {
		/*
		CutsceneManager.main.enabled = false;
		dialogue_box.SetActive (false);
		track_typing.enabled = true;
		is_prompt = true;
		GameObject display = null; // visual display for prompt (instructions, usually)
		string type = opt [0].Trim ().ToLower ();

		// initialize prompt
		switch (type) {
		case "sprite":
			display = Instantiate (Resources.Load<GameObject> ("prefabs/PlayerSpriteChoice"));
			break;
		case "pronoun":
			display = Instantiate (Resources.Load<GameObject> ("prefabs/PlayerPronounChoice"));
			break;
		}

		while (is_prompt) {
			// wait for uder to press ENTER
			yield return new WaitWhile (() => is_prompt);
			// check result
			string choice = prompt_input.Trim ().ToLower ();
			switch (type) {
			case "name": // set name
				// CHECK NAME PARAMETERS
				PlayerDialogueInfo.main.player_name = prompt_input;
				break;
			case "sprite": // set sprite (choice between 'a' and 'b')
				if (choice.CompareTo ("a") == 0) { PlayerDialogueInfo.main.setSprite (0); } 
				else if (choice.CompareTo ("b") == 0) { PlayerDialogueInfo.main.setSprite (1); } 
				else { 
					// INDICATE BAD INPUT
					is_prompt = true; // keep asking for input
				}
				break;
			case "pronoun": // set pronoun (options go from 1-4)
				if (choice.CompareTo("a") == 0) { PlayerDialogueInfo.main.setPronoun (0); }
				else if (choice.CompareTo("b") == 0) { PlayerDialogueInfo.main.setPronoun (1); }
				else if (choice.CompareTo("c") == 0) { PlayerDialogueInfo.main.setPronoun (2); }
				else if (choice.CompareTo("d") == 0) { PlayerDialogueInfo.main.setPronoun (3); }
				else {
					// INDICATE BAD INPUT
					is_prompt = true; // keep asking for input
				}
				break;
			}
		}

		if (display != null) Destroy (display);
		prompt_input = "";
		track_typing.enabled = false;
		dialogue_box.SetActive (true);
		CutsceneManager.main.enabled = true;
		CutsceneManager.main.forceNextLine ();
		*/
		yield return true;
	}

	// Toggles highlighting a character
	// input: [0]: string, name of sprite to highlight
	//        [1]: float, amount to highlight (multiplier to tint)
	IEnumerator highlightCharacter(string[] opt) {
		DialogueManager.main.highlightCharacter(opt[0], float.Parse(opt[1]));
		yield return true;
	}

	// Removes a specific character from the scene
	// input: [0]: string, name of sprite to remove
	IEnumerator removeCharacter(string[] opt) {
		DialogueManager.main.removeCharacter (opt [0]);
		yield return true;
	}

	// Removes all characters from a scene
	// input: NONE
	IEnumerator removeAllCharacter(string[] opt) {
		DialogueManager.main.removeAllCharacter ();
		yield return true;
	}

	// NON_OPERATIONAL
    IEnumerator evilEye(string[] opt) {
        //AnimationPlayer.main.playAnimation("Evil_Eye", new Vector3(-5, 0, 0), 2f);
        yield return true;
    }

	// NON_OPERATIONAL
	IEnumerator glitch(string[] opt)
	{
		for(int i = 0; i < 5; i++)
		{
			glitch_effect.SetActive(true);
			yield return new WaitForSeconds (0.05f);
			glitch_effect.SetActive(false);
			yield return new WaitForSeconds (0.05f);
		}
		glitch_effect.SetActive(true);
		yield return new WaitForSeconds (0.15f);
		glitch_effect.SetActive(false);
		
		yield return true;
	}

	// Sets name from last inputed text
	// Input: NONE
	IEnumerator setName(string[] opt) {
		PlayerDialogueInfo.main.player_name = DialogueManager.main.answer;
		yield return true;
	}

	// makes the screenframe come in/out
	// input: [0]: [in|out], 'in' re-reveals screenframe, 'out' hides it
	IEnumerator frame(string[] opt) {
		if (opt [0].CompareTo ("out") == 0) { // hide screenframe
			screen_frame.SetActive(false);
		} else { // show screenframe
			screen_frame.SetActive(true);
		}
		yield return true;
	}

	// restore player to full HP
	// input: N/A
	IEnumerator healPlayer(string[] opt) {
		Debug.Log ("[JohnTypocrypha Voice]: i need healing");
		Player.main.restoreToFull ();
		yield return true;
	}
}
