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
    /// EA : total achievement points of the character.
    /// </summary>
    [CriterionHandler("EA")]
    public class AchievementPointsCountCriterion : Criterion
    {
        public AchievementPointsCountCriterion(string criteriaFull) : base(criteriaFull)
        {
        }
        public override bool Eval(WorldClient client)
        {
            return ArithmeticEval((int)client.Character.AchievementPoints);
        }
    }
}
