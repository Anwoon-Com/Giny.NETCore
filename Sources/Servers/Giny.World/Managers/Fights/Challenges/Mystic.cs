using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Mystique des éléments : durant leur tour, les combattants alliés ne doivent
     *  utiliser que des éléments différents (un même élément ne peut être employé
     *  deux fois dans le même tour).
     */
    [Challenge(11)]
    [Challenge(225)]
    [Challenge(296)]
    public class Mystic : Challenge
    {
        private HashSet<EffectElementEnum> m_turnElements = new HashSet<EffectElementEnum>();
        public Mystic(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.20d;
        public override double DropBonusRatio => 0.15d;
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
            m_turnElements.Clear();
        }
        private void OnEnemyReceiveDamage(Damage damages, DamageResult result)
        {
            if (!AffectedFighters.Contains(damages.Source))
            {
                return;
            }
            if (!m_turnElements.Add(damages.Element))
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
