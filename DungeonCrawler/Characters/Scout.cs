using DungeonCrawler.Actions;
using DungeonCrawler.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Characters
{
    [Serializable]
    class Scout : Character
    {
        public new Weapon[] weapons = new Weapon[1];

        public override float MaxHealth => 175; 
        public override float MovementSpeed => .3f;
        public override float FOV => 100;

        public override int CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        public override Weapon[] Weapons { get => weapons; set => weapons = value; }

        public Scout(int playerId)
        {
            parentId = playerId;
            weapons[0] = new Weapon();
            weapons[0].primaryFire = new ScoutShotgunAction(playerId);
            weapons[0].secondaryFire = new ScoutViewfinderAction(playerId);
        }

        public override void OnPrimaryFire()
        {
            Game.states[Game.currentState].netState.RealTimeActions.Add(weapons[0].primaryFire);
        }

        public override void OnSecondaryFire()
        {
            Game.states[Game.currentState].netState.RealTimeActions.Add(weapons[0].secondaryFire);
        }

        public override void OnDeath()
        {

        }
    }
}
