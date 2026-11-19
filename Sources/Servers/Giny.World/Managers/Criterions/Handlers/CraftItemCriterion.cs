using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// EC : character crafts a given item (gid 0 = any), in a quantity greater than the count.
    /// </summary>
    [CriterionHandler("EC")]
    public class CraftItemCriterion : Criterion
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
        public CraftItemCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            ItemGid = int.Parse(split[0]);
            Count = split.Length > 1 ? int.Parse(split[1]) : 0;
        }
        public override bool Eval(WorldClient client)
        {
            if (ItemGid != 0 && client.Character.LastCraftedGid != ItemGid)
            {
                return false;
            }
            return client.Character.LastCraftedCount > Count;
        }
    }
}
