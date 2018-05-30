using DungeonCrawler.Entities;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking.NetEntities
{
    [Serializable]
    class NetShotgunPellet : NetEntity
    {
        internal float damagePoints = 10;
        internal int age = 0;

        internal float velocityX;
        internal float velocityY;

        public override Entity ToEntity()
        {
            ShotgunPellet pellet = new ShotgunPellet(new SFML.System.Vector2f(rectX, rectY), new SFML.System.Vector2f(velocityX, velocityY), parentId);

            RectangleShape rect = new RectangleShape();
            rect.Position = new SFML.System.Vector2f(rectX, rectY);
            rect.Size = new SFML.System.Vector2f(rectWidth, rectHeight);

            pellet.rect = rect;

            pellet.moveDelta = new SFML.System.Vector2f(moveDeltaX, moveDeltaY);

            pellet.damagePoints = damagePoints;
            pellet.age = age;

            return pellet;

        }
    }
}
