using DungeonCrawler.Actions;
using DungeonCrawler.Collision;
using DungeonCrawler.Handlers;
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

        public QuadTree quadTree;

        public List<RectangleShape> renderQueue = new List<RectangleShape>();

        public InGameState(TmxMap map, Texture tileset)
        {
            this.map = map;
            this.tileset = tileset;
            quadTree = new QuadTree(new FloatRect(-10000000, -100000000, 1000000000, 1000000000), 2);
        }

        public override void Init()
        {
            //t
            GenerateCollisionMap();
        }

        public override void Render()
        {
            Game.view.Center = Game.player.rect.Position;
            Game.app.SetView(Game.view);

            DrawMap(map, tileset);

            foreach (Entity ent in World.Entities.Values)
            {
                if ((ent.flags & Entity.Flags.RENDER) == Entity.Flags.RENDER)
                    Game.app.Draw(ent.rect);
            }

        }

        public override void Update()
        {
            ControlPlayer(Game.player.currentCharacter.MovementSpeed);
            World.Update();
            if (InputHandler.MouseButtonPressed(Mouse.Button.Left))
            {
                Game.player.OnPrimaryFire();
            }
            if (InputHandler.MouseButtonDown(Mouse.Button.Right))
            {
                Game.player.OnSecondaryFire();
            }
        }

        private void ControlPlayer(float playerSpeed)
        {
            

            //Check left and right. Seperated so that collision is not checked while inside a box because of the other direction.
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                Game.player.Move(new Vector2f(-playerSpeed, 0));
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                Game.player.Move(new Vector2f(playerSpeed, 0));
            }

            //check up and down.
            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                Game.player.Move(new Vector2f(0, -playerSpeed));
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                Game.player.Move(new Vector2f(0, playerSpeed));
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
                            World.Entities[wall.Id].flags = Entity.Flags.WALL;

                        }
                    }

                }
            }
        }
    }
}
