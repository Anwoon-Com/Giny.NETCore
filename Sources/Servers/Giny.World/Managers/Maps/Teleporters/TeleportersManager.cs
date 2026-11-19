using Giny.Core.DesignPattern;
using Giny.ORM;
using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.Protocol.Types;
using Giny.World.Managers.Generic;
using Giny.World.Records.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Maps.Teleporters
{
    public class TeleportersManager : Singleton<TeleportersManager>
    {
        private Dictionary<TeleporterTypeEnum, Dictionary<int, List<long>>> m_destinations = new Dictionary<TeleporterTypeEnum, Dictionary<int, List<long>>>();

        [StartupInvoke(StartupInvokePriority.SixthPath)]
        public void Intialize()
        {
            m_destinations.Add(TeleporterTypeEnum.TELEPORTER_ZAAP, new Dictionary<int, List<long>>());
            m_destinations.Add(TeleporterTypeEnum.TELEPORTER_SUBWAY, new Dictionary<int, List<long>>());

            foreach (var interactiveSkill in InteractiveSkillRecord.GetInteractiveSkills())
            {
                int zoneId;

                if (!int.TryParse(interactiveSkill.Param1, out zoneId))
                {
                    continue;
                }
                if (interactiveSkill.ActionIdentifier == GenericActionEnum.Zaap)
                {

                    var destinations = m_destinations[TeleporterTypeEnum.TELEPORTER_ZAAP];

                    if (!destinations.ContainsKey(zoneId))
                    {
                        destinations.Add(zoneId, new List<long>() { interactiveSkill.MapId });
                    }
                    else
                    {
                        destinations[zoneId].Add(interactiveSkill.MapId);
                    }
                }
                else if (interactiveSkill.ActionIdentifier == GenericActionEnum.Zaapi)
                {
                    var destinations = m_destinations[TeleporterTypeEnum.TELEPORTER_SUBWAY];

                    if (!destinations.ContainsKey(zoneId))
                    {
                        destinations.Add(zoneId, new List<long>() { interactiveSkill.MapId });
                    }
                    else
                    {
                        destinations[zoneId].Add(interactiveSkill.MapId);
                    }
                }
            }
        }

        public void AddDestination(TeleporterTypeEnum teleporterType, InteractiveTypeEnum interactiveType, GenericActionEnum genericAction, MapRecord targetMap, InteractiveElementRecord element, int zoneId)
        {
            var destinations = m_destinations[teleporterType];

            if (!destinations.ContainsKey(zoneId))
            {
                destinations.Add(zoneId, new List<long>());
            }

            if (destinations[zoneId].Contains(targetMap.Id))
            {
                return;
            }

            MapsManager.Instance.AddInteractiveSkill(targetMap, element.Identifier, genericAction,
                interactiveType, SkillTypeEnum.USE114, zoneId.ToString());


            if (!destinations.ContainsKey(zoneId))
            {
                destinations.Add(zoneId, new List<long>() { targetMap.Id });
            }
            else
            {
                destinations[zoneId].Add(targetMap.Id);
            }


        }


        public List<long> GetMaps(TeleporterTypeEnum teleporterType, int zoneId)
        {
            List<long> maps;

            if (!m_destinations[teleporterType].TryGetValue(zoneId, out maps))
            {
                return new List<long>();
            }
            return maps;
        }
        public List<long> GetMaps(TeleporterTypeEnum teleporterType)
        {
            List<long> results = new List<long>();

            foreach (var pair in m_destinations[teleporterType])
            {
                results.AddRange(pair.Value);
            }

            return results;

        }
    

    }
}
