using Giny.Protocol.Enums;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Challenges;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Giny.World.Managers.Fights.Challenges
{
    /*
     *  Duo / Trio : vaincre avec un groupe limite (GN<x dans ActivationCriterion)
     *  et en moins d'un nombre de tours donne (ST<n dans CompletionCriterion).
     */
    [Challenge(52)]
    [Challenge(53)]
    [Challenge(54)]
    [Challenge(55)]
    [Challenge(56)]
    [Challenge(57)]
    [Challenge(58)]
    [Challenge(59)]
    [Challenge(60)]
    [Challenge(61)]
    [Challenge(62)]
    [Challenge(63)]
    [Challenge(64)]
    [Challenge(65)]
    [Challenge(66)]
    [Challenge(67)]
    [Challenge(68)]
    [Challenge(69)]
    [Challenge(73)]
    [Challenge(74)]
    [Challenge(75)]
    [Challenge(76)]
    [Challenge(77)]
    [Challenge(78)]
    [Challenge(79)]
    [Challenge(80)]
    [Challenge(81)]
    [Challenge(82)]
    [Challenge(83)]
    [Challenge(84)]
    [Challenge(85)]
    [Challenge(86)]
    [Challenge(87)]
    [Challenge(88)]
    [Challenge(89)]
    [Challenge(90)]
    [Challenge(91)]
    [Challenge(92)]
    [Challenge(93)]
    [Challenge(94)]
    [Challenge(95)]
    [Challenge(96)]
    [Challenge(97)]
    [Challenge(98)]
    [Challenge(99)]
    [Challenge(101)]
    [Challenge(102)]
    [Challenge(103)]
    [Challenge(104)]
    [Challenge(105)]
    [Challenge(106)]
    [Challenge(107)]
    [Challenge(108)]
    [Challenge(109)]
    [Challenge(110)]
    [Challenge(111)]
    [Challenge(112)]
    [Challenge(113)]
    [Challenge(114)]
    [Challenge(243)]
    [Challenge(246)]
    [Challenge(249)]
    [Challenge(253)]
    [Challenge(254)]
    [Challenge(255)]
    [Challenge(256)]
    [Challenge(257)]
    [Challenge(275)]
    [Challenge(279)]
    [Challenge(283)]
    [Challenge(287)]
    [Challenge(290)]
    [Challenge(293)]
    [Challenge(294)]
    [Challenge(299)]
    [Challenge(302)]
    [Challenge(305)]
    [Challenge(311)]
    [Challenge(314)]
    [Challenge(320)]
    [Challenge(323)]
    [Challenge(327)]
    [Challenge(331)]
    [Challenge(337)]
    [Challenge(340)]
    [Challenge(341)]
    [Challenge(344)]
    [Challenge(347)]
    [Challenge(350)]
    [Challenge(353)]
    [Challenge(308)]
    [Challenge(317)]
    [Challenge(334)]
    public class GroupSize : Challenge
    {
        private int MaxFighters
        {
            get
            {
                var m = Regex.Match(Record.ActivationCriterion ?? string.Empty, "GN<([0-9]+)");
                return m.Success ? int.Parse(m.Groups[1].Value) - 1 : 2;
            }
        }
        private int MaxTurns
        {
            get
            {
                var m = Regex.Match(Record.CompletionCriterion ?? string.Empty, "ST<([0-9]+)");
                return m.Success ? int.Parse(m.Groups[1].Value) : 20;
            }
        }
        public GroupSize(ChallengeRecord record, FightTeam team) : base(record, team)
        {
        }
        public override double XpBonusRatio => 0.60d;
        public override double DropBonusRatio => 0.40d;
        public override void BindEvents()
        {
            Fight.TurnStarted += OnTurnStarted;
        }
        public override void UnbindEvents()
        {
            Fight.TurnStarted -= OnTurnStarted;
        }
        public override IEnumerable<Fighter> GetAffectedFighters()
        {
            return Team.GetFighters<CharacterFighter>();
        }
        public override bool IsValid()
        {
            return GetAffectedFighters().Count() <= MaxFighters;
        }
        private void OnTurnStarted(Fight fight, Fighter fighter)
        {
            if (!IsValid() || Fight.RoundNumber >= MaxTurns)
            {
                OnChallengeResulted(ChallengeStateEnum.CHALLENGE_FAILED);
            }
        }
    }
}
