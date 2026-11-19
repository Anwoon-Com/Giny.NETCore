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
    /// OA : another achievement is already finished.
    /// </summary>
    [CriterionHandler("OA")]
    public class AchievementDoneCriterion : Criterion
    {
        public short AchievementId
        {
            get;
            set;
        }
        public AchievementDoneCriterion(string criteriaFull) : base(criteriaFull)
        {
            AchievementId = short.Parse(Value);
        }
        public override bool Eval(WorldClient client)
        {
            bool done = client.Character.GetFinishedAchievements().Any(x => x.AchievementId == AchievementId);
            return Operator == CriterionComparaisonOperator.Negation ? !done : done;
        }
    }
}
