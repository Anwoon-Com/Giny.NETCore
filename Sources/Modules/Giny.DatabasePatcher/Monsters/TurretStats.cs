using Giny.Core;
using Giny.ORM;
using Giny.World.Records.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.DatabasePatcher.Monsters
{
    /// <summary>
    /// Steamer turrets must inherit a share of their summoner's
    /// characteristics, otherwise they deal near-zero damage.
    /// </summary>
    public class TurretStats
    {
        /// <summary>
        /// Base turrets + "évolution I/II/III" head turret monsters.
        /// </summary>
        private static readonly int[] TurretMonsterIds = new int[]
        {
            5071, 5072, 5204, 5205, 5206, 6737, 9275, 9278, 10130, 20138,
            14279, 14280, 14281, 14282, 14284, 14285, 14286, 14287, 14288,
            14289, 14290, 14291, 14292, 14293, 14294, 14295, 14296, 14297,
        };

        public static void Patch()
        {
            Logger.Write("Patching turret summon stats ...");

            int count = 0;

            foreach (var id in TurretMonsterIds)
            {
                MonsterRecord record = MonsterRecord.GetMonsterRecord((short)id);

                if (record == null)
                {
                    continue;
                }

                bool changed = false;

                foreach (var grade in record.Grades)
                {
                    if (grade.BonusCharacteristics == null)
                    {
                        grade.BonusCharacteristics = new MonsterBonusCharacteristics();
                    }

                    var bonus = grade.BonusCharacteristics;

                    if (bonus.Strength != 0 || bonus.Intelligence != 0 || bonus.Agility != 0 || bonus.Chance != 0)
                    {
                        continue;
                    }

                    bonus.LifePoints = 40;
                    bonus.Strength = 100;
                    bonus.Intelligence = 100;
                    bonus.Chance = 100;
                    bonus.Agility = 100;
                    bonus.Wisdom = 100;
                    bonus.TackleBlock = 50;
                    bonus.TackleEvade = 50;
                    bonus.FireResistance = 30;
                    bonus.WaterResistance = 30;
                    bonus.EarthResistance = 30;
                    bonus.AirResistance = 30;
                    bonus.NeutralResistance = 30;
                    bonus.BonusEarthDamage = 30;
                    bonus.BonusFireDamage = 30;
                    bonus.BonusWaterDamage = 30;
                    bonus.BonusAirDamage = 30;

                    changed = true;
                }

                if (changed)
                {
                    record.UpdateLater();
                    count++;
                }
            }

            Logger.Write(count + " turret monsters patched.");
        }
    }
}
