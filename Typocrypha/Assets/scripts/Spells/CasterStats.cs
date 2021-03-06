﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class with all read-only value that defines the necessary stats for any caster
public class CasterStats
{
    //Sets all read-only values
    public CasterStats(string name, string chat, int hp, int shield, int stagger, float atk, float def, float speed, float acc, int evade, float[] vsElem = null)
    {
        this.name = name;
        chatDatabaseID = chat;
        max_hp = hp;
        max_shield = shield;
        max_stagger = stagger;
        attack = atk;
        defense = def;
        this.speed = speed;
        accuracy = acc;
        evasion = evade;
        vsElement = vsElem;
    }

    //Readonly fields

    public string name;     //name
    public int max_hp;      //max health
    public int max_stagger; //max stagger
    public int max_shield;  //max shield
    //Spell modifiers 
    public float attack;      //numerical damage boost
    public float defense;     //numerical damage reduction
    public float speed;       //percentage of casting time reduction
    public float accuracy;      //numerical hitchance boost
    public int evasion;       //numerical dodgechance boost
    public float[] vsElement; //elemental weaknesses/resistances

    //chat database stuff
    private string chatDatabaseID;
    public virtual string ChatDatabaseID { get{ return chatDatabaseID; } }

    //Return the equivalent of this modified by debuff mod
    public CasterStats modify(BuffDebuff mod)
    {
        float atk = attack * mod.Attack;
        float def = defense * mod.Defense;
        float spd = speed * mod.Speed;
        float acc = accuracy * mod.Accuracy;
        int evade = Mathf.FloorToInt(evasion * mod.Evasion);
        float[] vE;
        vE = new float[Elements.count];

        for (int i = 0; i < Elements.count; i++) 
        {
            vE[i] = mod.modElementState(vsElement[i], i);
        }
            
        return new CasterStats(name, chatDatabaseID, max_hp, max_shield, max_stagger, atk, def, spd, acc, evade, vE);
    }
    //Just get debuff modified ACCURACY
    public float getModAcc(BuffDebuff mod)
    {
        return accuracy * mod.Accuracy;
    }
    //Just get debuff modified EVADE
    public int getModEvade(BuffDebuff mod)
    {
        return Mathf.FloorToInt(evasion * mod.Evasion);
    }
    public Elements.vsElement getModVsElement(BuffDebuff mod, int element)
    {
        return mod.modElementLevel(vsElement[element], element);
    }
    public float getFloatVsElement(BuffDebuff mod, int element)
    {
        return mod.modElementState(vsElement[element], element);
    }
}
