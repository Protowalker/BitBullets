using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [Serializable]
    abstract class Action
    {
        public abstract void Update(float elapsed);
        public bool finished;
    }
}
