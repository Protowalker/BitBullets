using DungeonCrawler.Entities;
using DungeonCrawler.States;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    class ScoutShotgunAction : Action
    {
        int playerId;
        

        public ScoutShotgunAction(int playerId)
        {
            this.playerId = playerId;
            finished = true;
        }

        public override void Update(float elapsed)
        {
            Player player = (Player)(Game.states[Game.currentState]).netState.Entities[playerId];
            Random rand = new Random();
            for(int i = 0; i < 10; i++)
            {
                float angle = (float)Math.Atan2(player.direction.Y, player.direction.X);
                angle += (float)rand.Next(-10,10)*.1f*rand.Next(-1,1);

                Vector2f randVelocity = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
                ShotgunPellet pellet = new ShotgunPellet(player.rect.Position, randVelocity, player.Id);
            }
        }
    }
}
