﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Builds and stores a database of enemy stats
public class EnemyDatabase
{
    public static EnemyDatabase main = new EnemyDatabase("enemyDatabase");
    Dictionary<string, EnemyStats> database = new Dictionary<string, EnemyStats>();

    private const int numFields = 8;

    TextAsset text_file; // original text asset
    public string file_name; // name of database file
    char[] line_delim = { '\n' };
    char[] col_delim = { '\t' };

    public EnemyDatabase(string path)
    {
        file_name = path;
    }

    //Builds database from text file specified by text_file
    public void build()
    {
        text_file = Resources.Load<TextAsset>(file_name);
        string[] lines = text_file.text.Split(line_delim);
        string[] cols;
        //Declare fields
        string name;
        int max_hp, max_shield,atk,def,acc,evade;//declare stat variables
        float speed;
        float[] vsElem;
        //For each line in input file
        for (int i = 1; lines[i].Trim().CompareTo("END") != 0; i++)
        {
            cols = lines[i].Split(col_delim);
            name = cols[0].Trim();
            int.TryParse(cols[1].Trim(), out max_hp);
            int.TryParse(cols[2].Trim(), out max_shield);
            int.TryParse(cols[3].Trim(), out atk);
            int.TryParse(cols[4].Trim(), out def);
            float.TryParse(cols[5].Trim(), out speed);
            int.TryParse(cols[6].Trim(), out acc);
            int.TryParse(cols[7].Trim(), out evade);
            vsElem = new float[Elements.count];
            //Read in elemental weakness/resistances
            for (int j = numFields; j < numFields + Elements.count; j++)
            {
                float.TryParse(cols[j].Trim(), out vsElem[j - numFields]);
            }
            //Read in Spell List
            List<SpellData> spells = new List<SpellData>();
            for (int j = numFields + Elements.count; cols[j].Trim().CompareTo("END") != 0; j++)
            {
                string root = cols[j].Trim();
                j++;
                string elem = cols[j].Trim();
                if (elem.CompareTo("null") == 0)
                    elem = null;
                j++;
                string style = cols[j].Trim();
                if (style.CompareTo("null") == 0)
                    style = null;
                SpellData s = new SpellData(root, elem, style);
                spells.Add(s);
            }
            EnemyStats stats = new EnemyStats(name, max_hp, max_shield, atk, def, speed, acc, evade, vsElem, spells.ToArray());
            database.Add(stats.name, stats);
        }
        Debug.Log("Enemy Database Loaded");
    }
    public EnemyStats getData(string id)
    {
        return database[id];
    }
}