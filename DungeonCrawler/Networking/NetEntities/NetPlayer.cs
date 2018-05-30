using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    [Serializable]
    class NetPlayer : NetEntity
    {
        public Character currentCharacter;

        public float health;

        public float angle;

        public override Entity ToEntity()
        {
            Player player = new Player();
            player.Id = id;
            player.currentCharacter = currentCharacter;
            RectangleShape rect = new RectangleShape();
            rect.Position = new SFML.System.Vector2f(rectX, rectY);
            rect.Size = new SFML.System.Vector2f(rectWidth, rectHeight);

            player.rect = rect;

            player.moveDelta = new SFML.System.Vector2f(moveDeltaX, moveDeltaY);

            player.direction = new SFML.System.Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));

            return player;
        }
    }
}
