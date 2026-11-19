using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Cast;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Liberte / Circulez ! / Temps qui court : les combattants allies ne doivent
     *  jamais utiliser certains effets (Ta=0,effectId,... dans CompletionCriterion),
     *  typiquement les retraits de PA, PM ou PO.
     */
    [Challenge(182)]
    [Challenge(207)]
    [Challenge(213)]
    [Challenge(219)]
    [Challenge(310)]
    [Challenge(312)]
    [Challenge(324)]
    public class ForbiddenEffect : Challenge
    {
        private HashSet<int> ForbiddenEffects
        {
            get
            {
                return new HashSet<int>(Regex.Matches(Record.CompletionCriterion ?? string.Empty,
                    "Ta=0,([0-9]+)").Cast<Match>().Select(x => int.Parse(x.Groups[1].Value)));
            }
        }
        public ForbiddenEffect(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.20d;
        public override double DropBonusRatio => 0.15d;
        public override void BindEvents()
        {
            foreach (var fighter in AffectedFighters)
            {
                fighter.ActionPerformed += OnActionPerformed;
            }
        }
        public override void UnbindEvents()
        {
            foreach (var fighter in AffectedFighters)
            {
                fighter.ActionPerformed -= OnActionPerformed;
            }
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnActionPerformed(SpellCastHandler handler)
        {
            var forbidden = ForbiddenEffects;

            foreach (var effectHandler in handler.GetEffectHandlers())
            {
                if (forbidden.Contains((int)effectHandler.Effect.EffectEnum))
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                    return;
                }
            }
        }
    }
}
