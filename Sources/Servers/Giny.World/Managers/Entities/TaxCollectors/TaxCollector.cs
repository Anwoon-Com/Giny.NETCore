using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Types;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Entities.Look;
using Giny.World.Records.Maps;

namespace Giny.World.Managers.Entities.TaxCollectors
{
    public class TaxCollector : Entity
    {
        public const short MonsterId = 3859;

        private static readonly short[] FirstNameIds = new short[] { 11, 13, 15, 17, 19, 21, 23, 25 };
        private static readonly short[] LastNameIds = new short[] { 10, 12, 14, 16, 18, 20, 22, 24 };

        private long m_Id;
        private ServerEntityLook m_Look;

        public Character Owner
        {
            get;
            private set;
        }
        public long GuildId
        {
            get;
            private set;
        }

        public override long Id => m_Id;
        public override string Name => "Percepteur";
        public override short CellId { get; set; }
        public override DirectionsEnum Direction { get; set; }
        public override ServerEntityLook Look
        {
            get => m_Look;
            set => m_Look = value;
        }

        public TaxCollector(Character owner, MapRecord map, short cellId) : base(map)
        {
            this.Owner = owner;
            this.GuildId = owner.HasGuild ? owner.Record.GuildId : 0;
            this.m_Id = map.Instance.PopNextNPEntityId();
            this.CellId = cellId;
            this.Direction = owner.Direction;
            this.m_Look = EntityLookManager.Instance.CreateLookFromBones(1131);
        }

        public override GameRolePlayActorInformations GetActorInformations(Character target)
        {
            var random = new Random((int)m_Id);
            return new GameRolePlayTaxCollectorInformations()
            {
                contextualId = Id,
                disposition = new EntityDispositionInformations(CellId, (byte)Direction),
                look = Look.ToEntityLook(),
                taxCollectorAttack = 1,
                identification = new TaxCollectorStaticInformations()
                {
                    firstNameId = FirstNameIds[random.Next(FirstNameIds.Length)],
                    lastNameId = LastNameIds[random.Next(LastNameIds.Length)],
                    callerId = Owner?.Id ?? 0,
                    allianceIdentity = new AllianceInformation() { allianceName = string.Empty, allianceTag = string.Empty },
                },
            };
        }

        /// <summary>
        /// Kamas harvested by this collector on fights of the map.
        /// </summary>
        public long CollectedKamas
        {
            get;
            set;
        }
        /// <summary>
        /// Items harvested by this collector (gid -> quantity).
        /// </summary>
        public readonly Dictionary<int, int> CollectedItems = new Dictionary<int, int>();

        /// <summary>
        /// Ordered spells the collector casts first in defense fights (guild presets).
        /// </summary>
        public readonly List<int> OrderedSpells = new List<int>();

        public void Collect(long kamas)
        {
            this.CollectedKamas += kamas;
        }
        public void CollectItem(int gid, int quantity)
        {
            CollectedItems.TryGetValue(gid, out int qty);
            CollectedItems[gid] = qty + quantity;
        }
        public void InteractWith(Character character)
        {
            if (character.HasGuild && character.Record.GuildId == GuildId && GuildId != 0)
            {
                Map.Instance.RemoveEntity(this.Id);
                character.AddKamas(CollectedKamas);
                foreach (var kv in CollectedItems)
                {
                    character.Inventory.AddItem(kv.Key, kv.Value);
                }
                character.Reply("Vous récoltez le percepteur.");
            }
            else
            {
                character.ReplyWarning("Vous ne pouvez pas interagir avec ce percepteur.");
            }
        }
    }
}
