using DungeonCrawler.Actions;
using DungeonCrawler.Characters;
using MessagePack;
using System;
using System.Collections.Generic;

namespace DungeonCrawler
{
    [Union(0, typeof(Scout))]
    [MessagePackObject]
    public abstract class Character
    {
        //Every Character Class is based on the abstract Class Character. (That's a mouthful.)
        //Characters have an FOV, a move speed, and a max health. These values are only the bases and the actual values will change with gameplay.
        [Key(0)]
        public int parentId;
        [Key(1)]
        public abstract float MaxHealth { get; }
        [Key(2)]
        public abstract float MovementSpeed { get;}
        [Key(3)]
        public abstract float FOV { get;}
        [Key(4)]
        public abstract int CurrentWeapon { get; set; }
        [Key(5)]
        public abstract Weapon[] Weapons { get; set; }

        [Key(6)]
        protected int currentWeapon;
        [Key(7)]
        protected Weapon[] weapons;

        public abstract Actions.Action OnPrimaryFire();
        public abstract Actions.Action OnSecondaryFire();
        public abstract void OnDeath();
    }
}
