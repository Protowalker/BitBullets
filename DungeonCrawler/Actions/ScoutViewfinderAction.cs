using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawler.Actions
{
    [MessagePackObject]
    public class ScoutViewfinderAction : Action
    {
        [SerializationConstructor]
        public ScoutViewfinderAction(bool finished, int id) : base(id)
        {
            this.finished = finished;
            this.id = id;
        }

        public override void Update(float elapsed)
        {
        }
    }
}
