using DungeonCrawler.Entities;
using MessagePack;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking.NetEntities
{
    [MessagePackObject]
    public class NetShotgunPellet : NetEntity
    {
        [Key(13)]
        public float damagePoints = 10;
        [Key(14)]
        public int age = 0;
        [Key(15)]
        public float velocityX;
        [Key(16)]
        public float velocityY;

        public override Entity ToEntity()
        {
            ShotgunPellet pellet = new ShotgunPellet(new SFML.System.Vector2f(RectX, RectY), new SFML.System.Vector2f(velocityX, velocityY), ParentId);
            pellet.Id = Id;

            RectangleShape rect = new RectangleShape();
            rect.Position = new SFML.System.Vector2f(RectX, RectY);
            rect.Size = new SFML.System.Vector2f(RectWidth, RectHeight);
            rect.FillColor = Color.Red;

            pellet.rect = rect;

            pellet.moveDelta = new SFML.System.Vector2f(MoveDeltaX, MoveDeltaY);

            pellet.damagePoints = damagePoints;
            pellet.age = age;

            pellet.moveSpeed = MoveSpeed;

            return pellet;

        }
    }
}
