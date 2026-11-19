using Giny.World.Managers.Criterias;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// ES : character enters a given map or subarea.
    /// </summary>
    [CriterionHandler("ES")]
    public class VisitMapCriterion : Criterion
    {
        public long Id
        {
            get;
            set;
        }
        public VisitMapCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            Id = long.Parse(split[0]);
        }
        public override bool Eval(WorldClient client)
        {
            var map = client.Character.Map;

            if (map == null)
            {
                return false;
            }
            return map.Id == Id || map.SubareaId == Id;
        }
    }
}
