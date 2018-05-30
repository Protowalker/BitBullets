using DungeonCrawler.Characters;
using DungeonCrawler.States;
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
using static DungeonCrawler.States.State;

namespace DungeonCrawler.Actions
{
    class Game
    {
        public static Clock deltaClock = new Clock();


        static readonly Vector2u resolution = new Vector2u(800, 600);
        static readonly string title = "RPG";
        public static RenderWindow app = new RenderWindow(new VideoMode(resolution.X, resolution.Y), title);
        public static View view;

        public static Dictionary<StateType, State> states = new Dictionary<StateType, State>();
        public static StateType currentState;


        static TmxMap map = new TmxMap("Assets/TestMap1.tmx");
        static Texture tileset = new Texture("Assets/16x16_Jerom_CC-BY-SA-3.0_0.png");
        static List<RectangleShape> colliders = new List<RectangleShape>();

        static bool isServer;

        public static void Init()
        {
            Console.WriteLine("Do you want to run server? y/n");
            isServer = Console.ReadLine().StartsWith("y");

            if (isServer)
            {
                ServerState serverState = new ServerState(map);
                currentState = StateType.ServerState;
                states.Add(StateType.ServerState, serverState);
                states[currentState].Init();
            }
            else
            {
                InGameState inGameState = new InGameState(map, tileset);
                currentState = StateType.InGameState;
                states.Add(StateType.InGameState, inGameState);
            }
            app.Closed += new EventHandler(onClose);
            view = app.GetView();
            view.Size = (Vector2f)resolution * .25f;
            view.Center = new Vector2f(0, 0);
            app.SetView(view);

        }

        public static void Main()
        {
            Init();

            while (app.IsOpen)
            {
                app.DispatchEvents();
                app.Clear();

                states[currentState].Render();
                states[currentState].Update();

                Console.WriteLine("test");

                app.Display();


                deltaClock.Restart();
            }
        }

      

        private static void onClose(object sender, EventArgs e)
        {
            app.Close();
        }

    }
}
