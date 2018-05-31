﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using DungeonCrawler.Networking;
using DungeonCrawler.Networking.NetEntities;
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
            type = EntityType.Wall;
        }

        public override void Init()
        {
            Game.states[Game.currentState].netState.Entities.Add(id, this);
            Game.states[Game.currentState].netState.quadTree.Insert(id);
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

        public override NetEntity ToNetEntity()
        {
            NetWall netWall = new NetWall();
            netWall.Flags = flags;
            netWall.Id = id;
            netWall.ParentId = parentId;
            netWall.MoveDeltaX = moveDelta.X;
            netWall.MoveDeltaY = moveDelta.Y;
            netWall.RectX = rect.Position.X;
            netWall.RectY = rect.Position.Y;
            netWall.RectWidth = rect.Size.X;
            netWall.RectHeight = rect.Size.Y;
            netWall.Type = type;

            return netWall;
        }
    }
}