using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    class Weapon
    {
        public readonly int clipSize;
        public int currentAmmo;

        public Action primaryFire;
        public Action secondaryFire;
    }
}
