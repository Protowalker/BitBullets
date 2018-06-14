using DungeonCrawler.Collision;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    //[MessagePackObject]
    class NetQuadTree
    {
        [Key(0)]
        public NetQuadTreeNode headNode;
        [Key(1)]
        public int maxItems;

        public QuadTree ToQuadTree() {
            QuadTree tree = new QuadTree(headNode.ToQuadTreeNode(), maxItems);
            return tree;
        }
    }
}
