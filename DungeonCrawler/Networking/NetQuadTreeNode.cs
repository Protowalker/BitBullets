using DungeonCrawler.Collision;
using MessagePack;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Networking
{
    //[MessagePackObject]
    class NetQuadTreeNode
    {
        [Key(0)]
        public NetQuadTreeNode parent;
        [Key(1)]
        public List<int> items = new List<int>();
        [Key(2)]
        public float rectX;
        [Key(3)]
        public float rectY;
        [Key(4)]
        public float rectWidth;
        [Key(5)]
        public float rectHeight;
        [Key(6)]
        public int maxItems;
        [Key(7)]
        public NetQuadTreeNode topLeftNode;
        [Key(8)]
        public NetQuadTreeNode topRightNode;
        [Key(9)]
        public NetQuadTreeNode bottomLeftNode;
        [Key(10)]
        public NetQuadTreeNode bottomRightNode;
        [Key(11)]
        public bool isPartitioned;

        public QuadTreeNode ToQuadTreeNode()
        {
            QuadTreeNode node;
            if(parent == null)
            {
                node = new QuadTreeNode(new FloatRect(rectX, rectY, rectWidth, rectHeight), maxItems);
            } else
            {
                node = new QuadTreeNode(parent.ToQuadTreeNode(), new FloatRect(rectX, rectY, rectWidth, rectHeight), maxItems);
            }

            node.items = items;
            node.isPartitioned = isPartitioned;

            if(topLeftNode != null) node.topLeftNode = topLeftNode.ToQuadTreeNode();
            if(topRightNode != null) node.topRightNode = topRightNode.ToQuadTreeNode();
            if(bottomLeftNode != null) node.bottomLeftNode = bottomLeftNode.ToQuadTreeNode();
            if (bottomRightNode != null) node.bottomRightNode = bottomRightNode.ToQuadTreeNode();

            return node;
        }
    }
}
