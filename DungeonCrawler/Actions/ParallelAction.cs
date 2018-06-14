using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class ParallelAction : CompositeAction
    {
        public override void Update(float elapsed)
        {
            //Do all actions in a single iteration, VS sequence which is one at a time.
            actionList.ForEach(a => a.Update(elapsed));
            actionList.FindAll(a => a.finished).ForEach(a => a.CleanUp());
            actionList.RemoveAll(a => a.finished);
            finished = actionList.Count == 0;
        }

        public ParallelAction()
        {
        }

        [SerializationConstructor]
        public ParallelAction(bool finished, List<Action> actionList)
        {
            this.finished = finished;
            this.actionList = actionList;
        }
    }
}
