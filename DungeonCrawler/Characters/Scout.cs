using DungeonCrawler.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Characters
{
    class Scout : Character
    {
        public new Weapon[] weapons = new Weapon[1];

        public override float MaxHealth => 175; 
        public override float MovementSpeed => .3f;
        public override float FOV => 100;

        public override int CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        public override Weapon[] Weapons { get => weapons; set => weapons = value; }

        public Scout(Player player)
        {
            parentId = player.Id;
            weapons[0] = new Weapon();
            weapons[0].primaryFire = new ScoutShotgunAction(player.Id);
            weapons[0].secondaryFire = new ScoutViewfinderAction(player.Id);
        }

        public override void OnPrimaryFire()
        {
            World.RealTimeActions.Add(weapons[0].primaryFire);
        }

        public override void OnSecondaryFire()
        {
            World.RealTimeActions.Add(weapons[0].secondaryFire);
        }

        public override void OnDeath()
        {

        }
    }
}
