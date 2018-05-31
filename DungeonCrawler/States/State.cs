﻿using DungeonCrawler.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.States
{
    abstract class State
    {
        public NetGameState netState;

        public enum StateType
        {
            InGameState,
            ServerState
        }

        public abstract void Init();
        public abstract void Update();
        public abstract void Render();
    }
}