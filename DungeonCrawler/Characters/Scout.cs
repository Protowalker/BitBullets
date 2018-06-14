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
        public override float MovementSpeed => .1f;
        [Key(11)]
        public override float FOV => .3f;

        [Key(12)]
        public override int CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        [Key(13)]
        public override Weapon[] Weapons { get => weapons; set => weapons = value; }
        
        [SerializationConstructor]
        public Scout(int playerId)
        {
            this.parentId = playerId;
            weapons[0] = new Weapon();
        }

        public override Actions.Action OnPrimaryFire()
        {
            return new ScoutShotgunAction(parentId);
        }

        public override Actions.Action OnSecondaryFire()
        {
            return new ScoutViewfinderAction(parentId, false);
        }

        public override void OnDeath()
        {

        }
    }
}
