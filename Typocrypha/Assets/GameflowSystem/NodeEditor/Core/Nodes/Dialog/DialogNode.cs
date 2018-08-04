﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;

namespace TypocryphaGameflow
{
    [Node(true, "Dialog/DialogBase", new System.Type[] { typeof(GameflowCanvas) })]
    public abstract class DialogNode : BaseNodeIO
    {

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        public string characterName;
        public string text;

        public override BaseNode next()
        {
            return toNextOUT.connections[0].body as BaseNode;
        }
        public override ProcessFlag process()
        {
            //    player_ui.SetActive(false);
            BattleManagerS.main.setEnabled(false);
            DialogueManager.main.startDialogue(this);
            return ProcessFlag.Wait;
        }
    }
}