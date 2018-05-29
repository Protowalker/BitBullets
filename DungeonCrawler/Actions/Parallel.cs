using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    class Parallel : CompositeAction
    {
        public override void Update(float elapsed)
        {
            //Do all actions in a single iteration, VS sequence which is one at a time.
            actionList.ForEach(a => a.Update(elapsed));
            actionList.RemoveAll(a => a.finished);
            finished = actionList.Count == 0;
        }
    }
}
