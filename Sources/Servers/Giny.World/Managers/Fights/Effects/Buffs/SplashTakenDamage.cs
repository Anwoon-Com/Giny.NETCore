using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Effects;
using Giny.World.Managers.Fights.Buffs;
using Giny.World.Managers.Fights.Cast;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Triggers;
using Giny.World.Managers.Fights.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Fights.Effects.Buffs
{
    /*
     * Effets "Splash" : une part des dommages subis par la cible
     * est renvoyée aux entités situées dans la zone autour d'elle.
     */
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenDamageNeutral)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenDamageAir)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenDamageFire)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenDamageWater)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenEarth)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageNeutral)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageAir)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageFire)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageWater)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageEarth)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashRawTakenDamageBestElement)]
    [SpellEffectHandler(EffectsEnum.Effect_SplashFinalTakenDamageBestElement)]
    public class SplashTakenDamage : SpellEffectHandler
    {
        public SplashTakenDamage(EffectDice effect, SpellCastHandler castHandler) : base(effect, castHandler)
        {
        }

        private bool IsRaw
        {
            get
            {
                switch (Effect.EffectEnum)
                {
                    case EffectsEnum.Effect_SplashRawTakenDamageNeutral:
                    case EffectsEnum.Effect_SplashRawTakenDamageAir:
                    case EffectsEnum.Effect_SplashRawTakenDamageFire:
                    case EffectsEnum.Effect_SplashRawTakenDamageWater:
                    case EffectsEnum.Effect_SplashRawTakenEarth:
                    case EffectsEnum.Effect_SplashRawTakenDamageBestElement:
                        return true;
                }
                return false;
            }
        }

        private EffectElementEnum GetElement(Fighter source)
        {
            switch (Effect.EffectEnum)
            {
                case EffectsEnum.Effect_SplashRawTakenDamageNeutral:
                case EffectsEnum.Effect_SplashFinalTakenDamageNeutral:
                    return EffectElementEnum.Neutral;
                case EffectsEnum.Effect_SplashRawTakenDamageAir:
                case EffectsEnum.Effect_SplashFinalTakenDamageAir:
                    return EffectElementEnum.Air;
                case EffectsEnum.Effect_SplashRawTakenDamageFire:
                case EffectsEnum.Effect_SplashFinalTakenDamageFire:
                    return EffectElementEnum.Fire;
                case EffectsEnum.Effect_SplashRawTakenDamageWater:
                case EffectsEnum.Effect_SplashFinalTakenDamageWater:
                    return EffectElementEnum.Water;
                case EffectsEnum.Effect_SplashRawTakenEarth:
                case EffectsEnum.Effect_SplashFinalTakenDamageEarth:
                    return EffectElementEnum.Earth;
                default:
                    return source.Stats.GetBestElement();
            }
        }

        protected override void Apply(IEnumerable<Fighter> targets)
        {
            int percent = Effect.Min;

            foreach (var target in targets)
            {
                int id = target.BuffIdProvider.Pop();

                TriggerBuff buff = new TriggerBuff(id, new Trigger[] { new Trigger(TriggerTypeEnum.OnDamaged) },
                    (triggerBuff, token) => OnTargetDamaged(triggerBuff, token, percent), null, 0, target, this, Effect.DispellableEnum);

                target.AddBuff(buff);
            }
        }

        private bool OnTargetDamaged(TriggerBuff buff, ITriggerToken token, int percent)
        {
            Damage damage = token as Damage;

            if (damage == null || !buff.Target.AliveSafe)
            {
                return false;
            }

            double baseAmount = IsRaw ? (damage.BaseMinDamages + damage.BaseMaxDamages) / 2d : (damage.Computed ?? 0);
            int amount = (int)(baseAmount * percent / 100d);

            if (amount <= 0)
            {
                return false;
            }

            var cells = Zone.GetCells(buff.Target.Cell, buff.Target.Cell, buff.Target.Fight.Map);

            foreach (var cell in cells)
            {
                Fighter target = buff.Target.Fight.GetFighter(cell.Id);

                if (target == null || target == buff.Target || !target.AliveSafe)
                {
                    continue;
                }

                target.InflictDamage(new Damage(damage.Source, target, GetElement(damage.Source), amount, amount, this)
                {
                    WontTriggerBuffs = true,
                });
            }

            return false;
        }
    }
}
