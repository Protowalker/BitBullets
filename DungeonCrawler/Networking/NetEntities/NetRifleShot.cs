using DungeonCrawler.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking.NetEntities
{
    [MessagePackObject]
    public class NetRifleShot : NetEntity
    {
        [Key(13)]
        public float DirectionX;
        [Key(14)]
        public float DirectionY;

        public override Entity ToEntity()
        {
            RifleShot rifleShot = new RifleShot(new SFML.System.Vector2f(RectX, RectY), new SFML.System.Vector2f(DirectionX, DirectionY), ParentId);
            rifleShot.flags = Flags;
            rifleShot.Id = Id;
            rifleShot.moveDelta.X = MoveDeltaX;
            rifleShot.moveDelta.Y = MoveDeltaY;
            rifleShot.moveSpeed = MoveSpeed;
            rifleShot.rect.Size = new SFML.System.Vector2f(1, 1);
            rifleShot.type = Type;

            return rifleShot;
        }
    }
}
