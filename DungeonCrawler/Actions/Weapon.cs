using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class Weapon
    {
        [Key(0)]
        public readonly int clipSize;
        [Key(1)]
        public int currentAmmo;
        [Key(2)]
        public Action primaryFire;
        [Key(3)]
        public Action secondaryFire;
    }
}
