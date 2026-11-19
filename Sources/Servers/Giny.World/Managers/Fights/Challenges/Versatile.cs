using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Cast;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Versatile : les combattants alliés ne doivent utiliser qu'une seule
     *  action (sort ou arme) par tour.
     */
    [Challenge(6)]
    [Challenge(120)]
    [Challenge(126)]
    [Challenge(136)]
    [Challenge(174)]
    [Challenge(179)]
    [Challenge(200)]
    [Challenge(237)]
    [Challenge(263)]
    public class Versatile : Challenge
    {
        private int m_actionCount = 0;
        public Versatile(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
            foreach (var fighter in AffectedFighters)
            {
                fighter.ActionPerformed += OnActionPerformed;
            }
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
            foreach (var fighter in AffectedFighters)
            {
                fighter.ActionPerformed -= OnActionPerformed;
            }
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnTurnStarted(Fight fight, Fighter fighter)
        {
            m_actionCount = 0;
        }
        private void OnActionPerformed(SpellCastHandler handler)
        {
            if (AffectedFighters.Contains(handler.Cast.Source) && ++m_actionCount > 1)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
