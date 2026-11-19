using Giny.Core;
using Giny.Protocol.Custom.Enums;
using Giny.World.Managers.Generic;
using Giny.World.Managers.Maps;
using Giny.World.Records.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.DatabasePatcher.Maps
{
    public class SmithmagicElements
    {
        public static void Patch()
        {
            Logger.Write("Spawning smithmagic workshops ...");

            // Abraknydes workshop [-5,-13] (Hel Munster)
            AddStation(149946368, 488201, InteractiveTypeEnum.MAGIC_ANVIL92, SkillTypeEnum.SMITHMAGE113);
            AddCrusher(149946368, 488126);

            // Bonta workshop [-29,-57]
            AddStations(217057286, 521408, 521407, 521409, 523833);
            AddCrusher(217057286, 521674);

            // Brakmar workshop [-26,38]
            AddStations(216927751, 521410, 521411, 521412, 523832);
            AddCrusher(216927751, 521403);

            // Pandala workshop [17,-29] (no magic anvil)
            AddStation(211289088, 520501, InteractiveTypeEnum.MAGIC_SEWING_MACHINE116, SkillTypeEnum.COSTUMAGE166);
            AddStation(211289088, 520499, InteractiveTypeEnum.MAGIC_WORKSHOP117, SkillTypeEnum.JEWELMAGE168);
            AddStation(211289088, 520500, InteractiveTypeEnum.MAGIC_TABLE118, SkillTypeEnum.SHOEMAGE164);
            AddCrusher(211289088, 520621);

            // Sufokia workshop [13,24]
            AddStations(91750921, 520542, 520540, 520541, 520539);
            AddCrusher(91750921, 520538);

            // Frigost workshop [-83,-40]
            AddStations(54530069, 520561, 520560, 520559, 520563);
            AddCrusher(54530069, 520564);

            // Rok Island workshop [38,-20]
            AddStations(163055618, 502523, 502521, 502522, 502520);
            AddCrusher(163055618, 502519);
        }

        private static void AddStations(int mapId, int anvilId, int sewingMachineId, int workshopId, int tableId)
        {
            AddStation(mapId, anvilId, InteractiveTypeEnum.MAGIC_ANVIL92, SkillTypeEnum.SMITHMAGE113);
            AddStation(mapId, sewingMachineId, InteractiveTypeEnum.MAGIC_SEWING_MACHINE116, SkillTypeEnum.COSTUMAGE166);
            AddStation(mapId, workshopId, InteractiveTypeEnum.MAGIC_WORKSHOP117, SkillTypeEnum.JEWELMAGE168);
            AddStation(mapId, tableId, InteractiveTypeEnum.MAGIC_TABLE118, SkillTypeEnum.SHOEMAGE164);
        }

        private static void AddStation(int mapId, int elementId, InteractiveTypeEnum type, SkillTypeEnum skillType)
        {
            MapsManager.Instance.AddInteractiveSkill(MapRecord.GetMap(mapId), elementId, GenericActionEnum.Smithmagic,
                type, skillType);
        }

        private static void AddCrusher(int mapId, int elementId)
        {
            MapsManager.Instance.AddInteractiveSkill(MapRecord.GetMap(mapId), elementId, GenericActionEnum.RuneTrade,
                InteractiveTypeEnum.MUNSTER_CRUSHER93, SkillTypeEnum.SHATTER_ITEMS181);
        }
    }
}
