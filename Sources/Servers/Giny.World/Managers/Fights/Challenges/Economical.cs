using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Econome : les combattants alliés ne doivent utiliser qu'un seul
     *  point d'action par tour.
     */
    [Challenge(5)]
    [Challenge(158)]
    public class Economical : Challenge
    {
        private Dictionary<long, int> m_apAtStart = new Dictionary<long, int>();

        public Economical(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
            Fight.TurnEnded += OnTurnEnded;
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
            Fight.TurnEnded -= OnTurnEnded;
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnTurnStarted(Fight fight, Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter))
            {
                m_apAtStart[fighter.Id] = fighter.Stats.ActionPoints.TotalInContext();
            }
        }
        private void OnTurnEnded(Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter) && m_apAtStart.ContainsKey(fighter.Id))
            {
                int used = (m_apAtStart[fighter.Id] - fighter.Stats.ActionPoints.TotalInContext());

                if (used > 1)
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                }
            }
        }
    }
}
