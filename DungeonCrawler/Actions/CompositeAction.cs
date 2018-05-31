using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [Union(1, typeof(ParallelAction))]
    [Union(2, typeof(SequenceAction))]
    [MessagePackObject]
    public abstract class CompositeAction : Action
    {
        [Key(1)]
        public List<Action> actionList = new List<Action>();
        public void Add(Action action)
        {
            actionList.Add(action);
        }
    }
}
