using DungeonCrawler.Actions;
using DungeonCrawler.Networking;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;

namespace DungeonCrawler.States
{
    class ServerState : State
    {
        TmxMap map;

        public override float TickRate => 1000 / 20;

        
        public ServerState(TmxMap map, int port)
        {
            this.map = map;

            netState = new NetServerGameState(port);
        }

        public override void Init()
        {
            GenerateCollisionMap();
            ((NetServerGameState)netState).Init();
        }

        public override void Render(float deltaTime)
        {
        }

        public override void Update(float deltaTime)
        {
            netState.Update(deltaTime);
        }



        private void GenerateCollisionMap()
        {

            var tilesetTiles = map.Tilesets[0].Tiles;

            for (int i = 0; i < map.Layers.Count; i++)
            {
                for (int j = 0; j < map.Layers[i].Tiles.Count; j++)
                {
                    var tile = map.Layers[i].Tiles[j];

                    int x = tile.X * map.TileWidth;
                    int y = tile.Y * map.TileHeight;

                    if (tile.Gid == 0) continue;

                    if (tilesetTiles.Keys.Contains(tile.Gid - 1))
                    {
                        TmxObject[] rects = tilesetTiles[tile.Gid - 1].ObjectGroups[0].Objects.ToArray();

                        foreach (TmxObject obj in rects)
                        {
                            RectangleShape rect = new RectangleShape(new Vector2f((float)obj.Width, (float)obj.Height));
                            rect.Position = new Vector2f(x, y) + new Vector2f((float)obj.X, (float)obj.Y);
                            Wall wall = new Wall(rect);
                            wall.Init();
                            Game.states[Game.currentState].netState.Entities[wall.Id].flags = Entity.Flags.WALL;

                        }
                    }

                }
            }
        }
    }
}
