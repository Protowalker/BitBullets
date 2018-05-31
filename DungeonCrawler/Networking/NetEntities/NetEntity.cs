using DungeonCrawler.Networking.NetEntities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    [Union(0, typeof(NetPlayer))]
    [Union(1, typeof(NetShotgunPellet))]
    [Union(2, typeof(NetWall))]
    [MessagePackObject]
    public abstract class NetEntity
    {
        [Key(0)]
        private Entity.EntityType type;

        [Key(1)]
        public virtual Entity.EntityType Type { get; set; }

        [Key(2)]
        private Entity.Flags flags;

        [Key(3)]
        public virtual Entity.Flags Flags { get; set; }

        [Key(4)]
        public virtual int Id { get; set; }
        [Key(5)]
        public virtual int ParentId { get; set; }
        [Key(6)]
        public virtual float MoveDeltaX { get; set; }
        [Key(7)]
        public virtual float MoveDeltaY { get; set; }
        [Key(8)]
        public virtual float RectX { get; set; }
        [Key(9)]
        public virtual float RectY { get; set; }
        [Key(10)]
        public virtual float RectWidth { get; set; }
        [Key(11)]
        public virtual float RectHeight { get; set; }


        public abstract Entity ToEntity();
    }
}
