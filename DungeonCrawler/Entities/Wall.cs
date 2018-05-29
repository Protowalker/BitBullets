using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using DungeonCrawler.States;
using SFML.Graphics;

namespace DungeonCrawler
{
    class Wall : Entity
    {
        public Wall(RectangleShape rect)
        {
            id = highestId + 1;
            highestId = id;

            this.rect = rect;

            World.Entities.Add(id, this);
            ((InGameState)Game.currentState).quadTree.Insert(id);
        }

        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }

        

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void Update(float deltaTime)
        {
        }
    }
}
