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
     *  Elémentaire : les combattants alliés ne doivent infliger des dégâts
     *  que d'un seul élément.
     */
    [Challenge(20)]
    public class Elemental : Challenge
    {
        private EffectElementEnum? m_element;
        public Elemental(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.DamageReceived += OnEnemyReceiveDamage;
            }
        }
        public override void UnbindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters<Fighter>(false))
            {
                enemy.DamageReceived -= OnEnemyReceiveDamage;
            }
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnEnemyReceiveDamage(Damage damages, DamageResult result)
        {
            if (!AffectedFighters.Contains(damages.Source))
            {
                return;
            }
            if (!m_element.HasValue)
            {
                m_element = damages.Element;
            }
            else if (damages.Element != m_element.Value)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
