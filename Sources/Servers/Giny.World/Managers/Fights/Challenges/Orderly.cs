using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Ordonné : les ennemis doivent être achevés dans l'ordre croissant
     *  de leur niveau.
     */
    [Challenge(25)]
    public class Orderly : Challenge
    {
        private long m_lastKilledLevel = 0;
        public Orderly(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.20d;
        public override double DropBonusRatio => 0.15d;
        public override void BindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.Death += OnEnemyDeath;
            }
        }
        public override void UnbindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.Death -= OnEnemyDeath;
            }
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnEnemyDeath(Fighter target, Fighter source)
        {
            if (!AffectedFighters.Contains(source))
            {
                return;
            }
            if (target.Level < m_lastKilledLevel)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
            m_lastKilledLevel = target.Level;
        }
    }
}
