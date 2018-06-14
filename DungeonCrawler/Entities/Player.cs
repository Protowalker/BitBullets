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
    class Player : Entity
    {
        public Character currentCharacter;

        public override float health { get; set; }

        public override int Id { get => id; set => id = value; }
        public override int ParentId { get => parentId; set => parentId = value; }
        public override float moveSpeed { get; set; }
        public float FOV { get; set; }

        public Player()
        {
            flags = flags | Entity.Flags.PLAYER;
            flags = flags | Entity.Flags.RENDER;

            rect = new RectangleShape(new Vector2f(5,10));
            health = 1;
            

            //rect.Origin = rect.Size / 2;
            type = EntityType.Player;
            if(Game.currentState == State.StateType.ServerState)
            {
                DieEvent += new DieHandler(OnDeath);
            }
        }

        public override void Init()
        {
            id = Entity.highestId + 1;
            highestId = id;
            (Game.states[Game.currentState]).netState.Entities.Add(id, this);
            (Game.states[Game.currentState]).netState.quadTree.Insert(id);
        }

        public void SetCurrentCharacter(Character currentCharacter)
        {
            moveSpeed = currentCharacter.MovementSpeed;
            health = currentCharacter.MaxHealth;
            this.currentCharacter = currentCharacter;
            FOV = currentCharacter.FOV;
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }


        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            NetGameState netState = Game.states[Game.currentState].netState;
            moveDelta *= deltaTime;
            //Deal with movement.
            HandleCollision();

        }

        public Actions.Action OnPrimaryFire()
        {
            return currentCharacter.OnPrimaryFire();
        }

        public Actions.Action OnSecondaryFire()
        {
            return currentCharacter.OnSecondaryFire();
        }

        public void TakeDamage(float damageTaken)
        {
            health -= damageTaken;
            Console.WriteLine("Current health is " + health);
        }

        public void OnDeath(Entity ent)
        {
            health = currentCharacter.MaxHealth;
            rect.Position = new Vector2f(0,0);
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
            netPlayer.MoveSpeed = moveSpeed;
            netPlayer.FOV = FOV;

            return netPlayer;
        }

        public override void HandleCollision()
        {
            NetGameState netState = Game.states[Game.currentState].netState;

            FloatRect overlap;

            List<int> collided = netState.quadTree.GetItems(rect.GetGlobalBounds());
            bool intersecting;
            //Wall collision is tested in movements of .5
            float iterations = (moveDelta.X / .5f);
            int sign = Math.Sign(iterations);

            Entity ent;

            //We do one direction at a time so that the character doesn't get moved in odd places.
            while (moveDelta.X * sign > 0f)
            {
                rect.Position += new Vector2f(Math.Min(.5f * sign, moveDelta.X * sign), 0);
                collided = netState.quadTree.GetItems(rect.GetGlobalBounds());


                foreach (int id in collided)
                {
                    ent = netState.Entities[id];
                    if ((ent.flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = ent.rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap);

                    if (intersecting)
                    {
                        if (ent.rect.GetGlobalBounds().Left + ent.rect.Size.X > overlap.Left + overlap.Width) //if the player is to the right of the shape
                        {
                            rect.Position -= new Vector2f(overlap.Width, 0);
                        }
                        else if (ent.rect.GetGlobalBounds().Left < overlap.Left) //to the left
                        {
                            rect.Position += new Vector2f(overlap.Width, 0);
                        }
                    }
                }
                moveDelta.X -= .5f * sign;
            }

            iterations = (moveDelta.Y / .5f);
            sign = Math.Sign(iterations);

            while (moveDelta.Y * sign > 0f)
            {
                rect.Position += new Vector2f(0, Math.Min(moveDelta.Y * sign, .5f * sign));
                collided = netState.quadTree.GetItems(rect.GetGlobalBounds());

                //Now Y
                foreach (int id in collided)
                {
                    ent = netState.Entities[id];
                    if ((ent.flags & Entity.Flags.WALL) != Entity.Flags.WALL) continue;

                    intersecting = ent.rect.GetGlobalBounds().Intersects(rect.GetGlobalBounds(), out overlap);

                    if (intersecting)
                    {
                        if (ent.rect.GetGlobalBounds().Top < overlap.Top) //above
                        {
                            rect.Position += new Vector2f(0, overlap.Height);

                        }
                        else if (ent.rect.GetGlobalBounds().Top + ent.rect.Size.Y > overlap.Top + overlap.Height) //below
                        {
                            rect.Position -= new Vector2f(0, overlap.Height);
                        }
                    }
                }
                moveDelta.Y -= .5f * sign;
            }
            moveDelta = new Vector2f();
        }
    }
}

