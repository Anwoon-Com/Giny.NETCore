using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Chacun son monstre : chaque ennemi achevé doit l'être par un combattant
     *  allié différent (aucun allié ne peut en achever deux).
     */
    [Challenge(46)]
    public class EachHisMonster : Challenge
    {
        private HashSet<Fighter> m_killers = new HashSet<Fighter>();
        public EachHisMonster(ChallengeRecord record, FightTeam team) : base(record, team)
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
            if (AffectedFighters.Contains(source) && !m_killers.Add(source))
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
