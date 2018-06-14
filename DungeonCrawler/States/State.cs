using DungeonCrawler.Networking;
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
        public abstract float TickRate { get; }

        public enum StateType
        {
            InGameState,
            ServerState
        }

        public abstract void Init();
        public abstract void Update(float deltaTime);
        public abstract void Render();
    }
}
