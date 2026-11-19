using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Partage : chaque combattant allié doit avoir achevé au moins un
     *  combattant ennemi pendant le combat.
     */
    [Challenge(44)]
    public class Sharing : Challenge
    {
        private HashSet<Fighter> m_killers = new HashSet<Fighter>();
        public Sharing(ChallengeRecord record, FightTeam team) : base(record, team)
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
            return Team.GetFighters<Fighter>().Where(x => !x.IsSummoned());
        }
        private void OnEnemyDeath(Fighter target, Fighter source)
        {
            if (AffectedFighters.Contains(source))
            {
                m_killers.Add(source);
            }
            if (!Team.EnemyTeam.GetFighters<Fighter>(false).Any(x => x.AliveSafe))
            {
                if (!AffectedFighters.All(x => m_killers.Contains(x)))
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                }
            }
        }
    }
}
