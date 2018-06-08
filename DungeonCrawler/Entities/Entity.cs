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


namespace DungeonCrawler
{
    public abstract class Entity
    {
        [Serializable]
        public enum EntityType
        {
            Player,
            ShotgunPellet,
            Wall
        }

        public EntityType type;

        public static int highestId = 0;

        protected int id;
        protected int parentId;
        public abstract int Id { get; set; }
        public abstract int ParentId { get; set; }
        public abstract float moveSpeed { get; set; }
        
        internal Vector2f moveDelta = new Vector2f();

        public RectangleShape rect = new RectangleShape();

        public Vector2f direction = new Vector2f();

        public delegate void MoveHandler(int id, Vector2f delta);
        public delegate void DieHandler(Entity ent, int timeOfDeath);
        public event MoveHandler MoveEvent = delegate { };
        public event DieHandler DieEvent = delegate { };

        [Serializable]
        [Flags]
        public enum Flags { PLAYER = 1,
                            WALL = 2,
                            RENDER = 4,
                            PROJECTILE = 8
                            };
        public Flags flags;

        public void Move(Vector2f delta)
        {
            moveDelta += delta;
            MoveEvent(id, delta);
        }

        public abstract void Init();
        public abstract void Update(float deltaTime);

        //Called on Entity death
        public abstract void Destroy();

        public abstract NetEntity ToNetEntity();

        public Entity Clone()
        {
            return (Entity)MemberwiseClone();
        }
    }
}
