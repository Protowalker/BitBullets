using DungeonCrawler.Characters;
using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;

namespace DungeonCrawler.Actions
{
    class Game
    {
        static readonly Vector2u resolution = new Vector2u(800, 600);
        static readonly string title = "RPG";
        public static RenderWindow app = new RenderWindow(new VideoMode(resolution.X, resolution.Y), title);
        public static View view;

        public static State currentState;

        static TmxMap map = new TmxMap("Assets/TestMap1.tmx");
        static Texture tileset = new Texture("Assets/16x16_Jerom_CC-BY-SA-3.0_0.png");
        static List<RectangleShape> colliders = new List<RectangleShape>();
        public static Player player;

        

        public static void Init()
        {
            currentState = new InGameState(map, tileset);
            app.Closed += new EventHandler(onClose);
            view = app.GetView();
            view.Size = (Vector2f)resolution * .25f;
            view.Center = new Vector2f(0, 0);
            app.SetView(view);
            player = new Player();

            player.rect.Origin = player.rect.Size / 2;

            app.MouseMoved += new EventHandler<MouseMoveEventArgs>(Handlers.InputHandler.OnMouseMove);
            app.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(Handlers.InputHandler.OnMouseButtonPressed);
            app.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(Handlers.InputHandler.OnMouseButtonReleased);

            player.SetCurrentCharacter(new Scout(player));

            currentState.Init();
        }

        public static void Main()
        {
            Init();

            while (app.IsOpen)
            {
                app.DispatchEvents();
                app.Clear();

                currentState.Render();

                app.Display();

                currentState.Update();
            }
        }

      

        private static void onClose(object sender, EventArgs e)
        {
            app.Close();
        }

    }
}
