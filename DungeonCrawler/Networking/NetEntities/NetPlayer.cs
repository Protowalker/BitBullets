using MessagePack;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    [MessagePackObject]
    public class NetPlayer : NetEntity
    {
        [Key(12)]
        private Character currentCharacter;
        [Key(13)]
        private float health;
        [Key(14)]
        private float angle;

        [Key(15)]
        public Character CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }
        [Key(16)]
        public virtual float Health { get => health; set => health = value; }
        [Key(17)]
        public virtual float Angle { get => angle; set => angle = value; }

        public override Entity ToEntity()
        {
            Player player = new Player();
            player.Id = Id;
            player.currentCharacter = CurrentCharacter;
            RectangleShape rect = new RectangleShape();
            rect.Position = new SFML.System.Vector2f(RectX, RectY);
            rect.Size = new SFML.System.Vector2f(RectWidth, RectHeight);

            player.rect = rect;

            player.moveDelta = new SFML.System.Vector2f(MoveDeltaX, MoveDeltaY);

            player.direction = new SFML.System.Vector2f((float)Math.Cos(Angle), (float)Math.Sin(Angle));

            return player;
        }
    }
}
