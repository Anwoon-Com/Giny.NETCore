using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Entities.TaxCollectors;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Monsters;
using Giny.World.Records.Maps;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Fights
{
    /// <summary>
    /// Perceptor defense fight : attackers (red/challenger) against the
    /// perceptor, fought as a monster group (monster 3859). When the
    /// perceptor loses, it is removed from the map and its collected
    /// kamas go to the winners.
    /// </summary>
    public class FightTaxCollector : FightPvM
    {
        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_PvT;

        private TaxCollector TaxCollector
        {
            get;
            set;
        }

        public FightTaxCollector(Character origin, int id, MapRecord map, FightTeam blueTeam,
            FightTeam redTeam, CellRecord cell, MonsterGroup monsterGroup, TaxCollector taxCollector)
            : base(origin, id, map, blueTeam, redTeam, cell, monsterGroup)
        {
            this.TaxCollector = taxCollector;
        }

        public IEnumerable<Fighter> CreateMonsterFighters()
        {
            return MonsterGroup.CreateFighters(BlueTeam);
        }

        public override void OnFightEnded()
        {
            if (Winners == GetTeam(TeamTypeEnum.TEAM_TYPE_PLAYER))
            {
                Map.Instance.RemoveEntity(TaxCollector.Id);

                long share = TaxCollector.CollectedKamas;

                if (share > 0)
                {
                    var winners = GetTeam(TeamTypeEnum.TEAM_TYPE_PLAYER).GetFighters<CharacterFighter>(false).ToArray();

                    foreach (var winner in winners)
                    {
                        winner.Character.AddKamas(share / winners.Length);
                    }
                }
            }
            base.OnFightEnded();
        }
    }
}
