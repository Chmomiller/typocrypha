﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enum for how a cast went (successful cast, botched, fizzled, etc)
public enum CastStatus { SUCCESS, BOTCH, FIZZLE, ONCOOLDOWN, COOLDOWNFULL };

//Stores all the spell info and contains methods to parse and cast spells from player input
//Currently does not actually support player or enemy casting, but has parsing
public class SpellDictionary : MonoBehaviour
{
    public CooldownList cooldown;
    
    //Loads the spell dictionary at the beginning of the game
    public void Start()
    {
        is_loaded = false;
        buildDicts(); // load gameflow file
        is_loaded = true;
        Debug.Log("Dict loaded"); 
    }

    public bool is_loaded; // is the spellDict done loading?

	TextAsset text_file; // original text asset
    public string file_name; // name of gameflow file
    char[] line_delim = { '\n' };
	char[] col_delim = { '\t' };

    // parses Dictionary file which should be a tab-delimited txt file (from excel)
    public void buildDicts()
    {
		text_file = Resources.Load<TextAsset> (file_name);
		string[] lines = text_file.text.Split(line_delim);
        string[] cols;
        int i = 1;
        //read in root keywords
        while(true)
        {
            cols = lines[i].Split(col_delim);
            string key = cols[0].Trim(); 
            if (key.CompareTo("END") == 0) //At end of root keywords
            {
                i += 2;//Skip example line
                break;
            }
            string type = cols[1].Trim();
            Spell s = createSpellFromType(type);
            s.name = key;
            s.type = type;
            s.description = cols[2].Trim();
            int.TryParse(cols[3].Trim(), out s.power);
            float.TryParse(cols[4].Trim(), out s.cooldown);
            int.TryParse(cols[5].Trim(), out s.hitPercentage);
            int.TryParse(cols[6].Trim(), out s.critPercentage);
            int.TryParse(cols[7].Trim(), out s.elementEffectMod);
            string pattern = cols[8].Trim();
            if (pattern.Contains("A"))
            {
                s.targetData = new TargetData(true);
                s.targetData.selfCenter = false;
                s.targetData.targeted = false;
            }
            else
            {
                s.targetData = new TargetData(false);
                if (pattern.Contains("L"))
                    s.targetData.enemyL = true;
                if (pattern.Contains("M"))
                    s.targetData.enemyM = true;
                if (pattern.Contains("R"))
                    s.targetData.enemyR = true;
                if (pattern.Contains("l"))
                    s.targetData.allyL = true;
                if (pattern.Contains("m"))
                    s.targetData.allyM = true;
                if (pattern.Contains("r"))
                    s.targetData.enemyR = true;
                if (pattern.Contains("S"))
                    s.targetData.selfCenter = true;
                if (pattern.Contains("T"))
                    s.targetData.targeted = true;
            }
            spells.Add(key, s);
            i++;
        }
        //read in element keywords
        while (true)
        {
            cols = lines[i].Split(col_delim);
            string key = cols[0].Trim();
            if (key.CompareTo("END") == 0)
            {
                i += 2;//Skip example line
                break;
            }
            ElementMod e = new ElementMod();
            e.name = key;
            e.element = Elements.fromString(cols[1].Trim());
            e.description = cols[2].Trim();
            float.TryParse(cols[3].Trim(), out e.cooldownMod);
            float.TryParse(cols[4].Trim(), out e.cooldownModM);
            elements.Add(key, e);
            i++;
        }
        //Read in casting style keywords
        while (true)
        {
            cols = lines[i].Split(col_delim);
            string key = cols[0].Trim();
            if (key.CompareTo("END") == 0)
            {
                return;
            }
            StyleMod s = new StyleMod();
            s.name = key;
            int.TryParse(cols[1].Trim(), out s.powerMod);
            s.description = cols[2].Trim();
            float.TryParse(cols[3].Trim(), out s.powerModM);
            float.TryParse(cols[4].Trim(), out s.cooldownMod);
            float.TryParse(cols[5].Trim(), out s.cooldownModM);
            int.TryParse(cols[6].Trim(), out s.accMod);
            float.TryParse(cols[7].Trim(), out s.accModM);
            int.TryParse(cols[8].Trim(), out s.critMod);
            float.TryParse(cols[9].Trim(), out s.critModM);
            int.TryParse(cols[10].Trim(), out s.statusEffectChanceMod);
            float.TryParse(cols[11].Trim(), out s.statusEffectChanceModM);
            string pattern = cols[12].Trim();
            if (pattern.Contains("N"))
                s.isTarget = false;
            else if (pattern.Contains("A"))
            {
                s.targets = new TargetData(true);
                s.targets.selfCenter = false;
                s.targets.targeted = false;
            }
            else
            {
                s.targets = new TargetData(false);
                if (pattern.Contains("L"))
                    s.targets.enemyL = true;
                if (pattern.Contains("M"))
                    s.targets.enemyM = true;
                if (pattern.Contains("R"))
                    s.targets.enemyR = true;
                if (pattern.Contains("l"))
                    s.targets.allyL = true;
                if (pattern.Contains("m"))
                    s.targets.allyM = true;
                if (pattern.Contains("r"))
                    s.targets.enemyR = true;
                if (pattern.Contains("S"))
                    s.targets.selfCenter = true;
                if (pattern.Contains("T"))
                    s.targets.targeted = true;
            }
            styles.Add(key, s);
            i++;
        }
    }

    //parses input spell, and returns an appropriate CastStatus code and a built (possibly null, possibly invalid Spelldata)
	public CastStatus parse(string spell, out SpellData s)
    {
        char[] delim = { ' ' };
        string[] lines = spell.Split(delim);
        CastStatus status;
		if (lines.Length == 1)
        {
			string first = lines [0].Trim ();
			if (spells.ContainsKey (first))
            {
                s = new SpellData(first);
                status = CastStatus.SUCCESS;
			}
            else
            {
				s = new SpellData ("b", null, null);
				status = CastStatus.BOTCH;
			}
		}
        else if (lines.Length == 2)
        {
			string first = lines [0].Trim ();
			string second = lines [1].Trim ();
			if (spells.ContainsKey (first))
            {
				if (styles.ContainsKey (second))
                {
                    s = new SpellData(first, null, second);
                    status = CastStatus.SUCCESS;
				}
                else
                {
					s = new SpellData(first, null, "b");
					status = CastStatus.BOTCH;
				}
			}
            else if (spells.ContainsKey (second))
            {
				if (elements.ContainsKey (first))
                {
                    s = new SpellData(second, first, null);
                    status = CastStatus.SUCCESS;
				}
                else
                {
					s = new SpellData("b", second, null);
					status = CastStatus.BOTCH;
				}
			}
            else
            {
                s = new SpellData("b", "b", null);
                status = CastStatus.BOTCH;
            }
		}
        else if (lines.Length == 3)
        {
			string elem = lines [0].Trim ();
			string root = lines [1].Trim ();
			string style = lines [2].Trim ();
			if (spells.ContainsKey (root))
            {
				if (elements.ContainsKey (elem))
                {
					if (styles.ContainsKey (style))
                    {
                        s = new SpellData(root, elem, style);
                        status = CastStatus.SUCCESS;
					}
                    else
                    {
						s = new SpellData (root, elem, "b");
						status = CastStatus.BOTCH;
					}
				}
                else if (styles.ContainsKey (style))
                {
					s = new SpellData(root, "b", style);
					status = CastStatus.BOTCH;
				}
                else
                {
					s = new SpellData(root, "b", "b");
					status = CastStatus.BOTCH;
				}
			}
            else if (elements.ContainsKey (elem))
            {
				if (styles.ContainsKey (style))
                {
					s = new SpellData("b", elem, style);
					status = CastStatus.BOTCH;
				}
                else
                {
					s = new SpellData("b", elem, "b");
					status = CastStatus.BOTCH;
				}
			}
            else if (styles.ContainsKey (style))
            {
				s = new SpellData("b", "b", style);
				status = CastStatus.BOTCH;
			}
            else
            {
				s = new SpellData("b", "b", "b");
				status = CastStatus.BOTCH;
			}
		}
        else
        {
			s = new SpellData("FIZZLE");
			status = CastStatus.FIZZLE;
		}
        //Get Cooldown
        if (status == CastStatus.SUCCESS)
        {
            if (cooldown.isFull())
            {
                return CastStatus.COOLDOWNFULL;
            }
            else if (isOnCooldown(s))
            {
                return CastStatus.ONCOOLDOWN;
            }
            else
                return CastStatus.SUCCESS;
        }
        else
            return status;
    }
    //Casts spell from NPC (enemy or ally)
    public List<CastData> cast(SpellData spell, ICaster[] targets, int selected, ICaster[] allies, int position)
    {
        ICaster caster = allies[position];
        Spell s = spells[spell.root];
        Spell c = createSpellFromType(s.type);
        s.copyInto(c);
        ElementMod e;
        StyleMod st;
        if (spell.element == null)
            e = null;
        else
            e = elements[spell.element];
        if (spell.style == null)
            st = null;
        else
            st = styles[spell.style];
        c.Modify(e, st);
        List<ICaster> toCastAt = c.target(targets, selected, allies, position);
        List<CastData> data = new List<CastData>();
        foreach(ICaster target in toCastAt)
        {
            CastData castData = c.cast(target, caster);
            if(castData.reflect == true)
                castData.setLocationData(caster, target);
            else
                castData.setLocationData(target, caster);
            data.Add(castData);
        }
        return data;
    }
    //Will contain method for botching a spell
    public List<CastData> botch(SpellData s, ICaster[] targets, int selected, ICaster[] allies, int position)
    {
        return null;
    }
    //Gets casting time of input spell
    public float getCastingTime(SpellData s, float speed)
    {
        float time = spells[s.root].cooldown;
        if (s.element != null && s.style != null)
        {
            float baseTime = time;
            time *= elements[s.element].cooldownModM;
            time += (baseTime * styles[s.style].cooldownModM) - baseTime;
            time += elements[s.element].cooldownMod;
            time += styles[s.style].cooldownMod;
        }
        else if(s.element != null)
        {
            time *= elements[s.element].cooldownModM;
            time += elements[s.element].cooldownMod;
        }
        else if (s.style != null)
        {
            time *= styles[s.style].cooldownModM;
            time += styles[s.style].cooldownMod;
        }
        return time * speed;
    }
    //Starts cooldown of spell
    public void startCooldown(SpellData data, Player caster)
    {
        Spell spell = spells[data.root];
        ElementMod e;
        StyleMod s;
        float cooldownTime = spell.cooldown;
        if (data.element != null && data.style != null)
        {
            e = elements[data.element];
            s = styles[data.style];
            float baseTime = cooldownTime;
            cooldownTime *= e.cooldownModM;
            cooldownTime += (baseTime * s.cooldownModM) - baseTime;
            cooldownTime += e.cooldownMod;
            cooldownTime += s.cooldownMod;
        }
        else if (data.element != null)
        {
            e = elements[data.element];
            cooldownTime *= e.cooldownModM;
            cooldownTime += e.cooldownMod;
        }
        else if (data.style != null)
        {
            s = styles[data.style];
            cooldownTime *= s.cooldownModM;
            cooldownTime += s.cooldownMod;
        }
        spell.startCooldown(cooldown, data.root, caster.Stats.speed * cooldownTime);
    }
    //Return cooldown of spell
    //Pre: spell is on cooldown
    public float getTimeLeft(SpellData data)
    {
        Spell s = spells[data.root];
        return s.TimeLeft;
    }

    //PRIVATE//--------------------------------------------------------------------------------------------------------------------------------------------//

    //Helper method for cloning appropriately typed spells
    private Spell createSpellFromType(string type)
    {
        if (type.CompareTo("attack") == 0)
            return new AttackSpell();
        else if (type.CompareTo("heal") == 0)
            return new HealSpell();
        else if (type.CompareTo("shield") == 0)
            return new ShieldSpell();
        else
            return null;
    }
    //Returns if spell s is on cooldown
    //Pre: coolDownList is not full
    private bool isOnCooldown(SpellData s)
    {
        Spell spell = spells[s.root];//Get root keyword from dictionary
        if (spell.IsOnCooldown)//Casting fails if root is on cooldown
        {
            return true;
        }
        return false;
    }

    //Dictionaries containing spells associated with keywords
    private Dictionary<string, Spell> spells = new Dictionary<string, Spell>();
    private Dictionary<string, ElementMod> elements = new Dictionary<string, ElementMod>();
    private Dictionary<string, StyleMod> styles = new Dictionary<string, StyleMod>();
}
//A class containing the required data to cast a spell (with defined keyword composition)
//Also contains associated methods like ToString()
public class SpellData
{
    //Make a new spelldata instance with root keyword root, element keyword prefix, and style keyword suffix
    public SpellData(string root, string prefix = null, string suffix = null)
    {
        this.root = root;
        element = prefix;
        style = suffix;
    }
    public string root;
    public string element;
    public string style;
    //Returns a string representation of the spell (Display mode, with "-" delimiters and all caps)
    public override string ToString()
    {
        string result;
        if (element != null)
            result = element + "-" + root;
        else
            result = root;
        if (style != null)
            result += ("-" + style);
        return result.ToUpper();
    }
}
//A class containing the result data from a casted spell (which can be used to generate animations an effects)
//Contains hit/miss, crit, stun, elemental weakness/resistance status, damage inflicted, etc.
//Does not contain cast status data (Botch, Fizzle)
public class CastData
{
    //Elemental vs status
    public enum vsElement
    {
        INVALID,
        NEUTERAL,
        WEAK,
        RESISTANT,
        ABSORB,
        REFLECT,
    }

    //Data fields
    public bool isHit = false;
    public bool isCrit = false;
    public bool isStun = false;
    public int damageInflicted = 0;
    public int element = Elements.notAnElement;
    public vsElement elementalData = vsElement.INVALID;

    public ICaster Target
    {
        get { return target; }
    }
    public ICaster Caster
    {
        get { return caster; }
    }

    //Location data (used for targeting)
    ICaster target;
    ICaster caster;

    //Just used, in cast INNACURATE
    public bool reflect = false;

    //Used to set location data
    public void setLocationData(ICaster target, ICaster caster)
    {
        this.target = target;
        this.caster = caster;
    }
  
}
//Class containing element constants and associated methods.
//Essentially a glorified int enum
public static class Elements
{
    public const int count = 4;

    public const int notAnElement = -1;
    public const int @null = 0;
    public const int fire = 1;
    public const int ice = 2;
    public const int bolt = 3;

    public const int absorb = -1;
    public const int reflect = -2;
    public const int weak = -3;
    public const int reflect_mod = 1;

    //Returns integer form of element for equivalent elementName string
    public static int fromString(string elementName)
    {
        switch (elementName)
        {
            case "null":
                return @null;
            case "fire":
                return fire;
            case "ice":
                return ice;
            case "bolt":
                return bolt;
            default:
                return notAnElement;
        }
    }

    public static string toString(int elementNum)
    {
        switch (elementNum)
        {
            case 0:
                return "null";
            case 1:
                return "fire";
            case 2:
                return "ice";
            case 3:
                return "bolt";
            default:
                return "not an element";
        }
    }
}



