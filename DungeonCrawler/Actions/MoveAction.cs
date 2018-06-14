using DungeonCrawler.Networking;
using DungeonCrawler.States;
using MessagePack;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class MoveAction : Action
    {
        [Key("directionX")]
        public int directionX;
        [Key("directionY")]
        public int directionY;

        [SerializationConstructor]
        public MoveAction(int id, int directionX, int directionY) : base(id)
        {
            if (directionX < 0) directionX = -1;
            if (directionX > 0) directionX = 1;
            if (directionY < 0) directionY = -1;
            if (directionY > 0) directionY = 1;
            this.directionX = directionX;
            this.directionY = directionY;

        }

        public override void Update(float elapsed)
        {
            float moveSpeed = Game.states[Game.currentState].netState.Entities[id].moveSpeed * elapsed;
            Game.states[Game.currentState].netState.Entities[id].Move(new Vector2f(directionX * moveSpeed, directionY * moveSpeed));
            finished = true;
        }
    }
}
