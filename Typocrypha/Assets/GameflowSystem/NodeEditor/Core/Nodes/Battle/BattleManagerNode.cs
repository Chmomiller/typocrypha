﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework;

namespace TypocryphaGameflow
{
    [Node(true, "Battle/ManagerBase", new System.Type[] { typeof(GameflowCanvas) })]
    public abstract class BattleManagerNode : BaseGameflowNode
    {

        public override bool AutoLayout { get { return true; } }
        //Connection from previous node (INPUT)
        [ConnectionKnob("From Previous", Direction.In, "Gameflow", ConnectionCount.Multi, NodeSide.Left, 30)]
        public ConnectionKnob fromPreviousIN;
        //Next Node to go to (OUTPUT)
        [ConnectionKnob("To Next", Direction.Out, "Gameflow", ConnectionCount.Single, NodeSide.Right, 30)]
        public ConnectionKnob toNextOUT;
        public override BaseGameflowNode next()
        {
            return toNextOUT.connections[0].body as BaseGameflowNode;
        }
    }
}

