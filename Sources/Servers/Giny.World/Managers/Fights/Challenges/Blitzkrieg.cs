using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Fights.Units;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Blitzkrieg : lorsqu'un ennemi est attaqué, il doit être achevé
     *  avant la fin du tour de l'attaquant.
     */
    [Challenge(38)]
    [Challenge(122)]
    [Challenge(138)]
    [Challenge(144)]
    [Challenge(188)]
    [Challenge(202)]
    [Challenge(228)]
    [Challenge(230)]
    [Challenge(238)]
    [Challenge(242)]
    [Challenge(282)]
    [Challenge(289)]
    [Challenge(313)]
    [Challenge(339)]
    [Challenge(351)]
    public class Blitzkrieg : Challenge
    {
        private HashSet<Fighter> m_attacked = new HashSet<Fighter>();
        public Blitzkrieg(ChallengeRecord record, FightTeam team) : base(record, team)
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
            m_attacked.Clear();
        }
        private void OnTurnEnded(Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter) && m_attacked.Any(x => x.AliveSafe))
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
        private void OnEnemyReceiveDamage(Damage damages, DamageResult result)
        {
            if (AffectedFighters.Contains(damages.Source) && damages.Target.AliveSafe)
            {
                m_attacked.Add(damages.Target);
            }
        }
    }
}
