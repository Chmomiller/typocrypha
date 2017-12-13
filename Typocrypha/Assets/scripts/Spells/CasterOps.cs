﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Defines static operations for ICaster children
public static class CasterOps
{
    //Applies damage formula base on base_power (spell strength) and stats of caster and target)
    //damages ref stats appropriately (will not go below zero)
    //Precondition: target.Stats.vsElement[element] != Elements.reflect
    public static bool calcDamage(int base_power, int element, ICaster caster, ICaster target, bool crit, bool is_stunned = false)
    {
        float dMod = base_power;
        int staggerDamage = 0;

        //Apply buff/debuffs here

        //Apply stat mods here

        //Absorb damage if enemy absorbs this type
        if (target.Stats.vsElement[element] == Elements.absorb)
        {
            Debug.Log(target.Stats.name + " absorbs " + dMod + " " + Elements.toString(element) + " damage");
            if (target.Curr_hp + Mathf.CeilToInt(dMod) > target.Stats.max_hp)
                target.Curr_hp = target.Stats.max_hp;
            else
                target.Curr_hp += Mathf.CeilToInt(dMod);
            return false;
        }

        //Apply crit here
        if (crit)
            staggerDamage++;

        //Apply elemental weakness/resistances
        dMod *= target.Stats.vsElement[element];
        if (target.Stats.vsElement[element] > 1)//If enemy is weak
            staggerDamage++;

        //Apply stun damage mod (if stunned)
        if (is_stunned)
            dMod *= (1.25F + (0.25F * staggerDamage));
        Debug.Log(target.Stats.name + " was hit for " + dMod + " of " + Elements.toString(element) + " damage x" + target.Stats.vsElement[element]);
        //Apply shield
        if (target.Curr_shield > 0)
        {
            if (target.Curr_shield - dMod < 0)//Shield breaks
            {
                target.Curr_shield = 0;
                target.Curr_hp -= Mathf.FloorToInt(dMod - target.Curr_shield);
                if (staggerDamage >= 1 && is_stunned == false)
                    target.Curr_stagger--;
            }
            else
                target.Curr_shield -= Mathf.FloorToInt(dMod);
        }
        else
        {
            target.Curr_hp -= Mathf.CeilToInt(dMod);
            //Stagger if enemy is actually damaged
            if (staggerDamage >= 1 && !is_stunned && dMod > 0)
                target.Curr_stagger--;
        }
        if (target.Curr_hp < 0)
            target.Curr_hp = 0;
        return dMod > 0;
    }
}
