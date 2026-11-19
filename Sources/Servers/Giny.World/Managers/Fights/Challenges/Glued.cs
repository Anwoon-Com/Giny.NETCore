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
    /// Collant
    /// Les combattants alliés doivent finir leur tour sur une cellule adjacente à celle d'un autre combattant allié.
    /// </summary>
    [Challenge(37)]
    [Challenge(116)]
    [Challenge(162)]
    [Challenge(164)]
    [Challenge(166)]
    [Challenge(168)]
    [Challenge(183)]
    [Challenge(212)]
    [Challenge(231)]
    [Challenge(240)]
    [Challenge(265)]
    [Challenge(278)]
    [Challenge(298)]
    [Challenge(316)]
    [Challenge(336)]
    [Challenge(342)]
    public class Glued : Challenge
    {
        public Glued(ChallengeRecord record, FightTeam team) : base(record, team)
        {

        }

        public override double XpBonusRatio => 0.50d;

        public override double DropBonusRatio => 0.20d;

        public override void BindEvents()
        {
            Fight.TurnEnded += OnTurnEnded;
        }
        public override void UnbindEvents()
        {
            Fight.TurnEnded -= OnTurnEnded;
        }

        private void OnTurnEnded(Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter))
            {
                if (!fighter.GetMeleeFighters().Any(x => x != fighter && x.IsFriendlyWith(fighter)))
                {
                    OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                }
            }
        }
        public override bool IsValid()
        {
            return Team.GetFightersCount() > 1;
        }

        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
    }
}
