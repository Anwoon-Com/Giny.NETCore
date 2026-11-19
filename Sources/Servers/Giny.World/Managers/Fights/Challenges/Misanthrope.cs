using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Misanthrope : les combattants alliés ne doivent jamais terminer leur tour
     *  sur une cellule adjacente à celle d'un allié.
     */
    [Challenge(39)]
    [Challenge(119)]
    [Challenge(127)]
    [Challenge(131)]
    [Challenge(141)]
    [Challenge(156)]
    [Challenge(191)]
    [Challenge(262)]
    [Challenge(319)]
    [Challenge(338)]
    public class Misanthrope : Challenge
    {
        public Misanthrope(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.10d;
        public override double DropBonusRatio => 0.10d;
        public override void BindEvents()
        {
            Fight.TurnEnded += OnTurnEnded;
        }
        public override void UnbindEvents()
        {
            Fight.TurnEnded -= OnTurnEnded;
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<Fighter>();
        }
        private void OnTurnEnded(Fighter fighter)
        {
            if (AffectedFighters.Contains(fighter))
            {
                foreach (var ally in fighter.Team.GetFighters<Fighter>(false))
                {
                    if (ally != fighter && fighter.Cell.Point.ManhattanDistanceTo(ally.Cell.Point) == 1)
                    {
                        OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
                        return;
                    }
                }
            }
        }
    }
}
