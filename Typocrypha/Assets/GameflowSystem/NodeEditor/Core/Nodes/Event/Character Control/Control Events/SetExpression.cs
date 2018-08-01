﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TypocryphaGameflow
{
    public class SetExpression : CharacterControlNode.EventData
    {
        public string expression = "base";
        public override void doGUI(Rect rect, int index, IList list)
        {
            Rect UIrect = new Rect(rect);
            UIrect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(UIrect, new GUIContent("Set Expression", ""), new GUIStyle(GUIStyle.none) { alignment = TextAnchor.MiddleCenter });
            UIrect.y += EditorGUIUtility.singleLineHeight + 1;
            characterName = EditorGUI.TextField(UIrect, characterName);
            UIrect.y += EditorGUIUtility.singleLineHeight + 1;
            expression = EditorGUI.TextField(UIrect, expression);
        }
        public override float getHeight(int index)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 3;
        }
    }
}
