﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManagerS : MonoBehaviour {
    public static BattleManagerS main = null;
    public Player player;
    public TrackTyping trackTyping;
    public BattleKeyboard battleKeyboard;
    public BattleUI uiManager;
    public CastManager castManager;
    public EnemyDatabase enemyData;
    public AllyDatabase allyData;
    public GameObject enemy_prefab; // prefab for enemy object
    public GameObject ally_prefab; //prefab for ally object
    public BattleField field;
    [HideInInspector] public bool pause = true;

    private BattleWave Wave { get { return waves[curr_wave]; } }
    private int curr_wave = -1;
    private BattleWave[] waves;
    private GameObject curr_battle;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
            //Database code (possibly subject to becoming more object-y)
            EnemyDatabase.main.build();
            enemyData = EnemyDatabase.main;
            AllyDatabase.main.build();
            allyData = AllyDatabase.main;
        }
        else Destroy(this);
    }

    private void Start()
    {
        field = new BattleField(this);
        castManager.Field = field;
        field.player_arr[1] = player;
    }

    // check if player switches targets or attacks
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote)) // toggle pause
            setPause(!pause);
        if (pause) return;
        int old_ind = field.target_ind;

        //TARGET RETICULE CODE 

        // move target left or right
        if (Input.GetKeyDown(KeyCode.LeftArrow)) --field.target_ind;
        if (Input.GetKeyDown(KeyCode.RightArrow)) ++field.target_ind;

        //toggle enemy info on Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            uiManager.toggleScouter();
        }

        // fix if target is out of bounds
        if (field.target_ind < 0) field.target_ind = 0;
        if (field.target_ind > 2) field.target_ind = 2;
        // check if target was actually moved
        if (old_ind != field.target_ind)
        {
            uiManager.setTarget(field.target_ind);
        }
        //SPELLBOOK CODE

        // go to next page if down is pressed
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (castManager.pageDown())
                AudioPlayer.main.playSFX("sfx_spellbook_scroll", 0.3F);
            //else {play sfx_thud (player is on the last page this direction)}
        }
        // go to last page if down is pressed
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (castManager.pageUp())
                AudioPlayer.main.playSFX("sfx_spellbook_scroll", 0.3F);
            //else {play sfx_thud (player is on the last page this direction)}
        }
    }

    public void setEnabled(bool e)
    {
        enabled = e;
        trackTyping.enabled = e;
    }
    public void setPause(bool p)
    {
        pause = p;
        trackTyping.enabled = !p;
    }

    //MAIN BATTLE FLOW-----------------------------------------------------------------------//

    // start battle scene
    public void startBattle(GameObject new_battle)
    {
        //Reset player stats and status
        player.restoreToFull();
        battleKeyboard.clearStatus();
        castManager.cooldown.removeAll();
        //Reset BattleManager curr variables
        curr_battle = new_battle;
        curr_wave = -1;
        waves = new_battle.GetComponents<BattleWave>();
        //Reset target
		field.target_ind = 1;
        StartCoroutine(finishBattlePrep());
    }
    // finishes up battle start and effects
    IEnumerator finishBattlePrep()
    {
        // play battle transition
		pause = true;
        BattleEffects.main.battleTransitionEffect("swirl_in", 1f);
        yield return new WaitForSeconds(1f);
        uiManager.initBg();
        BattleEffects.main.battleTransitionEffect("swirl_out", 1f);
        yield return new WaitForSeconds(1f);
        nextWave();
        yield return new WaitForSeconds(3f);
        uiManager.initTarget();
    }
    // go to next wave (also starts first wave for real)
    public void nextWave()
    {
        if (++curr_wave >= waves.Length)
        {
            Debug.Log("Encounter over: going to victory screen");
            victoryScreen();
            return;
        }
        StartCoroutine(waveTransition());
    }
    private IEnumerator waveTransition()
    {
        setPause(true);
        resetInterruptData();
        Debug.Log("starting wave: " + Wave.Title);
        foreach (Transform tr in transform)
        {
            Destroy(tr.gameObject);
        }
        uiManager.startWave();

        //WAVE TRANSITION STANDIN (UNITL ACTUAL GOOD STUFF IS ADDED)
        uiManager.wave_banner_text.text = Wave.Title;
        uiManager.wave_title_text.text = "Wave " + (curr_wave + 1) + "/ " + waves.Length.ToString();
        while (uiManager.wave_transition_banner.color.a < 1)
        {
            yield return new WaitForEndOfFrame();
            float apl = Time.deltaTime;
            Color tmp = uiManager.wave_transition_banner.color;
            tmp.a += apl;
            uiManager.wave_transition_banner.color = tmp;
            tmp = uiManager.wave_transition_title.color;
            tmp.a += apl;
            uiManager.wave_transition_title.color = tmp;
            tmp = uiManager.wave_banner_text.color;
            tmp.a += apl;
            uiManager.wave_banner_text.color = tmp;
            tmp = uiManager.wave_title_text.color;
            tmp.a += apl;
            uiManager.wave_title_text.color = tmp;
        }
        yield return new WaitForSeconds(0.5f);
        while (uiManager.wave_transition_banner.color.a > 0)
        {
            yield return new WaitForEndOfFrame();
            float apl = Time.deltaTime;
            Color tmp = uiManager.wave_transition_banner.color;
            tmp.a -= apl;
            uiManager.wave_transition_banner.color = tmp;
            tmp = uiManager.wave_transition_title.color;
            tmp.a -= apl;
            uiManager.wave_transition_title.color = tmp;
            tmp = uiManager.wave_banner_text.color;
            tmp.a -= apl;
            uiManager.wave_banner_text.color = tmp;
            tmp = uiManager.wave_title_text.color;
            tmp.a -= apl;
            uiManager.wave_title_text.color = tmp;
        }

        createEnemies(Wave);
        uiManager.updateUI();
        if (Wave.Music != string.Empty)
            AudioPlayer.main.playMusic(Wave.Music);
        if(checkInterrupts() == false)
            setPause(false);
    }
    // show victory screen after all waves are done
    public void victoryScreen()
    {
        //Transition to victoryScreen
        endBattle();
    }
    // end the battle and transition to the next GameflowItem
    public void endBattle()
    {
        pause = true;
        foreach (Enemy enemy in field.enemy_arr)
        {
            if (enemy != null) GameObject.Destroy(enemy.gameObject);
        }
		foreach (Transform tr in transform) 
		{
			Destroy (tr.gameObject);
		}
        uiManager.clear();
        GameflowManager.main.next();

    }

    //END MAIN BATTLE FLOW-------------------------------------------------------------------//

    //Handles a spellcast (by calling the castmanager) and clears callback's buffer if necessary
    public void handleSpellCast(string spell, TrackTyping callback)
    {
        AudioPlayer.main.playSFX("sfx_enter"); // MIGHT WANT TO BE MOVED
        castManager.attackCurrent(spell, callback);
    }
    //Handle player death
    public void playerDeath()
    {
        Debug.Log("The player has died!");
        StopAllCoroutines();
        castManager.StopAllCoroutines();
        uiManager.StopAllCoroutines();
        pause = true;
        foreach (Enemy enemy in field.enemy_arr)
        {
            if (enemy != null) GameObject.Destroy(enemy.gameObject);
        }
        foreach(BattleWave w in waves)
        {
            foreach(BattleInterruptTrigger  e in  w.events)
            {
                e.HasTriggered = false;
            }
        }
        field.curr_dead = 0;
        field.enemy_count = 0;
        resetInterruptData();
        uiManager.clear();
        startBattle(curr_battle);
    }
    //Update Enemies and Check for death
    public void updateEnemies()
    {
        for (int i = 0; i < field.enemy_arr.Length; i++)
        {
            if (field.enemy_arr[i] != null && !field.enemy_arr[i].Is_dead)
            {
                field.enemy_arr[i].updateCondition();
                if (field.enemy_arr[i].Is_dead)
                    ++field.curr_dead;
            }
        }
        uiManager.updateUI();
        if (player.Is_dead)
        {
            playerDeath();
        }
        else if (field.curr_dead >= field.enemy_count) // next wave if all enemies dead
        {
            Debug.Log("Wave: " + Wave.Title + " complete!");
            nextWave();
        }
        else
            checkInterrupts();
    }

    //Create all enemies for this wave
    private void createEnemies(BattleWave wave)
    {
        if (wave.Enemy1 != string.Empty)
            createEnemy(0, wave.Enemy1);
        if (wave.Enemy2 != string.Empty)
            createEnemy(1, wave.Enemy2);
        if (wave.Enemy3 != string.Empty)
            createEnemy(2, wave.Enemy3);
    }
    // creates the enemy specified at 'i' (0-left, 1-mid, 2-right) by the 'scene'
    private void createEnemy(int i, string name)
    {
        ++field.enemy_count;
        Enemy enemy = Instantiate(enemy_prefab, transform).GetComponent<Enemy>(); ;
        enemy.transform.localScale = new Vector3(1, 1, 1);
		Vector3 enemy_pos = new Vector3(i * BattleUI.enemy_spacing, BattleUI.enemy_y_offset, 0);
		enemy.transform.localPosition = enemy_pos;
        enemy.field = field; 
        enemy.castManager = castManager;
        enemy.initialize(enemyData.getData(name)); //sets enemy stats (AND INITITIALIZES ATTACKING AND AI)
        enemy.Position = i;      //Log enemy position in field
        enemy.bars = uiManager.charge_bars; //Give enemy access to charge_bars
        Vector3 bar_pos = enemy.transform.position + new Vector3(-0.5f, -1.0f, 0);
        uiManager.charge_bars.makeChargeMeter(i, bar_pos);
        uiManager.stagger_bars.makeStaggerMeter(i, bar_pos);
        uiManager.health_bars.makeHealthMeter(i, bar_pos);
        field.enemy_arr[i] = enemy;
    }

    private bool checkInterrupts()
    {
        foreach (BattleEventTrigger e in Wave.events)
        {
            if (!e.HasTriggered && e.checkTrigger(field) && e.onTrigger(field))
            {
                setPause(true);
                return true;
            }
        }
        return false;
    }

    private void resetInterruptData()
    {

        field.time_started = System.DateTime.Now; // time battle started
        if(field.last_cast != null)
            field.last_cast.Clear(); // last performed cast action
        field.last_spell = new SpellData(""); // last performed spell
        field.last_register = new bool[3] { false, false, false }; // last spell register status
        field.num_player_attacks = 0; // number of player attacks from beginning of battle
    }

}
