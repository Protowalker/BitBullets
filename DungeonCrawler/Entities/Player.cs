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
    [Serializable]
    class Player : Entity
    {
        public Character currentCharacter;

        public float health;

        [NonSerialized]
        public Vector2f direction = new Vector2f();

        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }

        public Player()
        {
            flags = flags | Entity.Flags.PLAYER;
            flags = flags | Entity.Flags.RENDER;

            rect = new RectangleShape(new Vector2f(5,10));
            health = 1;
            id = Entity.highestId + 1;
            highestId = id;

            rect.Origin = rect.Size / 2;
            type = EntityType.Player;
        }

        public override void Init()
        {
            MoveEvent += new MoveHandler(Game.states[Game.currentState].netState.OnMove);
            (Game.states[Game.currentState]).netState.Entities.Add(id, this);
            (Game.states[Game.currentState]).netState.quadTree.Insert(id);
        }

        public void SetCurrentCharacter(Character currentCharacter)
        {
            health = currentCharacter.MaxHealth;
            this.currentCharacter = currentCharacter;
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }


        public override void Update(float deltaTime)
        {
            //Deal with movement.
            {
                //We do one direction at a time so that the character doesn't get moved in odd places.
                rect.Position += new Vector2f(moveDelta.X * deltaTime, 0);
                FloatRect overlap;

                List<int> collided = (Game.states[Game.currentState].netState.quadTree.GetItems(rect.GetGlobalBounds()));
                bool intersecting;

                foreach (int id in collided)
                {
                    if ((Game.states[Game.currentState].netState.Entities[id].flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = (Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap));

                    if (intersecting)
                    {
                        if (Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Left + Game.states[Game.currentState].netState.Entities[id].rect.Size.X > overlap.Left + overlap.Width) //if the player is to the right of the shape
                        {
                            rect.Position -= new Vector2f(overlap.Width, 0);
                        }
                        else if (Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Left < overlap.Left) //to the left
                        {
                            rect.Position += new Vector2f(overlap.Width, 0);
                        }
                    }
                }


                rect.Position += new Vector2f(0, moveDelta.Y * deltaTime);
                collided = Game.states[Game.currentState].netState.quadTree.GetItems(rect.GetGlobalBounds());

                //Now Y
                foreach (int id in collided)
                {
                    if ((Game.states[Game.currentState].netState.Entities[id].flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap);

                    if (intersecting)
                    {
                        if (Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Top < overlap.Top) //above
                        {
                            rect.Position += new Vector2f(0, overlap.Height);

                        }
                        else if (Game.states[Game.currentState].netState.Entities[id].rect.GetGlobalBounds().Top + Game.states[Game.currentState].netState.Entities[id].rect.Size.Y > overlap.Top + overlap.Height) //below
                        {
                            rect.Position -= new Vector2f(0, overlap.Height);
                        }
                    }
                }
                moveDelta = new Vector2f();
            }
        }

        public void OnPrimaryFire()
        {
            currentCharacter.OnPrimaryFire();
        }

        public void OnSecondaryFire()
        {
            currentCharacter.OnSecondaryFire();
        }

        public void TakeDamage(float damageTaken)
        {
            health -= damageTaken;
        }

        public override NetEntity ToNetEntity()
        {
            NetPlayer netPlayer = new NetPlayer();
            netPlayer.Flags = flags;
            netPlayer.Id = id;
            netPlayer.ParentId = parentId;
            netPlayer.MoveDeltaX = moveDelta.X;
            netPlayer.MoveDeltaY = moveDelta.Y;
            netPlayer.RectX = rect.Position.X;
            netPlayer.RectY = rect.Position.Y;
            netPlayer.RectWidth = rect.Size.X;
            netPlayer.RectHeight = rect.Size.Y;
            netPlayer.CurrentCharacter = currentCharacter;
            netPlayer.Health = health;
            netPlayer.Angle = (float)Math.Atan2(direction.Y, direction.X);
            netPlayer.Type = type;

            return netPlayer;
        }
    }
}
