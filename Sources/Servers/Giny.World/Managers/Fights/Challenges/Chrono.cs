using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Chrono : vaincre tous les monstres en moins de X tours
     *  (ST<n dans CompletionCriterion).
     */
    [Challenge(250)]
    [Challenge(251)]
    [Challenge(252)]
    [Challenge(272)]
    [Challenge(276)]
    [Challenge(280)]
    [Challenge(284)]
    [Challenge(328)]
    public class Chrono : Challenge
    {
        private int MaxTurns
        {
            get
            {
                var m = Regex.Match(Record.CompletionCriterion ?? string.Empty, "ST<([0-9]+)");
                return m.Success ? int.Parse(m.Groups[1].Value) : 20;
            }
        }
        public Chrono(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.20d;
        public override double DropBonusRatio => 0.15d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnTurnStarted(Fight fight, Fighter fighter)
        {
            if (Fight.RoundNumber >= MaxTurns)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
