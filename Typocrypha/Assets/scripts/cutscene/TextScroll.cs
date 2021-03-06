﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
// edits by James Iwamasa

// displays text character by character
public class TextScroll : MonoBehaviour {
	const float default_delay = 0.05f; // default delay

    public TextEvents event_handler;
    public float delay; // time between displaying characters
	public bool is_print; // is currently printing
	public bool pause_print; // is the text scroll paused?

	int text_pos; // position in text
	string in_text; // current text being printed out
	string out_buffer; // buffer for what will be shown
	Stack<Pair<string, string>> tag_stack; // stores currently used tags
	Text out_text; // where text gets outputted
	Coroutine curr; // current printing coroutine

	Regex tag_cutoff = new Regex ("<|>|=.*");
	Regex evt_cutout = new Regex (Regex.Escape("[") + ".*?" + Regex.Escape("]")); // matches events
	char[] opt_delim = new char[]{ ',' };

	void Start() {
		is_print = false;
		tag_stack = new Stack<Pair<string, string>> ();
	}

	// start printing string to Text display
	public void startPrint(string in_txt, Text out_txt) {
		in_text = in_txt.Trim().Replace("\"", "");
		out_text = out_txt;
		out_text.text = "";
		is_print = true;
		pause_print = false; // unpause text
		substituteMacros();
		if (curr != null) StopCoroutine (curr);
		event_handler.StopAllCoroutines (); // stop all previously running events
		curr = StartCoroutine (scrollText ());
	}

	// start deleting string in Text display
	public void startDelete(string in_txt, Text out_txt, string delete_sfx) {
		AudioPlayer.main.setSFX (3, delete_sfx); // put sfx in channel 3
		in_text = in_txt;
		out_text = out_txt;
		out_text.text = in_text;
		is_print = true;
		curr = StartCoroutine (deleteScrollText ());
	}

	// dump rest of string to output
	public void dump() {
		StopCoroutine (curr); // stop current printing
        event_handler.StopAllCoroutines (); // stop all previously running events
		is_print = false;
		out_text.text = evt_cutout.Replace(in_text, ""); // dump all text
	}

	// print characters to Text object one by one
	IEnumerator scrollText() {
		text_pos = 0;
		out_buffer = "";
		tag_stack.Clear ();
		while (text_pos < in_text.Length) {
			while (pause_print) yield return new WaitForEndOfFrame();
			if (text_pos >= in_text.Length) break;
			if (in_text [text_pos].CompareTo ('<') == 0) { // check if tag
				checkTags();
				continue;
			} else if (in_text [text_pos].CompareTo ('[') == 0) { // check if text event
				checkEvents(); 
				continue;
			}
			if (in_text[text_pos].CompareTo(' ') != 0)
				AudioPlayer.main.playSFX (3); // play speaking sfx if not a space
			out_buffer += in_text [text_pos++];
			out_text.text = out_buffer + tag_stack.Aggregate ("", (acc, next) => acc + next.second); // functional programming trick to build end tags
			if (delay > 0) yield return new WaitForSeconds (delay);
		}
		is_print = false;
	}

	// checks for tags (triangle brackets <>)
	void checkTags() {
		if (in_text [text_pos + 1].CompareTo ('/') == 0) { // check if end tag (pop)
			string end_tag = tag_stack.Pop().second; // remove tag from stack
			out_buffer += end_tag;                   // place end tag directly into buffer
			text_pos += end_tag.Length;              // move text_pos over after tag
		} else { // otherwise, is a start tag (push)
			Pair<string, string> tag = new Pair<string, string>();
			// get starting tag
			int fstart_pos = text_pos;
			int fend_pos = in_text.IndexOf ('>', fstart_pos);
			tag.first = in_text.Substring (fstart_pos, fend_pos - fstart_pos + 1);
			// generate ending tag
			tag.second = "</" + tag_cutoff.Replace(tag.first, "") + ">";
			tag_stack.Push (tag);    // add tag to tag stack
			text_pos = fend_pos + 1; // set new text_pos at end of start tag
			out_buffer += tag.first; // add start tag to out_buffer (end tag is added later)
		}
	}

	// checks for text events (square brackets []), and parse and play them
	void checkEvents() {
		int start_pos = text_pos;
		int end_pos = in_text.IndexOf (']', start_pos);
		int eq_pos = in_text.IndexOf ('=', start_pos);
		string evt;
		string[] opt;
		if (eq_pos == -1 || eq_pos > end_pos) { // if no '='
			evt = in_text.Substring (start_pos + 1, end_pos - start_pos - 1);
			opt = new string[0]; 
		} else {
			evt = in_text.Substring (start_pos + 1, eq_pos - start_pos - 1);
			opt = in_text.Substring (eq_pos + 1, end_pos - eq_pos - 1).Split (opt_delim);
		}
        event_handler.playEvent (evt, opt);
		Debug.Log ("text_event:" + evt + ":" + opt.Aggregate("", (acc, next) => acc + "," + next));
		if (evt.CompareTo ("next") == 0) return; // forceNextLine will handle rest
		text_pos = end_pos + 1;
	}

	// substiutes macros in 'in_text' (curly braces {})
	void substituteMacros() {
		StringBuilder true_str = new StringBuilder ();
		for (int i = 0; i < in_text.Length;) {
			if (in_text [i].CompareTo ('{') == 0) {
				int start_pos = i + 1;
				int end_pos = in_text.IndexOf ('}', start_pos);
				string[] macro = in_text.Substring (start_pos, end_pos - start_pos).Split(opt_delim);
				Debug.Log ("macro:" + macro.Aggregate("", (acc, next) => acc + "," + next));
				string[] opt = macro.Skip (1).Take (macro.Length - 1).ToArray ();
				string sub = TextMacros.main.macro_map [macro[0]] (opt);
				Debug.Log (sub);
				true_str.Append (sub);
				i = end_pos + 1;
			} else {
				true_str.Append (in_text [i++]);
			}
		}
		in_text = true_str.ToString ();
	}


	// deletes characters currently in buffer one by one (doesnt check for tags)
	IEnumerator deleteScrollText() {
		int text_pos = in_text.Length - 2;
		while (text_pos >= 0) {
			out_text.text = out_text.text.Substring(0, text_pos--);
			yield return new WaitForSeconds (delay);
		}
		is_print = false;
	}
}
