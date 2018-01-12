﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple container for Ally stats (Not a struct anymore cuz structs pass by value in c#)
public class AllyStats : CasterStats
{
    //Sorry for the massive constructor but all the vals are readonly so...
    public AllyStats(string name, int hp, int shield, int stag, float atk, float def, float speed, float acc, int evade, float[] vsElem = null)
        : base(name, hp, shield, stag, atk, def, speed, acc, evade, vsElem)
    {
        spell = new SpellData(name.ToLower());
    }
    public readonly SpellData spell; // castable spell
}

public class Ally : MonoBehaviour, ICaster {

    //ICASTER STUFF//

    private AllyStats stats;
    public CasterStats Stats
    {
        get
        {
            return stats;
        }
    }

    private BuffDebuff buffDebuff = new BuffDebuff();
    public BuffDebuff BuffDebuff { get { return buffDebuff; } }

    private int gauge_value = 50;
    public int Curr_hp { get { return gauge_value; } set { gauge_value = value; } }
    public int Curr_shield { get { return 0; } set { } }
    private int curr_stagger;
    public int Curr_stagger { get { return curr_stagger; } set { curr_stagger = value; } }

    bool is_stunned = false;
    public bool Is_stunned { get { return is_stunned; } }
    private bool is_dead = false;
    public bool Is_dead { get { return is_dead; } }

    public ICasterType CasterType { get { return ICasterType.NPC_ALLY; } }

    public void damage(CastData data, int d, int element, ICaster caster, bool crit, bool reflect = false)
    {
        throw new System.NotImplementedException();
    }

    public void setStats(AllyStats stats)
    {
        this.stats = stats;
    }

    //END ICASTER STUFF//

    public int position; //position in battle field
    public int target = 1;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
