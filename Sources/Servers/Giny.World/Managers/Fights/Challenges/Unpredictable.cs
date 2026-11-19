using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Imprevisible : chaque tour, toutes les attaques d'un combattant allie
     *  doivent etre concentrees sur un meme ennemi.
     */
    [Challenge(34)]
    public class Unpredictable : Challenge
    {
        private HashSet<Fighter> m_targets = new HashSet<Fighter>();
        public Unpredictable(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.DamageReceived += OnEnemyReceiveDamage;
            }
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.DamageReceived -= OnEnemyReceiveDamage;
            }
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnTurnStarted(Fight fight, Fighter fighter)
        {
            m_targets.Clear();
        }
        private void OnEnemyReceiveDamage(Damage damages, DamageResult result)
        {
            if (!AffectedFighters.Contains(damages.Source))
            {
                return;
            }
            m_targets.Add(damages.Target);
            if (m_targets.Count > 1)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
