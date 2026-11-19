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
    /// Hardi
    /// Les combattants alliés doivent finir leur tour sur une cellule adjacente à celle d'un ennemi.
    /// </summary>
    [Challenge(36)]
    [Challenge(137)]
    [Challenge(143)]
    [Challenge(173)]
    [Challenge(178)]
    [Challenge(186)]
    [Challenge(187)]
    [Challenge(189)]
    [Challenge(215)]
    [Challenge(229)]
    [Challenge(241)]
    [Challenge(248)]
    [Challenge(286)]
    [Challenge(301)]
    [Challenge(309)]
    [Challenge(329)]
    [Challenge(333)]
    [Challenge(346)]
    public class Daring : Challenge
    {
        public Daring(ChallengeRecord record, FightTeam team) : base(record, team)
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
                if (!fighter.GetMeleeFighters().Any(x => x != fighter && !x.IsFriendlyWith(fighter)))
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
