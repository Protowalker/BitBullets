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
    public class NetWall : NetEntity
    {
        public override Entity ToEntity()
        {
            RectangleShape rect = new RectangleShape(new SFML.System.Vector2f(RectX, RectY));
            rect.Position = new SFML.System.Vector2f(RectWidth, RectHeight);
            Wall wall = new Wall(rect);
            wall.Id = Id;
            wall.ParentId = ParentId;
            wall.flags = Flags;

            return wall;
        }
    }
}
