using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Deux pour le prix d'un : a chaque tour, un combattant allie qui inflige
     *  des degats doit toucher au moins deux ennemis differents.
     */
    [Challenge(42)]
    public class TwoForOne : Challenge
    {
        private HashSet<Fighter> m_targets = new HashSet<Fighter>();
        public TwoForOne(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
            Fight.TurnEnded += OnTurnEnded;
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.DamageReceived += OnEnemyReceiveDamage;
            }
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
            Fight.TurnEnded -= OnTurnEnded;
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
        private void OnTurnEnded(Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter) && m_targets.Count == 1)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
        private void OnEnemyReceiveDamage(Damage damages, DamageResult result)
        {
            if (AffectedFighters.Contains(damages.Source))
            {
                m_targets.Add(damages.Target);
            }
        }
    }
}
