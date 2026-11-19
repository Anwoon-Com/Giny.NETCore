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
    /// EI : use an item (consumable, key...).
    /// </summary>
    [CriterionHandler("EI")]
    public class UseItemCriterion : Criterion
    {
        public short ItemGid
        {
            get;
            set;
        }
        public UseItemCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            ItemGid = short.Parse(split[0]);
        }
        public override bool Eval(WorldClient client)
        {
            return client.Character.LastUsedItemGid == ItemGid;
        }
    }
}
