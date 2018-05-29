using DungeonCrawler.Actions;
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
    class Player : Entity
    {
        public Character currentCharacter;

        float health;

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
            World.Entities.Add(id, this);
            ((InGameState)Game.currentState).quadTree.Insert(id);
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

                List<int> collided = ((InGameState)Game.currentState).quadTree.GetItems(rect.GetGlobalBounds());
                bool intersecting;

                foreach (int id in collided)
                {
                    if ((World.Entities[id].flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = World.Entities[id].rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap);

                    Console.WriteLine("X: " + overlap);

                    if (intersecting)
                    {
                        if (World.Entities[id].rect.GetGlobalBounds().Left + World.Entities[id].rect.Size.X > overlap.Left + overlap.Width) //if the player is to the right of the shape
                        {
                            rect.Position -= new Vector2f(overlap.Width, 0);
                        }
                        else if (World.Entities[id].rect.GetGlobalBounds().Left < overlap.Left) //to the left
                        {
                            rect.Position += new Vector2f(overlap.Width, 0);
                        }
                    }
                }


                rect.Position += new Vector2f(0, moveDelta.Y * deltaTime);
                collided = ((InGameState)Game.currentState).quadTree.GetItems(rect.GetGlobalBounds());

                //Now Y
                foreach (int id in collided)
                {
                    if ((World.Entities[id].flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = World.Entities[id].rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap);

                    Console.WriteLine("Y: " + overlap);

                    if (intersecting)
                    {
                        if (World.Entities[id].rect.GetGlobalBounds().Top < overlap.Top) //above
                        {
                            rect.Position += new Vector2f(0, overlap.Height);

                        }
                        else if (World.Entities[id].rect.GetGlobalBounds().Top + World.Entities[id].rect.Size.Y > overlap.Top + overlap.Height) //below
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
    }
}
