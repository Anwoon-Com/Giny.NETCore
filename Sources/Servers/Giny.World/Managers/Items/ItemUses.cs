using Giny.Core.Extensions;
using Giny.ORM;
using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.World.Managers.Effects;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Fights;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Managers.Monsters;
using Giny.World.Managers.Stats;
using Giny.World.Records.Items;
using Giny.World.Managers.Mounts;
using Giny.World.Records.Mounts;
using Giny.World.Records.Maps;
using Giny.World.Records.Mounts;
using Giny.World.Records.Monsters;
using Giny.World.Records.Spells;
using Giny.World.Records.Tinsel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Items
{
    public class CandyBoost
    {
        public EffectCollection Effects;
        public int RemainingFights;

        public CandyBoost(EffectCollection effects, int remainingFights)
        {
            this.Effects = effects;
            this.RemainingFights = remainingFights;
        }
    }
    public class ItemUses
    {
        [ItemUsageHandler(EffectsEnum.Effect_SoulStoneSummon)]
        public static bool UseSoulStone(Character character, Effect effect)
        {
            EffectString stringEffect = effect as EffectString;

            if (stringEffect == null || stringEffect.Value == null || character.Fighting)
            {
                return false;
            }

            var monsters = new List<MonsterRecord>();
            var grades = new List<byte>();

            foreach (var entry in stringEffect.Value.Split(';'))
            {
                var parts = entry.Split(',');

                if (parts.Length != 2)
                {
                    continue;
                }

                var record = MonsterRecord.GetMonsterRecord(short.Parse(parts[0]));

                if (record != null)
                {
                    monsters.Add(record);
                    grades.Add(byte.Parse(parts[1]));
                }
            }

            if (monsters.Count == 0)
            {
                return false;
            }

            MonstersManager.Instance.AddFixedMonsterGroup(character.Map.Instance, character.CellId, monsters.ToArray(), grades.ToArray());
            return true;
        }
        /// <summary>
        /// Candies / boost food : applies the item stat effects to the character for
        /// Effect_RemainingFights fights, then they expire.
        /// </summary>
        [ItemUsageHandler(ItemTypeEnum.BOOST_FOOD)]
        public static bool UseBoostFood(Character character, CharacterItemRecord item)
        {
            var remainingFights = item.Effects.OfType<EffectInteger>().FirstOrDefault(x => x.EffectEnum == EffectsEnum.Effect_RemainingFights);

            int fights = remainingFights != null ? remainingFights.Value : 1;

            ItemEffectsManager.Instance.AddEffects(character, item.Effects);
            character.ActiveBoosts.Add(new CandyBoost(item.Effects, fights));
            character.RefreshStats();
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_LearnSpell)]
        public static bool LearnSpell(Character character, EffectInteger effect)
        {
            var level = SpellLevelRecord.GetSpellLevel(effect.Value);
            var record = SpellRecord.GetSpellRecord(level.SpellId);

            return character.LearnSpell(record.Id, true);
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddTitle)]
        public static bool AddTitle(Character character, EffectInteger effect)
        {
            var titleId = (short)effect.Value;

            if (TitleRecord.Exists(titleId))
            {
                character.LearnTitle(titleId);
                return true;
            }

            return false;
        }
        [ItemUsageHandler(EffectsEnum.Effect_ItemTeleportMapReference)]
        public static bool TeleportToMapReference(Character character, EffectInteger effect)
        {
            MapReferenceRecord? reference = MapReferenceRecord.GetMapReference(effect.Value);

            if (reference == null)
            {
                return false;
            }

            character.Teleport(reference.MapId, reference.CellId);
            return true;

        }
        [ItemUsageHandler(EffectsEnum.Effect_ConsultDocument)]
        public static bool ConsultDocument(Character character, EffectInteger effect)
        {
            character.OpenBookDialog(effect.Value);
            return false;
        }
        [ItemUsageHandler(EffectsEnum.Effect_TeleportToSavePoint)]
        public static bool TeleportSavePoint(Character character, EffectInteger effect)
        {
            character.PlayEmote(61);
            character.SpawnPoint();
            return true;
        }
        /// <summary>
        /// Keyrings : using the item teleports the player to the dungeon entrance map
        /// carried by the Effect_Teleport effect and consumes it.
        /// </summary>
        [ItemUsageHandler(EffectsEnum.Effect_Teleport)]
        public static bool Teleport(Character character, EffectInteger effect)
        {
            var map = MapRecord.GetMap(effect.Value);

            if (map == null)
            {
                return false;
            }

            int week = DateTime.Now.Year * 100 + System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now);

            if (character.KeyringUsages.TryGetValue(map.Id, out int usedWeek) && usedWeek == week)
            {
                character.ReplyWarning("Vous avez déjà utilisé un trousseau pour ce donjon cette semaine.");
                return false;
            }
            character.KeyringUsages[map.Id] = week;
            character.Teleport(map.Id);
            return true;
        }
        /// <summary>
        /// Candies : apply the appearance (bones id) carried by the effect to the character.
        /// </summary>
        [ItemUsageHandler(EffectsEnum.Effect_ChangeAppearance)]
        public static bool ChangeAppearance(Character character, EffectInteger effect)
        {
            var look = character.Look.Clone();
            look.SetBones((short)effect.Value);
            character.Record.ContextualLook = look;
            character.RefreshActorOnMap();
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_LearnEmote)]
        public static bool LearnEmote(Character character, EffectInteger effect)
        {
            character.LearnEmote((byte)effect.Value);
            return true;
        }
        [ItemUsageHandler(ItemTypeEnum.BREEDING_ITEM)]
        public static bool UseRaisingObject(Character character, CharacterItemRecord item)
        {
            var stabled = MountRecord.GetMountsByOwnerId(character.Id).Where(x => x.MapId != 0).ToList();

            if (stabled.Count == 0)
            {
                character.ReplyWarning("Aucune monture en enclos : l'objet d'elevage n'a aucun effet.");
                return false;
            }

            var stat = RaisingObjects.StatOf.TryGetValue((int)item.GId, out var s)
                ? s : MountRaisingStat.Maturity;

            foreach (var mount in stabled)
            {
                mount.AddExperience(RaisingObjects.XpPerUse[stat]);

                // Trained stats speed up the next breeding.
                if (stat != MountRaisingStat.Energy && mount.LastBred != default)
                {
                    mount.LastBred = mount.LastBred.AddHours(-1);
                }

                mount.UpdateLater();
            }

            character.Reply(stabled.Count + " monture(s) entrainee(s) : " + stat);
            return true;
        }

        [ItemUsageHandler(EffectsEnum.Effect_LearnSmileyPack)]
        public static bool LearnSmileyPack(Character character, EffectInteger effect)
        {
            if (effect is EffectDice dice)
            {
                for (short i = (short)dice.Min; i <= (short)dice.Max; i++)
                {
                    character.LearnEmote(i);
                }
            }

            if (effect.Value > 0)
            {
                character.LearnEmote((short)effect.Value);
            }

            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentAgility)]
        public static bool PermanentAgility(Character character, EffectInteger effect)
        {
            character.Record.Stats.Agility.Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 12, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentStrength)]
        public static bool PermanentStrength(Character character, EffectInteger effect)
        {
            character.Record.Stats.Strength.Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 10, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentIntelligence)]
        public static bool PermanentIntelligence(Character character, EffectInteger effect)
        {
            character.Record.Stats.Intelligence.Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 14, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentWisdom)]
        public static bool PermanentWisdom(Character character, EffectInteger effect)
        {
            character.Record.Stats.Wisdom.Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 9, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentChance)]
        public static bool PermanentChance(Character character, EffectInteger effect)
        {
            character.Record.Stats.Chance.Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 11, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddPermanentVitality)]
        public static bool PermanentVitality(Character character, EffectInteger effect)
        {
            character.Record.Stats.GetCharacteristic<DetailedCharacteristic>(CharacteristicEnum.VITALITY).Additional += (short)effect.Value;
            character.TextInformation(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 13, effect.Value);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_AddExp)]
        public static bool AddExp(Character character, EffectInteger effect)
        {
            character.AddExperience(effect.Value, true);
            return true;
        }
        [ItemUsageHandler(EffectsEnum.Effect_SummonTaxcollector)]
        public static bool SummonTaxcollector(Character character, EffectInteger effect)
        {
            if (!character.HasGuild)
            {
                character.ReplyWarning("Vous devez appartenir à une guilde pour poser un percepteur.");
                return false;
            }

            var taxCollector = new Managers.Entities.TaxCollectors.TaxCollector(character, character.Map, character.CellId);

            character.Map.Instance.AddEntity(taxCollector);
            var info = (Giny.Protocol.Types.GameRolePlayTaxCollectorInformations)taxCollector.GetActorInformations(character);
            character.Map.Instance.Send(new Giny.Protocol.Messages.TaxCollectorAddedMessage(character.Id,
                new Giny.Protocol.Types.TaxCollectorInformations()
                {
                    uniqueId = taxCollector.Id,
                    firstNameId = info.identification.firstNameId,
                    lastNameId = info.identification.lastNameId,
                    allianceIdentity = info.identification.allianceIdentity,
                    additionalInfos = new Giny.Protocol.Types.AdditionalTaxCollectorInformation()
                    {
                        collectorCallerId = character.Id,
                        collectorCallerName = character.Name,
                    },
                    look = info.look,
                    state = (byte)Giny.Protocol.Enums.TaxCollectorStateEnum.STATE_COLLECTING,
                }));

            return true;
        }
        [ItemUsageHandler(14485)]
        public static bool Mimicry(Character character, CharacterItemRecord item)
        {
            character.OpenUIByObject(ObjectUITypeEnum.MIMICRY, item.UId);
            return false;
        }

        [ItemUsageHandler(16836)]
        public static bool Container16836(Character character, CharacterItemRecord item)
        {
            character.Inventory.AddItem(2012, 10);
            return true;
        }
        [ItemUsageHandler(16833)]
        public static bool Container16833(Character character, CharacterItemRecord item)
        {
            character.Inventory.AddItem(519, 10);
            return true;
        }
        [ItemUsageHandler(16828)]
        public static bool Container16828(Character character, CharacterItemRecord item)
        {
            character.Inventory.AddItem(6671, 10);
            return true;
        }

        [ItemUsageHandler(EffectsEnum.Effect_SpellAnim)]
        public static bool SpellAnim(Character character, EffectInteger effect)
        {
            if (effect is EffectDice dice)
            {
                character.PlaySpellAnimOnMap(character.CellId, (short)dice.Max, 1, (DirectionsEnum)dice.Value);
                return true;
            }
            return false;
        }

        [ItemUsageHandler(EffectsEnum.Effect_ChallengeMonster)]
        public static bool ChallengeMonster(Character character, EffectInteger effect)
        {
            if (effect is EffectDice dice)
            {
                MonsterRecord record = MonsterRecord.GetMonsterRecord((short)dice.Max);
                Monster monster = new Monster(record, character.GetCell(), (byte)dice.Min);

                var fight = FightManager.Instance.CreateFightContextual(character);
                fight.BlueTeam.AddFighter(character.CreateFighter(fight.BlueTeam));

                fight.RedTeam.AddFighter(new MonsterFighter(fight.RedTeam, character.GetCell(), monster));

                fight.StartPlacement();
                return true;
            }
            return false;
        }

        [ItemUsageHandler(ItemTypeEnum.DRAGOTURKEY_CERTIFICATE)]
        public static bool DragoturkeyCertificate(Character character, CharacterItemRecord item)
        {
            return CreateMountFromCertificate(character, item);
        }

        [ItemUsageHandler(ItemTypeEnum.SEEMYOOL_CERTIFICATE)]
        public static bool SeemyoolCertificate(Character character, CharacterItemRecord item)
        {
            return CreateMountFromCertificate(character, item);
        }

        [ItemUsageHandler(ItemTypeEnum.RHINEETLE_CERTIFICATE)]
        public static bool RhineetleCertificate(Character character, CharacterItemRecord item)
        {
            return CreateMountFromCertificate(character, item);
        }

        private static bool CreateMountFromCertificate(Character character, CharacterItemRecord item)
        {
            MountRecord mount = new MountRecord()
            {
                Id = MountRecord.NextId(),
                OwnerId = character.Id,
                ModelId = item.GId,
                Name = item.Record.Name,
                Level = 1,
                Experience = 0,
                Sex = new Random().Next(2) == 0,
                Effects = item.Record.Effects.Generate(),
                Equiped = false,
                Riding = false,
                MapId = 0,
                Sterilized = false,
            };

            mount.AddNow();
            character.Inventory.RemoveItem(item.UId, 1);
            character.EquipMount(mount);

            character.Reply("Monture obtenue : " + mount.Name);
            return true;
        }
    }
}
