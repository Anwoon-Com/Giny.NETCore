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
    /// EB : own a mount (dragodinde certificate item).
    /// </summary>
    [CriterionHandler("EB")]
    public class HasMountCriterion : Criterion
    {
        public short MountItemGid
        {
            get;
            set;
        }
        public int Count
        {
            get;
            set;
        }
        public HasMountCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            MountItemGid = short.Parse(split[0]);
            Count = split.Length > 1 ? int.Parse(split[1]) : 0;
        }
        public override bool Eval(WorldClient client)
        {
            int owned = client.Character.Inventory.GetItems().Where(x => x.GId == MountItemGid).Sum(x => x.Quantity);
            return owned > Count;
        }
    }
}
