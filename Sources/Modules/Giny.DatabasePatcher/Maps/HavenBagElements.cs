using Giny.Core;
using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Generic;
using Giny.World.Managers.Maps;
using Giny.World.Records.Maps;
using System.Linq;

namespace Giny.DatabasePatcher.Maps
{
    /// <summary>
    /// Attaches the chest (bank) skill to the interactive elements of the
    /// haven bag map (id 0).
    /// </summary>
    public class HavenBagElements
    {
        public const long HavenBagMapId = 0;
        public static void Patch()
        {
            Logger.Write("Attaching haven bag chests ...");
            int count = 0;
            var map = MapRecord.GetMap(HavenBagMapId);

            if (map == null)
            {
                Logger.Write("Haven bag map 0 not found (skipped)", Channels.Warning);
                return;
            }
            foreach (var element in map.Elements)
            {
                if (element.IsInMap() && !InteractiveSkillRecord.ExistAndHandled(element.Identifier, map.Id))
                {
                    MapsManager.Instance.AddInteractiveSkill(map, element.Identifier, GenericActionEnum.OpenBank,
                        InteractiveTypeEnum.GOLDEN_CHEST233, SkillTypeEnum.OPEN_A_CHEST260);
                    count++;
                }
            }
            Logger.Write(count + " haven bag chests attached.");
        }
    }
}
