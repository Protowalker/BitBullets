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
        internal float damagePoints = 10;
        internal int age = 0;

        internal float velocityX;
        internal float velocityY;

        public override Entity ToEntity()
        {
            ShotgunPellet pellet = new ShotgunPellet(new SFML.System.Vector2f(RectX, RectY), new SFML.System.Vector2f(velocityX, velocityY), ParentId);

            RectangleShape rect = new RectangleShape();
            rect.Position = new SFML.System.Vector2f(RectX, RectY);
            rect.Size = new SFML.System.Vector2f(RectWidth, RectHeight);

            pellet.rect = rect;

            pellet.moveDelta = new SFML.System.Vector2f(MoveDeltaX, MoveDeltaY);

            pellet.damagePoints = damagePoints;
            pellet.age = age;

            return pellet;

        }
    }
}
