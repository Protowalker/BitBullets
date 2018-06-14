using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class ScoutViewfinderAction : Action
    {
        bool zoomed;

        [SerializationConstructor]
        public ScoutViewfinderAction(int id, bool finished) : base(id)
        {
            zoomed = true;
            this.id = id;
            this.finished = finished;
            if (!finished)
            {
                ((Player)Game.states[Game.currentState].netState.Entities[id]).FOV *= 1.6f;
                ((Player)Game.states[Game.currentState].netState.Entities[id]).moveSpeed *= .09f;
            }
        }

        public override void Update(float elapsed)
        {
            if (!Handlers.InputHandler.MouseButtonDown(SFML.Window.Mouse.Button.Right))
            {
                zoomed = false;
                finished = true;
                new ScoutViewfinderAction(id, true);
            }
        }

        public override void CleanUp()
        {
            if (zoomed)
            {
                ((Player)Game.states[Game.currentState].netState.Entities[id]).FOV /= 1.6f;
                ((Player)Game.states[Game.currentState].netState.Entities[id]).moveSpeed /= .09f;
            }
        }
    }
}
