using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;

namespace Giny.World.Managers.Fights.Challenges
{
    /// <summary>
    /// Chaque ennemi ne doit être attaqué que par un seul personnage allié.
    /// </summary>
    [Challenge(45)]
    [Challenge(147)]
    [Challenge(149)]
    [Challenge(151)]
    [Challenge(153)]
    [Challenge(161)]
    [Challenge(163)]
    [Challenge(165)]
    [Challenge(167)]
    [Challenge(190)]
    public class Duel : Challenge
    {
        public override double XpBonusRatio => 0.3;

        public override double DropBonusRatio => 0.3;

        private Dictionary<Fighter, Fighter> Attackers
        {
            get;
            set;
        } = new Dictionary<Fighter, Fighter>();

        public Duel(ChallengeRecord record, FightTeam team) : base(record, team)
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
            var target = damage.Target;
            var source = damage.Source;

            if (source == null || !source.IsFriendlyWith(Team.Leader))
                return;

            if (Attackers.TryGetValue(target, out var attacker))
            {
                if (attacker != source)
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                }
            }
            else
            {
                Attackers.Add(target, source);
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
