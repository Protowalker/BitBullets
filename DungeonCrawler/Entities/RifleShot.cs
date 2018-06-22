using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using DungeonCrawler.Networking;
using DungeonCrawler.Networking.NetEntities;
using SFML.System;

namespace DungeonCrawler.Entities
{
    class RifleShot : Entity
    {
        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }
        public override float moveSpeed { get; set; }
        public override float health { get; set; }

        public RifleShot(Vector2f position, Vector2f direction, int parentId)
        {
            moveSpeed = 4f;
            rect = new SFML.Graphics.RectangleShape(new Vector2f(1,1));
            rect.Position = position;
            this.parentId = parentId;
            this.direction = direction;
            type = EntityType.RifleShot;
            flags = Flags.PROJECTILE | Flags.RENDER;
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void Update(float deltaTime)
        {
            rect.Position += direction * moveSpeed * deltaTime;
            HandleCollision();
        }

        public override void HandleCollision()
        {
            
        }

        public override void Init()
        {
            id = Entity.highestId + 1;
            highestId = id;
            (Game.states[Game.currentState]).netState.Entities.Add(id, this);
            (Game.states[Game.currentState]).netState.quadTree.Insert(id);
        }

        public override NetEntity ToNetEntity()
        {
            NetRifleShot netRifleShot = new NetRifleShot();
            netRifleShot.Flags = flags;
            netRifleShot.Id = id;
            netRifleShot.MoveDeltaX = moveDelta.X;
            netRifleShot.MoveDeltaY = moveDelta.Y;
            netRifleShot.MoveSpeed = moveSpeed;
            netRifleShot.ParentId = parentId;
            netRifleShot.RectHeight = 1;
            netRifleShot.RectWidth = 1;
            netRifleShot.RectX = rect.Position.X;
            netRifleShot.RectY = rect.Position.Y;
            netRifleShot.Type = type;
            netRifleShot.DirectionX = direction.X;
            netRifleShot.DirectionY = direction.Y;

            return netRifleShot;

        }
    }
}
