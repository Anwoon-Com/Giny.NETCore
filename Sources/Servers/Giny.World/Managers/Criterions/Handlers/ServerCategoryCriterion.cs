using Giny.World.Managers.Criterias;
using Giny.World.Network;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// SC : server category filter used by official achievement objectives.
    /// In client data, SC=5 (classic game server) is used everywhere while
    /// SC=6/7 mark event-server-only content (e.g. Temporis). On a classic
    /// server SC=5 is the only passing value.
    /// </summary>
    [CriterionHandler("SC")]
    public class ServerCategoryCriterion : Criterion
    {
        public const short ClassicServerCategory = 5;

        public short Category
        {
            get;
            set;
        }
        public ServerCategoryCriterion(string criteriaFull) : base(criteriaFull)
        {
            Category = short.Parse(Value);
        }
        public override bool Eval(WorldClient client)
        {
            return Category == ClassicServerCategory;
        }
    }
}
