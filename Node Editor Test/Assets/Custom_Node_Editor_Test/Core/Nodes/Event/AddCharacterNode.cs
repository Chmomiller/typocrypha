﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeEditorFramework;

namespace TypocryphaGameflow
{
    [Node(false, "Event/Add Character", new System.Type[] { typeof(GameflowCanvas) })]
    public class AddCharacterNode : GameflowStandardIONode
    {
        public const string ID = "Add Character Node";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Add Character"; } }
        public override Vector2 MinSize { get { return new Vector2(150, 40); } }

        public string characterName;
        public string startingExpression;
        public Vector2 pos;

        protected override void OnCreate()
        {
            characterName = "Character Name";
            startingExpression = "default";
            pos = new Vector2(0, 0);
        }

        public override void NodeGUI()
        {
            GUILayout.Space(5);
            GUILayout.BeginVertical("box");
            characterName = GUILayout.TextField(characterName);
            startingExpression = GUILayout.TextField(startingExpression);
            pos = EditorGUILayout.Vector2Field("", pos);
            GUILayout.EndVertical();
        }
    }
}
