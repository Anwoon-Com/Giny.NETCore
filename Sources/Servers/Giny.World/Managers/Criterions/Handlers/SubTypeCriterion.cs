using Giny.World.Managers.Criterias;
using Giny.World.Network;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// ST : content subtype filter paired with SC in official achievement
    /// objectives (ST=2 and ST=3 on classic content, ST=7 on event content).
    /// </summary>
    [CriterionHandler("ST")]
    public class SubTypeCriterion : Criterion
    {
        public static readonly short[] ClassicSubTypes = new short[] { 2, 3 };

        public short SubType
        {
            get;
            set;
        }
        public SubTypeCriterion(string criteriaFull) : base(criteriaFull)
        {
            SubType = short.Parse(Value);
        }
        public override bool Eval(WorldClient client)
        {
            return System.Linq.Enumerable.Contains(ClassicSubTypes, SubType);
        }
    }
}
