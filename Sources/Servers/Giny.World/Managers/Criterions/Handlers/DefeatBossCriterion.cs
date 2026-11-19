using Giny.Core.Network;
using Giny.Protocol.Enums;
using Giny.World.Managers.Criterias;
using Giny.World.Managers.Fights;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Criterions.Handlers
{
    /// <summary>
    /// EH : defeat a boss monster.
    /// </summary>
    [CriterionHandler("EH")]
    public class DefeatBossCriterion : Criterion
    {
        public short MonsterId
        {
            get;
            set;
        }
        public int Count
        {
            get;
            set;
        }
        public DefeatBossCriterion(string criteriaFull) : base(criteriaFull)
        {
            var split = Value.Split(',');
            MonsterId = short.Parse(split[0]);
            Count = split.Length > 1 ? int.Parse(split[1]) : 0;
        }
        public override bool Eval(WorldClient client)
        {
            if (!client.Character.Fighting)
            {
                return false;
            }
            if (client.Character.Fighter.Fight is FightPvM fightPvM)
            {
                if (fightPvM.Winners != client.Character.Fighter.Team)
                {
                    return false;
                }
                var team = fightPvM.GetTeam(TeamTypeEnum.TEAM_TYPE_MONSTER).GetFighters<MonsterFighter>(false);
                var count = team.Count(x => x.Monster.Record.Id == MonsterId && x.Monster.Record.IsBoss);
                if (count < Count)
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
