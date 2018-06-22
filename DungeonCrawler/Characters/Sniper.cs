using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler.Actions;
using MessagePack;

namespace DungeonCrawler.Characters
{
    [MessagePackObject]
    public class Sniper : Character
    {
        [Key(8)]
        public override float MaxHealth => 250;

        [Key(9)]
        public override float MovementSpeed => .07f;

        [Key(10)]
        public override float FOV => .5f;

        [Key(11)]
        public override int CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        [Key(12)]
        public override Weapon[] Weapons { get => weapons; set => weapons = value; }

        public Sniper(int id)
        {
            parentId = id;
            weapons = new Weapon[1];
        }

        public override void OnDeath()
        {
            throw new NotImplementedException();
        }

        public override Actions.Action OnPrimaryFire()
        {
            return new SniperRifleAction(parentId);
        }

        public override Actions.Action OnSecondaryFire()
        {
            throw new NotImplementedException();
        }
    }
}
