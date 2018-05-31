using DungeonCrawler.Actions;
using DungeonCrawler.States;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Characters
{
    [MessagePackObject]
    public class Scout : Character
    {   
        [Key(8)]
        public new Weapon[] weapons = new Weapon[1];

        [Key(9)]
        public override float MaxHealth => 175;
        [Key(10)]
        public override float MovementSpeed => .3f;
        [Key(11)]
        public override float FOV => 100;

        [Key(12)]
        public override int CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        [Key(13)]
        public override Weapon[] Weapons { get => weapons; set => weapons = value; }
        
        [SerializationConstructor]
        public Scout(int playerId)
        {
            this.parentId = playerId;
            weapons[0] = new Weapon();
            weapons[0].primaryFire = new ScoutShotgunAction(true, playerId);
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
