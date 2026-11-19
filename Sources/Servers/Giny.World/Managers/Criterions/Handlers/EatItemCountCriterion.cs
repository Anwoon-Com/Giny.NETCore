using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// Ee : number of edible items eaten by the character (gid 0 = any edible).
    /// </summary>
    [CriterionHandler("Ee")]
    public class EatItemCountCriterion : Criterion
    {
        public int ItemGid
        {
            get;
            set;
        }
        public int Count
        {
            get;
            set;
        }
        public EatItemCountCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            ItemGid = int.Parse(split[0]);
            Count = split.Length > 1 ? int.Parse(split[1]) : 0;
        }
        public override bool Eval(WorldClient client)
        {
            int count = client.Character.GetEdibleUseCount(ItemGid);
            return count > Count;
        }
    }
}
