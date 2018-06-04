using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using DungeonCrawler.Networking;
using DungeonCrawler.Networking.NetEntities;
using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;

namespace DungeonCrawler.Entities
{
    class ShotgunPellet : Entity
    {
        internal float damagePoints = 10;

        internal int age = 0;

        Vector2f velocity;

        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }
        public override float moveSpeed { get; set; }

        public ShotgunPellet(Vector2f position, Vector2f velocity, int parentId)
        {
            moveSpeed = 5;
            rect.Size = new Vector2f(1,1);
            rect.Position = position;
            rect.FillColor = Color.Red;
            this.velocity = velocity;
            this.parentId = parentId;
            this.flags = this.flags | Entity.Flags.PROJECTILE | Entity.Flags.RENDER;
            id = highestId + 1;
            highestId = id;
            type = EntityType.ShotgunPellet;
        }


        public override void Init()
        {
            Game.states[Game.currentState].netState.Entities.Add(id, this);
            Game.states[Game.currentState].netState.quadTree.Insert(id);
        }

        public override void Destroy()
        {
            if (Game.states[Game.currentState].netState.quadTree.RemoveItem(id))
            {
                Game.states[Game.currentState].netState.EntitiesForDestruction.Add(id);
            }
            //rect.Dispose();
        }

        public override void Update(float deltaTime)
        {
            List<int> collided = Game.states[Game.currentState].netState.quadTree.GetItems(rect.GetGlobalBounds());

            Move(velocity);

            foreach(int id in collided)
            {
                    if ((Game.states[Game.currentState].netState.Entities[id].flags & Entity.Flags.PLAYER) == Entity.Flags.PLAYER  && Game.states[Game.currentState].netState.Entities[id].Id != ParentId)
                    {
                    Player player = (Player)Game.states[Game.currentState].netState.Entities[id];
                    player.TakeDamage(damagePoints);
                    Destroy();
                    }
                    else if ((Game.states[Game.currentState].netState.Entities[id].flags & Entity.Flags.WALL) == Entity.Flags.WALL)
                    {
                            Destroy();
                            break;
                    }
            }

            //Console.WriteLine(rect.Position);

            rect.Position += moveDelta;
            moveDelta = new Vector2f();

            age += 1;

            if (age >= 1000) Destroy();
        }

        public override NetEntity ToNetEntity()
        {
            NetShotgunPellet netShotgunPellet = new NetShotgunPellet();
            netShotgunPellet.Flags = flags;
            netShotgunPellet.Id = id;
            netShotgunPellet.ParentId = parentId;
            netShotgunPellet.MoveDeltaX = moveDelta.X;
            netShotgunPellet.MoveDeltaY = moveDelta.Y;
            netShotgunPellet.RectX = rect.Position.X;
            netShotgunPellet.RectY = rect.Position.Y;
            netShotgunPellet.RectWidth = rect.Size.X;
            netShotgunPellet.RectHeight = rect.Size.Y;
            netShotgunPellet.damagePoints = damagePoints;
            netShotgunPellet.age = age;
            netShotgunPellet.velocityX = velocity.X;
            netShotgunPellet.velocityY = velocity.Y;
            netShotgunPellet.Type = type;
            netShotgunPellet.MoveSpeed = moveSpeed;

            return netShotgunPellet;
        }

    }
}
