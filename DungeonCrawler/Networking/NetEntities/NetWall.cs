using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking.NetEntities
{
    [Serializable]
    class NetWall : NetEntity
    {
        public override Entity ToEntity()
        {
            RectangleShape rect = new RectangleShape(new SFML.System.Vector2f(rectX, rectY));
            rect.Position = new SFML.System.Vector2f(rectWidth, rectHeight);
            Wall wall = new Wall(rect);
            wall.Id = id;
            wall.ParentId = parentId;
            wall.flags = flags;

            return wall;
        }
    }
}
