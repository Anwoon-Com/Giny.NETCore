using Giny.Core.Extensions;
using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Fights.Challenges
{
    /// <summary>
    /// La cible désignée doit être achevée en premier.
    /// </summary>
    [Challenge(3)]
    [Challenge(117)]
    [Challenge(118)]
    [Challenge(130)]
    [Challenge(132)]
    [Challenge(134)]
    [Challenge(146)]
    [Challenge(157)]
    [Challenge(160)]
    [Challenge(172)]
    [Challenge(177)]
    [Challenge(181)]
    [Challenge(184)]
    [Challenge(185)]
    [Challenge(194)]
    [Challenge(208)]
    [Challenge(218)]
    [Challenge(220)]
    [Challenge(221)]
    [Challenge(223)]
    [Challenge(234)]
    [Challenge(239)]
    [Challenge(244)]
    [Challenge(277)]
    [Challenge(297)]
    [Challenge(300)]
    [Challenge(303)]
    [Challenge(306)]
    [Challenge(315)]
    [Challenge(345)]
    public class Prime : Challenge
    {
        public Prime(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }

        public override double XpBonusRatio => 0.3;

        public override double DropBonusRatio => 0.3;

        private Fighter? Target
        {
            get;
            set;
        }
        public override void BindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters())
            {
                enemy.Death += OnEnemyDie;
            }
        }


        public override void Initialize()
        {
            base.Initialize();

            Target = Team.EnemyTeam.GetFighters<Fighter>().Random(new Random());
            OnTargetUpdated();


        }
        private void OnEnemyDie(Fighter target, Fighter source)
        {
            if (Target == null)
            {
                return;
            }

            if (target == Target)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_COMPLETED);
                Target = null;
                OnTargetUpdated();
            }
            else
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }

        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }

        public override bool IsValid()
        {
            return Team.EnemyTeam.GetFightersCount() > 1;
        }
        public override IEnumerable<Fighter> GetTargets()
        {
            return Target != null ? new Fighter[] { Target } : base.GetTargets();
        }
        public override void UnbindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters())
            {
                enemy.Death -= OnEnemyDie;
            }
        }
    }
}
