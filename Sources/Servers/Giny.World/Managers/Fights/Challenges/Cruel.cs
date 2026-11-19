using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /// <summary>
    /// Les ennemis doivent être achevés dans l'ordre croissant de leurs points de vie.
    /// </summary>
    [Challenge(10)]
    public class Cruel : Challenge
    {
        public override double XpBonusRatio => 0.5;

        public override double DropBonusRatio => 0.5;

        private List<Fighter> KillOrder
        {
            get;
            set;
        }
        private int KillIndex
        {
            get;
            set;
        }

        public Cruel(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            KillOrder = Team.EnemyTeam.GetFighters()
                .OrderBy(x => x.Stats.Life.TotalInContext()).ToList();
        }

        public override void BindEvents()
        {
            foreach (var enemy in Team.EnemyTeam.GetFighters())
            {
                enemy.Death += OnEnemyDie;
            }
        }

        private void OnEnemyDie(Fighter target, Fighter source)
        {
            if (KillIndex < KillOrder.Count && target != KillOrder[KillIndex])
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }

            KillIndex++;

            if (KillIndex >= KillOrder.Count)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_COMPLETED);
            }
        }

        public override bool IsValid()
        {
            return Team.EnemyTeam.GetFightersCount() > 1;
        }


        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
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
