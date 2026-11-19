using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Les petits d'abord : le premier ennemi achevé doit être celui
     *  possédant le plus faible niveau.
     */
    [Challenge(30)]
    public class LittleOnesFirst : Challenge
    {
        private bool m_firstKill = true;
        public LittleOnesFirst(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
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
            if (m_firstKill)
            {
                m_firstKill = false;
                var lowest = Team.EnemyTeam.GetFighters<Fighter>(false).Min(x => x.Level);

                if (target.Level > lowest)
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                }
            }
        }
    }
}
