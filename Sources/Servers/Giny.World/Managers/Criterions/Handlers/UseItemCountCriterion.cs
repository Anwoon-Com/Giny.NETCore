using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// Eu : number of items used by the character (gid 0 = any item).
    /// </summary>
    [CriterionHandler("Eu")]
    public class UseItemCountCriterion : Criterion
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
        public UseItemCountCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            ItemGid = int.Parse(split[0]);
            Count = split.Length > 1 ? int.Parse(split[1]) : 0;
        }
        public override bool Eval(WorldClient client)
        {
            int count = client.Character.GetItemUseCount(ItemGid);
            return count > Count;
        }
    }
}
