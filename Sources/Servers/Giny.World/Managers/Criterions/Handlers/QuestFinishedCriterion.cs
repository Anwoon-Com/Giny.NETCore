using Giny.Core.Network;
using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// Qf : quest finished.
    /// </summary>
    [CriterionHandler("Qf")]
    public class QuestFinishedCriterion : Criterion
    {
        public short QuestId
        {
            get;
            set;
        }
        public QuestFinishedCriterion(string criteriaFull) : base(criteriaFull)
        {
            QuestId = short.Parse(Value);
        }
        public override bool Eval(WorldClient client)
        {
            var quest = client.Character.GetQuest(QuestId);
            bool done = quest != null && quest.Finished();
            return Operator == CriterionComparaisonOperator.Negation ? !done : done;
        }
    }
}
