using DungeonCrawler.Actions;
using System.Collections.Generic;

namespace DungeonCrawler
{
    abstract class Character
    {
        //Every Character Class is based on the abstract Class Character. (That's a mouthful.)
        //Characters have an FOV, a move speed, and a max health. These values are only the bases and the actual values will change with gameplay.
        public abstract float MaxHealth { get; }
        public abstract float MovementSpeed { get;}
        public abstract float FOV { get;}
        public abstract int CurrentWeapon { get; set; }
        public abstract Weapon[] Weapons { get; set; }

        protected int currentWeapon;
        protected Weapon[] weapons;

        public int parentId;

        public abstract void OnPrimaryFire();
        public abstract void OnSecondaryFire();
        public abstract void OnDeath();
    }
}
