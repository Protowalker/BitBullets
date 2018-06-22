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
            Wall,
            RifleShot
        }

        public EntityType type;

        public static int highestId = 0;

        protected int id;
        protected int parentId;
        public abstract int Id { get; set; }
        public abstract int ParentId { get; set; }
        public abstract float moveSpeed { get; set; }

        public abstract float health { get; set; }
        
        internal Vector2f moveDelta = new Vector2f();

        public RectangleShape rect = new RectangleShape();

        public Vector2f direction = new Vector2f();

        public delegate void MoveHandler(int id, Vector2f delta);
        public delegate void DieHandler(Entity ent);
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
        public virtual void Update(float deltaTime)
        {
            if (health <= 0)
            {
                DieEvent(this);
            }
        }

        //Called on Entity death
        public abstract void Destroy();

        public abstract NetEntity ToNetEntity();

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
            {
                return false;
            }
            else
            {
                Entity ent = (Entity)obj;
                if (type == ent.type &&
                   id == ent.id &&
                   parentId == ent.parentId &&
                   moveSpeed == ent.moveSpeed &&
                   moveDelta.X == ent.moveDelta.X &&
                   moveDelta.Y == ent.moveDelta.Y &&
                   rect.Position.X == ent.rect.Position.X &&
                   rect.Position.Y == ent.rect.Position.Y &&
                   direction.X == ent.direction.X &&
                   direction.Y == ent.direction.Y &&
                   flags == ent.flags) return true;
                else return false;
            }

        }

        public abstract void HandleCollision();

        public override int GetHashCode()
        {
            return id.GetHashCode() + (id * rect.GetHashCode()) * moveDelta.GetHashCode();
        }

        public Entity Clone()
        {
            Entity ent = (Entity)MemberwiseClone();
            ent.rect = new RectangleShape(rect);

            return ent;
        }
    }
}
