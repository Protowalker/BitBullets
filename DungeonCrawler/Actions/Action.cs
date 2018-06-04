using DungeonCrawler.States;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [Union(0, typeof(MoveAction))]
    [Union(1, typeof(ScoutShotgunAction))]
    [Union(2, typeof(ScoutViewfinderAction))]
    [Union(3, typeof(CompositeAction))]
    [Union(4, typeof(ChangeDirectionAction))]
    [MessagePackObject]
    public abstract class Action
    {
        [SerializationConstructor]
        public Action()
        {
            id = -1;
        }

        public Action(int id) {
            this.id = id;
            if (Game.currentState == State.StateType.InGameState)
            {
                if(((InGameState)Game.states[Game.currentState]).playerId == id)
                {
                    Game.states[Game.currentState].netState.actionsForSending.Add(this);
                }
            }
        }

        public abstract void Update(float elapsed);

        [Key("finished")]
        public bool finished;
        [Key("id")]
        public int id;
    }
}
