using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    [Serializable]
    abstract class NetEntity
    {
        public Entity.EntityType type;

        public Entity.Flags flags;

        protected int id;
        protected int parentId;
        public int Id { get; set; }
        public int ParentId { get; set; }

        public float moveDeltaX;
        public float moveDeltaY;

        public float rectX;
        public float rectY;
        public float rectWidth;
        public float rectHeight;

        public abstract Entity ToEntity();
    }
}
