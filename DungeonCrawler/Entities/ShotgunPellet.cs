using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;

namespace DungeonCrawler.Entities
{
    class ShotgunPellet : Entity
    {
        float damagePoints = 10;
        public static float speed = 10;

        int age = 0;

        Vector2f velocity;

        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }

        public ShotgunPellet(Vector2f position, Vector2f velocity, int parentId)
        {
            rect.Size = new Vector2f(1,1);
            rect.Position = position;
            rect.FillColor = Color.Red;
            this.velocity = velocity;
            this.parentId = parentId;
            this.flags = this.flags | Entity.Flags.PROJECTILE | Entity.Flags.RENDER;
            id = highestId + 1;
            highestId = id;

            World.Entities.Add(id, this);
            ((InGameState)Game.currentState).quadTree.Insert(id);
        }


        public override void Destroy()
        {
            if (((InGameState)Game.currentState).quadTree.RemoveItem(id))
            {
                World.EntitiesForDestruction.Add(id);
            }
            //rect.Dispose();
        }

        public override void Update(float deltaTime)
        {
            InGameState curState = (InGameState)Game.currentState;
            List<int> collided = curState.quadTree.GetItems(rect.GetGlobalBounds());

            Move(velocity);

            foreach(int id in collided)
            {
                    if ((World.Entities[id].flags & Entity.Flags.PLAYER) == Entity.Flags.PLAYER  && World.Entities[id].Id != ParentId)
                    {
                    Player player = (Player)World.Entities[id];
                    player.TakeDamage(damagePoints);
                    Destroy();
                    }
                    else if ((World.Entities[id].flags & Entity.Flags.WALL) == Entity.Flags.WALL)
                    {
                            Destroy();
                            break;
                    }
            }

            //Console.WriteLine(rect.Position);

            rect.Position += moveDelta;
            moveDelta = new Vector2f();

            age += 1;

            if (age == 1000) Destroy();
        }
    }
}
