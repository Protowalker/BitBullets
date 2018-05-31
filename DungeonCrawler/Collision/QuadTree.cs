using DungeonCrawler.Actions;
using DungeonCrawler.Networking;
using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Collision
{
    class QuadTree
    {

        protected QuadTreeNode headNode;

        protected int maxItems;

        public QuadTree(QuadTreeNode headNode, int maxItems)
        {
            this.headNode = headNode;
            this.maxItems = maxItems;
        }

        public QuadTree(FloatRect worldRect, int maxItems)
        {
            this.headNode = new QuadTreeNode(worldRect, maxItems);
            this.maxItems = maxItems;
        }

        public void Insert(int item)
        {
            FloatRect rect = Game.states[Game.currentState].netState.Entities[item].rect.GetGlobalBounds();
            // check if the world needs resizing
            if (!headNode.rect.Intersects(rect))
            {
                Vector2f topLeft = new Vector2f(headNode.rect.Left, headNode.rect.Top);
                Vector2f bottomRight = new Vector2f(headNode.rect.Width, headNode.rect.Height);
                Vector2f itemTopLeft = new Vector2f(rect.Left, rect.Top);
                Vector2f itemBottomRight = new Vector2f(rect.Width, rect.Height);
                Resize(new FloatRect(
                     new Vector2f(Math.Min(topLeft.X, itemTopLeft.X), Math.Min(topLeft.Y, itemTopLeft.Y)) * 2,
                     new Vector2f(Math.Max(bottomRight.X, itemBottomRight.X), Math.Max(bottomRight.Y, itemBottomRight.Y)) * 2));
            }

            headNode.Insert(item);
        }

        public void Resize(FloatRect newWorld)
        {
            // Get all of the items in the tree
            List<int> components = new List<int>();
            components = GetAllItems();

            // Destroy the head node
            headNode.Destroy();
            headNode = null;

            // Create a new head
            headNode = new QuadTreeNode(newWorld, maxItems);

            // Reinsert the items
            foreach (int id in components)
            {
                headNode.Insert(id);
            }
        }

        public List<int> GetItems(FloatRect Rect)
        {
            return headNode.GetItems(Rect);
        }

        public List<int> GetAllItems()
        {
            return headNode.GetAllItems();
        }

        public QuadTreeNode FindItemNode(int itemId)
        {
            return headNode.FindItemNode(itemId);
        }

        public bool RemoveItem(int itemId)
        {
            QuadTreeNode node = FindItemNode(itemId);
            if (node != null)
            {
                node.RemoveItem(node.items.IndexOf(itemId));
                node = FindItemNode(itemId);
                return true;
            }
            else return false;
        }

        public NetQuadTree ToNetQuadTree()
        {
            NetQuadTree tree = new NetQuadTree();
            tree.headNode = headNode.ToNetQuadTreeNode();
            tree.maxItems = maxItems;

            return tree;
        }
    }
}
