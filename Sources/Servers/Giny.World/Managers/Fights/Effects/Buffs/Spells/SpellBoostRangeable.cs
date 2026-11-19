using Giny.Protocol.Enums;
using Giny.World.Managers.Effects;
using Giny.World.Managers.Fights.Buffs;
using Giny.World.Managers.Fights.Buffs.SpellBoost;
using Giny.World.Managers.Fights.Cast;
using Giny.World.Managers.Fights.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Fights.Effects.Buffs.Spells
{
    /*
     * Rend la portée d'un sort modifiable (mécanique Pandawa :
     * la portée du sort devient influencée par la statistique Portée).
     */
    [SpellEffectHandler(EffectsEnum.Effect_BoostSpellRangeable)]
    public class SpellBoostRangeable : SpellEffectHandler
    {
        public SpellBoostRangeable(EffectDice effect, SpellCastHandler castHandler) : base(effect, castHandler)
        {

        }

        protected override void Apply(IEnumerable<Fighter> targets)
        {
            short spellId = (short)Effect.Min;
            short delta = (short)Effect.Value;

            foreach (var target in targets)
            {
                int id = target.BuffIdProvider.Pop();
                Buff buff = new SpellBoostRangeableBuff(id, spellId, delta, target, this, Effect.DispellableEnum);
                target.AddBuff(buff);
            }
        }
    }
}
