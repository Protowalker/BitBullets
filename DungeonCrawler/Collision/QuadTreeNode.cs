using DungeonCrawler.Actions;
using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Collision
{
    class QuadTreeNode
    {
        QuadTreeNode parent;
        internal List<int> items = new List<int>();

        internal FloatRect rect;
        int maxItems;

        QuadTreeNode topLeftNode;
        QuadTreeNode topRightNode;
        QuadTreeNode bottomLeftNode;
        QuadTreeNode bottomRightNode;



        bool isPartitioned;

        public QuadTreeNode(QuadTreeNode parent, FloatRect rect, int maxItems)
        {
            this.parent = parent;
            this.rect = rect;
            this.maxItems = maxItems;

        }

        public QuadTreeNode(FloatRect rect, int maxItems)
        {
            this.rect = rect;
            this.maxItems = maxItems;
        }

        public void Insert(int item)
        {
            if (!InsertInChild(item))
            {
                World.Entities[item].MoveEvent += new Entity.MoveHandler(ItemMove);
                items.Add(item);
            }

            if (!isPartitioned & items.Count > maxItems)
            {
                Partition();
            }
        }

        private void Partition()
        {
            Vector2f topLeft = new Vector2f(rect.Left, rect.Top);
            Vector2f bottomRight = new Vector2f(rect.Left + rect.Width, rect.Top + rect.Height);
            Vector2f midPoint = (topLeft + bottomRight) / 2;
            Vector2f midPointLocal = (bottomRight - topLeft) / 2;

            topLeftNode = new QuadTreeNode(this, new FloatRect(topLeft, midPointLocal), maxItems);
            topRightNode = new QuadTreeNode(this, new FloatRect(new Vector2f(midPoint.X, topLeft.Y), midPointLocal), maxItems);
            bottomLeftNode = new QuadTreeNode(this, new FloatRect(new Vector2f(topLeft.X, midPoint.Y), midPointLocal), maxItems);
            bottomRightNode = new QuadTreeNode(this, new FloatRect(midPoint, midPointLocal), maxItems);

            List<int> removalList = new List<int>();

            isPartitioned = true;

            foreach (int item in items)
            {
                if (InsertInChild(item)) removalList.Add(item);
            }

            foreach (int item in removalList)
            {
                RemoveItem(items.IndexOf(item));
            }
        }

        private void UnPartition()
        {
            if (isPartitioned)
            {
                isPartitioned = false;
                topLeftNode.Destroy();
                topRightNode.Destroy();
                bottomLeftNode.Destroy();
                bottomRightNode.Destroy();
            }
        }

        //Get a list of items that intersect a certain rectangle
        public List<int> GetItems(FloatRect colRect)
        {
            List<int> itemsFound = new List<int>();
            if (rect.Intersects(colRect))
            {
                // test the point in each item
                foreach (int item in items)
                {
                    if (World.Entities[item].rect.GetGlobalBounds().Intersects(colRect)) itemsFound.Add(item);
                }

                // query all subtrees
                if (isPartitioned)
                {
                    itemsFound.AddRange(topLeftNode.GetItems(colRect));
                    itemsFound.AddRange(topRightNode.GetItems(colRect));
                    itemsFound.AddRange(bottomLeftNode.GetItems(colRect));
                    itemsFound.AddRange(bottomRightNode.GetItems(colRect));
                }
            }
            return itemsFound;
        }

        //Get all items in this node
        public List<int> GetAllItems()
        {
            List<int> itemsFound = new List<int>();

            itemsFound.AddRange(items);

            // query all subtrees
            if (isPartitioned)
            {
                itemsFound.AddRange(topLeftNode.GetAllItems());
                itemsFound.AddRange(topRightNode.GetAllItems());
                itemsFound.AddRange(bottomLeftNode.GetAllItems());
                itemsFound.AddRange(bottomRightNode.GetAllItems());
            }
            return itemsFound;
        }

        public QuadTreeNode FindItemNode(int item)
        {
            if (items.Contains(item)) return this;

            else if (isPartitioned)
            {
                QuadTreeNode n = null;

                FloatRect rect = World.Entities[item].rect.GetGlobalBounds();

                // Check the nodes that could contain the item
                if (topLeftNode.ContainsRect(rect))
                {
                    n = topLeftNode.FindItemNode(item);
                }
                if (n == null && topRightNode.ContainsRect(rect))
                {
                    n = topRightNode.FindItemNode(item);
                }
                if (n == null && bottomLeftNode.ContainsRect(rect))
                {
                    n = bottomLeftNode.FindItemNode(item);
                }
                if (n == null && bottomRightNode.ContainsRect(rect))
                {
                    n = bottomRightNode.FindItemNode(item);
                }

                return n;
            }
            else return null;
        }

        private void PushItemUp(int i)
        {
            int id = items[i];

            RemoveItem(i);
            parent.Insert(id);

            if (GetAllItems().Count < maxItems)
            {
                UnPartition();
            }
        }

        internal void RemoveItem(int i)
        {
            World.Entities[items[i]].MoveEvent -= new Entity.MoveHandler(ItemMove);
            items.RemoveAt(i);
        }

        private bool PushItemDown(int i)
        {
            if (InsertInChild(items[i]))
            {
                RemoveItem(i);
                return true;
            }

            else return false;

        }

        private bool InsertInChild(int item)
        {
            if (!isPartitioned)
            {
                return false;
            }

            FloatRect rect = World.Entities[item].rect.GetGlobalBounds();

            if (topLeftNode.ContainsRect(rect))
            {
                topLeftNode.Insert(item);
            }
            else if (topRightNode.ContainsRect(rect))
            {
                topRightNode.Insert(item);
            }
            else if (bottomLeftNode.ContainsRect(rect))
            {
                bottomLeftNode.Insert(item);
            }
            else if (bottomRightNode.ContainsRect(rect))
            {
                bottomRightNode.Insert(item);
            }
            else return false;

            return true;
        }

        private bool ContainsRect(FloatRect itemRect)
        {
            Vector2f topLeft = new Vector2f(rect.Left, rect.Top);
            Vector2f bottomRight = new Vector2f(rect.Left + rect.Width, rect.Top + rect.Height);
            Vector2f itemTopLeft = new Vector2f(itemRect.Left, itemRect.Top);
            Vector2f itemBottomRight = new Vector2f(itemRect.Left + itemRect.Width, itemRect.Top + itemRect.Height);
            return ((itemTopLeft.X > topLeft.X) && (itemTopLeft.Y > topLeft.Y) && (itemBottomRight.X < bottomRight.X) && (itemBottomRight.Y < bottomRight.Y));
        }

        public void Destroy()
        {
            if (parent != null)
            {
                for(int i = 0; i < items.Count; i++)
                {
                    PushItemUp(i);
                }
            }
            // Destroy all child nodes
            if (isPartitioned)
            {
                topLeftNode.Destroy();
                topRightNode.Destroy();
                bottomLeftNode.Destroy();
                bottomRightNode.Destroy();

                topLeftNode = null;
                topRightNode = null;
                bottomLeftNode = null;
                bottomRightNode = null;
            }

            // Remove all items
            while (items.Count > 0)
            {
                RemoveItem(0);
            }
        }

        private void ItemMove(int id, Vector2f delta)
        {
            Entity item = World.Entities[id];
            if (items.Contains(id))
            {
                int i = items.IndexOf(id);

                // Try to push the item down to the child
                if (!PushItemDown(i))
                {
                    // otherwise, if not root, push up
                    if (parent != null)
                    {
                        PushItemUp(i);
                    }
                }
            }
            else
            {
                // this node doesn't contain that item, stop notifying it about it
                item.MoveEvent -= new Entity.MoveHandler(ItemMove);
            }
        }
    }
}
