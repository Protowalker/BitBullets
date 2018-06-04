using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class ChangeDirectionAction : Action
    {
        [Key("angle")]
        public float angle;

        [SerializationConstructor]
        public ChangeDirectionAction(float angle)
        {
            this.angle = angle;
        }

        public ChangeDirectionAction(int id, float angle) : base(id)
        {
            this.angle = angle;
            finished = true;
        }

        public override void Update(float elapsed)
        {
            Game.states[Game.currentState].netState.Entities[id].direction = new SFML.System.Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
    }
}
