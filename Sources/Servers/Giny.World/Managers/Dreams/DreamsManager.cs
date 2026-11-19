using Giny.Core.DesignPattern;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Monsters;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Records.Maps;
using Giny.World.Managers.Maps.Elements;
using Giny.Core;
using Giny.Core.Time;
using Giny.Protocol.Custom.Enums;
using Giny.World.Records.Items;
using Giny.World.Managers.Generic;
using Giny.World.Records.Monsters;
using Giny.World.Records.Npcs;
using Giny.World.Managers.Entities.Npcs;
using Giny.ORM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Managers.Dreams
{
    /// <summary>
    /// Puits des Songes : enchainement de salles (maps de la sous-zone 1170456).
    /// Chaque victoire envoie le joueur dans la salle suivante, chaque defaite
    /// ou sortie donne du Sable des songes proportionnel aux salles vaincues.
    /// </summary>
    public class DreamsManager : Singleton<DreamsManager>
    {
        public const long DreamsSubareaId = 1170456;
        public const long EntryMapId = 195559424;
        public const int DreamSandGid = 20226;
        public const int SandPerRoom = 5;
        public const int BossRoomPeriod = 10;

        /// <summary>
        /// Dream monsters : Eosonge, Aquasonge, Pyrosonge, Geosonge,
        /// Reflet de Prysmaradoth, Ombre Astrale.
        /// </summary>
        private static readonly int[] DreamMonsterIds = { 5489, 5490, 5491, 5492, 3577, 4044 };

        /// <summary>
        /// Dream boss : Reflet d'Urdomeda (et assimiles).
        /// </summary>
        private static readonly int[] DreamBossIds = { 6314 };

        private static MapRecord[] m_dreamMaps;
        private static readonly Random m_random = new Random();

        private static MapRecord[] GetDreamMaps()
        {
            if (m_dreamMaps == null)
            {
                m_dreamMaps = MapRecord.GetMaps().Where(x => x.SubareaId == DreamsSubareaId
                    && x.RedCells.Count > 0 && x.BlueCells.Count > 0).ToArray();
            }
            return m_dreamMaps;
        }

        public static bool IsDreamMap(MapRecord map)
        {
            return map != null && map.SubareaId == DreamsSubareaId;
        }

        /// <summary>
        /// How long a spawned world portal stays before moving elsewhere.
        /// </summary>
        public const int WorldPortalLifetimeMinutes = 30;
        public const int WorldPortalCount = 10;

        private readonly List<(MapInteractiveElement Element, ActionTimer Timer)> m_portals =
            new List<(MapInteractiveElement, ActionTimer)>();
        private int m_nextPortalIdentifier = -1;

        /// <summary>
        /// Spawns dream portals on random non-dream maps (open-world access).
        /// Each portal is a dynamic interactive element with the EnterDreams action.
        /// </summary>
        public void SpawnWorldPortals()
        {
            var candidates = MapRecord.GetMaps()
                .Where(x => !IsDreamMap(x) && x.Instance != null && x.Instance.GetElements<MapInteractiveElement>().Any())
                .ToArray();

            for (int i = 0; i < WorldPortalCount && candidates.Length > 0; i++)
            {
                var map = candidates[m_random.Next(candidates.Length)];
                SpawnPortalOn(map);
            }
        }

        private void SpawnPortalOn(MapRecord map)
        {
            // Reuse the gfx of an existing interactive on that map for the portal look.
            var donor = map.Instance.GetElements<MapInteractiveElement>().First();

            var skill = new InteractiveSkillRecord()
            {
                Id = m_nextPortalIdentifier,
                MapId = map.Id,
                ActionIdentifier = GenericActionEnum.EnterDreams,
                SkillId = SkillTypeEnum.USE184,
            };

            var record = new InteractiveElementRecord()
            {
                Identifier = m_nextPortalIdentifier--,
                CellId = donor.Record.CellId,
                GfxId = donor.Record.GfxId,
                BonesId = donor.Record.BonesId,
                MapId = map.Id,
                Skills = new List<InteractiveSkillRecord>() { skill },
            };

            var element = new MapInteractiveElement(map.Instance, record);
            map.Instance.AddElement(element);
            element.Update();

            ActionTimer timer = null;
            timer = new ActionTimer((int)TimeSpan.FromMinutes(WorldPortalLifetimeMinutes).TotalMilliseconds,
                delegate
                {
                    map.Instance.RemoveElement(element);
                    lock (m_portals) { m_portals.Remove((element, timer)); }
                    timer.Dispose();
                    SpawnPortalOn(map); // the portal reappears elsewhere
                }, false);
            timer.Start();

            lock (m_portals) { m_portals.Add((element, timer)); }
        }

        /// <summary>
        /// Dream sand merchant : a BUYSELL npc on the dreams entry map whose
        /// currency is the Dream sand item itself (tokenId = DreamSandGid).
        /// </summary>
        public const short ShopNpcTemplateId = 23;
        private static readonly int[] ShopItemGids = new int[] { 70, 77, 84, 103, 112, 121 };

        private void SpawnShopNpc()
        {
            var entry = MapRecord.GetMap(EntryMapId);

            if (NpcSpawnRecord.GetNpcsOnMap((int)EntryMapId).Any(x => x.TemplateId == ShopNpcTemplateId))
                return;

            var spawn = new NpcSpawnRecord()
            {
                Id = NpcSpawnRecord.PopNextId(),
                TemplateId = ShopNpcTemplateId,
                Template = NpcRecord.GetNpcRecord(ShopNpcTemplateId),
                MapId = (int)EntryMapId,
                CellId = entry.RandomWalkableCell().Id,
                Direction = DirectionsEnum.DIRECTION_SOUTH_EAST,
                Actions = new List<NpcActionRecord>(),
            };
            spawn.AddNow();

            var action = new NpcActionRecord()
            {
                Id = NpcActionRecord.PopNextId(),
                NpcSpawnId = spawn.Id,
                Action = NpcActionsEnum.BUYSELL,
                Param1 = string.Join(",", ShopItemGids),
                Param2 = DreamSandGid.ToString(),
                Param3 = "",
                Criteria = "",
            };
            action.AddNow();
            spawn.Actions.Add(action);

            entry.Instance.AddEntity(new Npc(spawn, entry));
        }

        [StartupInvoke("DreamsPortals", StartupInvokePriority.Last)]
        public void Initialize()
        {
            SpawnWorldPortals();
            SpawnShopNpc();
        }

        public void EnterDreams(Character character)
        {
            character.DreamsRooms = 1;
            GoToNextRoom(character);
        }

        public void ExitDreams(Character character)
        {
            RewardAndLeave(character);
        }

        /// <summary>
        /// Called when a dream PvM fight ends. Winners go deeper, losers get
        /// their sand and are sent back to the entrance.
        /// </summary>
        public void OnDreamFightEnded(CharacterFighter fighter)
        {
            var character = fighter.Character;

            if (character == null || character.DreamsRooms <= 0)
            {
                return;
            }
            if (fighter.Left || fighter.GetFighterOutcome() == Giny.Protocol.Enums.FightOutcomeEnum.RESULT_LOST)
            {
                RewardAndLeave(character);
            }
            else
            {
                character.DreamsRooms++;
                GoToNextRoom(character);
            }
        }

        private void GoToNextRoom(Character character)
        {
            var maps = GetDreamMaps();

            if (maps.Length == 0)
            {
                character.ReplyError("Aucune salle de songe disponible.");
                RewardAndLeave(character);
                return;
            }

            var map = maps[m_random.Next(maps.Length)];

            EnsureDreamGroup(map, character.DreamsRooms);

            character.Teleport(map.Id);
        }

        private void EnsureDreamGroup(MapRecord map, int room)
        {
            if (map.Instance.GetEntities<MonsterGroup>().Any())
            {
                return;
            }

            bool bossRoom = room > 0 && room % BossRoomPeriod == 0;
            var pool = bossRoom ? DreamBossIds : DreamMonsterIds;
            var random = m_random;
            var count = random.Next(2, bossRoom ? 3 : 6);
            var monsters = new List<MonsterRecord>();

            for (int i = 0; i < count; i++)
            {
                var record = MonsterRecord.GetMonsterRecord((short)pool[m_random.Next(pool.Length)]);

                if (record != null)
                {
                    monsters.Add(record);
                }
            }
            if (monsters.Count == 0)
            {
                return;
            }
            var cell = map.Instance.FindMonsterGroupCell(m_random);
            MonstersManager.Instance.AddFixedMonsterGroup(map.Instance, cell.Id, monsters.ToArray());
        }

        private void RewardAndLeave(Character character)
        {
            int rooms = Math.Max(0, character.DreamsRooms - 1);
            character.DreamsRooms = 0;

            if (rooms > 0)
            {
                character.Inventory.AddItem(DreamSandGid, rooms * SandPerRoom);
                character.Reply("Sable des songes obtenu : " + rooms * SandPerRoom + " (" + rooms + " salles).");
            }
            var entry = MapRecord.GetMap(EntryMapId);

            if (entry != null)
            {
                character.Teleport(entry.Id);
            }
        }
    }
}
