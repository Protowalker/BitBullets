using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class SequenceAction : CompositeAction
    {
        public override void Update(float elapsed)
        {
            //Do a single action in the list until it's finished, VS Parallel which does an iteration for all actions
            if (actionList.Count > 0)
            {
                actionList[0].Update(elapsed);
                if (actionList[0].finished)
                {
                    actionList.RemoveAt(0);
                }
            }
            finished = actionList.Count == 0;
        }
    }
}
