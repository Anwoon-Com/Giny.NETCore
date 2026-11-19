using Giny.World.Managers.Criterias;
using Giny.Core.IO.Configuration;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// Wo : target world id. 0 means any world.
    /// </summary>
    [CriterionHandler("Wo")]
    public class WorldCriterion : Criterion
    {
        public WorldCriterion(string criteriaFull) : base(criteriaFull)
        {
        }
        public override bool Eval(WorldClient client)
        {
            int worldId = 0;
            int.TryParse(Value, out worldId);

            return worldId == 0 || worldId == ConfigManager<WorldConfig>.Instance.ServerId;
        }
    }
}
