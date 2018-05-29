using DungeonCrawler.States;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;

namespace DungeonCrawler.Actions
{
    class World
    {
        static Clock deltaClock = new Clock();

        static Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> Entities { get => entities; private set => entities = value; }

        static List<int> entitiesForDestruction = new List<int>();
        public static List<int> EntitiesForDestruction { get => entitiesForDestruction; set => entitiesForDestruction = value; }


        static Parallel realtimeActions = new Parallel();
        public static Parallel RealTimeActions { get => realtimeActions; private set => realtimeActions = value; }

        public static void Update()
        {
            realtimeActions.Update(deltaClock.ElapsedTime.AsMilliseconds());
            foreach (Entity ent in Entities.Values)
            {
                ent.Update(deltaClock.ElapsedTime.AsMilliseconds());
            }

            foreach(int i in EntitiesForDestruction)
            {
                entities.Remove(i);
            }
            entitiesForDestruction.Clear();

            deltaClock.Restart();
        }
    }
}