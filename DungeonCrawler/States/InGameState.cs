using DungeonCrawler.Actions;
using DungeonCrawler.Characters;
using DungeonCrawler.Collision;
using DungeonCrawler.Handlers;
using DungeonCrawler.Networking;
using Lidgren.Network;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;

namespace DungeonCrawler.States
{
    class InGameState : State
    {

        public TmxMap map;
        public Texture tileset;


        public List<RectangleShape> renderQueue = new List<RectangleShape>();

        public int playerId;

        public InGameState(TmxMap map, Texture tileset, string host, int port)
        {
            this.map = map;
            this.tileset = tileset;
            netState = new NetGameState(host, port);
            TickRate = 1000 / 100;
        }

        public override void Init()
        {
            Game.app.MouseMoved += new EventHandler<MouseMoveEventArgs>(OnMouseMove);
            Game.app.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(Handlers.InputHandler.OnMouseButtonPressed);
            Game.app.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(Handlers.InputHandler.OnMouseButtonReleased);
            //GenerateCollisionMap();
        }

        public override void Render()
        {
            if (netState.ready)
            {
                Game.view.Center = netState.Entities[playerId].rect.Position;
                Game.app.SetView(Game.view);

                DrawMap(map, tileset);

                foreach (Entity ent in Game.states[Game.currentState].netState.Entities.Values)
                {
                    if ((ent.flags & Entity.Flags.RENDER) == Entity.Flags.RENDER)
                        Game.app.Draw(ent.rect);
                }
            }

        }


        public override void Update()
        {
            netState.Update();
            if (netState.ready)
            {
                if (Game.app.HasFocus())
                {
                    Input();
                }
            }
        }

        private void Input()
        {
            ControlPlayer();
            if (InputHandler.MouseButtonPressed(Mouse.Button.Left))
            {
                netState.RealTimeActions.Add(new ScoutShotgunAction(playerId));
            }
            if (InputHandler.MouseButtonDown(Mouse.Button.Right)) 
            {
                netState.RealTimeActions.Add(new ScoutShotgunAction(playerId));
            }
        }

        private void ControlPlayer()
        {
            

            //Check left and right. Seperated so that collision is not checked while inside a box because of the other direction.
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                netState.RealTimeActions.Add(new MoveAction(playerId, -1, 0));
            }

            else if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                netState.RealTimeActions.Add(new MoveAction(playerId, 1, 0));
            }

            //check up and down.
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                netState.RealTimeActions.Add(new MoveAction(playerId, 0, -1));
            }

            else if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                netState.RealTimeActions.Add(new MoveAction(playerId, 0, 1));
            }

        }

        private void DrawMap(TmxMap map, Texture tileset)
        {
            for (int i = 0; i < map.Layers.Count; i++)
            {
                for (int j = 0; j < map.Layers[i].Tiles.Count; j++)
                {

                    var tile = map.Layers[i].Tiles[j];
                    int gid = tile.Gid - 1;




                    int x = tile.X * map.TileWidth;
                    int y = tile.Y * map.TileHeight;

                    if (gid == -1) continue;
                    if (x - map.TileWidth > Game.view.Center.X + Game.view.Size.X / 2)
                        continue;
                    if (x + map.TileWidth < Game.view.Center.X - Game.view.Size.X / 2)
                        continue;
                    if (y - map.TileHeight > Game.view.Center.Y + Game.view.Size.Y / 2)
                        continue;
                    if (y + map.TileHeight < Game.view.Center.Y - Game.view.Size.Y / 2)
                        continue;

                    int columns = (int)tileset.Size.X / map.TileWidth;
                    int rows = (int)tileset.Size.Y / map.TileHeight;

                    RectangleShape tileRect = new RectangleShape(new Vector2f(map.TileWidth, map.TileHeight));
                    tileRect.Position = new Vector2f(x, y);
                    tileRect.Texture = tileset;

                    //Find the row by finding how many times columns goes into gid. Find the column by finding what's left over
                    tileRect.TextureRect = new IntRect((gid % columns) * map.TileWidth, (int)Math.Floor((double)(gid / columns)) * map.TileHeight, map.TileWidth, map.TileHeight);

                    Game.app.Draw(tileRect);
                }
            }
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
                            Game.states[Game.currentState].netState.Entities[wall.Id].flags = Entity.Flags.WALL;

                        }
                    }

                }
            }
        }

        public void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            Vector2f mousePos = Game.app.MapPixelToCoords(new Vector2i(e.X, e.Y));
            Vector2f relPos = mousePos - netState.Entities[playerId].rect.Position;
            float angle = (float)Math.Atan2(relPos.Y, relPos.X);
            netState.RealTimeActions.Add(new ChangeDirectionAction(playerId, angle));

        }
    }
}
