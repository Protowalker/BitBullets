using DungeonCrawler.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class SniperRifleAction : Action
    {
        [SerializationConstructor]
        public SniperRifleAction(int id) : base(id)
        {
            this.id = id;
            finished = true;
        }

        public override void Update(float elapsed)
        {
            if(Game.currentState == States.State.StateType.ServerState)
            {
                Player player = (Player)Game.states[Game.currentState].netState.Entities[id];
                RifleShot shot = new RifleShot(player.rect.Position, player.direction, id);
                shot.Init();
            }
        }
    }
}
