using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// Ea : number of achievements finished by the character.
    /// </summary>
    [CriterionHandler("Ea")]
    public class AchievementCountCriterion : Criterion
    {
        public AchievementCountCriterion(string criteriaFull) : base(criteriaFull)
        {
        }
        public override bool Eval(WorldClient client)
        {
            return ArithmeticEval(client.Character.GetFinishedAchievements().Count());
        }
    }
}
