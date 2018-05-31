using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [Union(0, typeof(CompositeAction))]
    [MessagePackObject]
    public abstract class Action
    {
        public abstract void Update(float elapsed);

        [Key(0)]
        public bool finished;
    }
}
