using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;

namespace Giny.World.Managers.Fights.Challenges
{
    /// <summary>
    /// Aucune créature ne doit subir de dégâts directs de la part d'un personnage.
    /// </summary>
    [Challenge(19)]
    [Challenge(124)]
    [Challenge(140)]
    [Challenge(142)]
    [Challenge(176)]
    [Challenge(192)]
    [Challenge(193)]
    [Challenge(274)]
    [Challenge(288)]
    [Challenge(292)]
    public class CleanHands : Challenge
    {
        public override double XpBonusRatio => 0.4;

        public override double DropBonusRatio => 0.4;

        public CleanHands(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }

        public override void BindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters())
            {
                enemy.DamageReceived += OnDamageReceived;
            }
        }

        private void OnDamageReceived(Damage damage, DamageResult result)
        {
            if (damage.Source is CharacterFighter && damage.Source.IsFriendlyWith(Team.Leader))
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }


        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        public override void UnbindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters())
            {
                enemy.DamageReceived -= OnDamageReceived;
            }
        }
    }
}
