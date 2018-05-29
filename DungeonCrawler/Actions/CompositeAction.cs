using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    abstract class CompositeAction : Action
    {
        protected List<Action> actionList = new List<Action>();
        public void Add(Action action)
        {
            actionList.Add(action);
        }
    }
}
