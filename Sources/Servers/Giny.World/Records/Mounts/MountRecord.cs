using Giny.Core.DesignPattern;
using Giny.Core.Extensions;
using Giny.Core.Pool;
using Giny.ORM;
using Giny.ORM.Attributes;
using Giny.ORM.Interfaces;
using Giny.Protocol.Types;
using Giny.World.Managers.Effects;
using Giny.World.Records.Items;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Records.Mounts
{
    [Table("mounts")]
    public class MountRecord : IRecord
    {
        [Container]
        private static readonly ConcurrentDictionary<long, MountRecord> Mounts = new ConcurrentDictionary<long, MountRecord>();

        private static UniqueLongIdProvider idProvider;

        [Primary]
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// Owner character id, 0 when stored in a paddock (MapId set).
        /// </summary>
        [Update]
        public long OwnerId
        {
            get;
            set;
        }

        /// <summary>
        /// Mount model id (gid of the certificate item).
        /// </summary>
        public int ModelId
        {
            get;
            set;
        }

        [Update]
        public string Name
        {
            get;
            set;
        }

        [Update]
        public bool Sex
        {
            get;
            set;
        }

        [Update]
        public byte Level
        {
            get;
            set;
        }

        [Update]
        public long Experience
        {
            get;
            set;
        }

        public EffectCollection Effects
        {
            get;
            set;
        }

        [Update]
        public bool Equiped
        {
            get;
            set;
        }

        [Update]
        public bool Riding
        {
            get;
            set;
        }

        /// <summary>
        /// Paddock map id where the mount is stored. 0 when carried by its owner.
        /// </summary>
        [Update]
        public long MapId
        {
            get;
            set;
        }

        [Update]
        public bool Sterilized
        {
            get;
            set;
        }

        [Update]
        public DateTime LastBred
        {
            get;
            set;
        }

        [Update]
        public DateTime StableSince
        {
            get;
            set;
        }

        [Update]
        public int XpRatio
        {
            get;
            set;
        }

        public MountRecord()
        {
            this.Effects = new EffectCollection();
        }

        /// <summary>
        /// Grants experience to the mount. Experience is accumulated and the
        /// level is recomputed with a progressive 100-levels scale.
        /// </summary>
        public void AddExperience(long xp)
        {
            if (xp <= 0 || Level >= 100)
            {
                return;
            }
            this.Experience += xp;

            long required = 0;
            byte level = 1;

            while (level < 100)
            {
                required += GetLevelExperienceFloor(level);

                if (Experience < required)
                {
                    break;
                }
                level++;
            }
            this.Level = level;
            this.UpdateLater();
        }

        /// <summary>
        /// Experience required to go from the given level to the next one.
        /// </summary>
        private static long GetLevelExperienceFloor(int level)
        {
            return level * level * 100L;
        }

        [StartupInvoke(StartupInvokePriority.Last)]
        public static void Initialize()
        {
            long lastId = Mounts.Count > 0 ? Mounts.Keys.Max() : 0;
            idProvider = new UniqueLongIdProvider(lastId);
        }

        public static MountRecord CreateMount(int modelId, long ownerId, long mapId)
        {
            var mount = new MountRecord();
            mount.Id = NextId();
            mount.ModelId = modelId;
            mount.OwnerId = ownerId;
            mount.MapId = mapId;
            mount.Name = "";
            mount.Sex = new Random().Next(2) == 0;
            mount.Level = 1;
            mount.Experience = 0;
            mount.Effects = new EffectCollection();
            mount.Equiped = false;
            mount.Riding = false;
            mount.Sterilized = false;
            mount.XpRatio = 0;
            mount.AddLater();
            return mount;
        }

        public static long NextId()
        {
            return idProvider.Pop();
        }

        public static IEnumerable<MountRecord> GetMounts()
        {
            return Mounts.Values;
        }

        public static MountRecord GetMountRecord(long id)
        {
            return Mounts.TryGetValue(id);
        }

        public static IEnumerable<MountRecord> GetMountsByOwnerId(long ownerId)
        {
            return Mounts.Values.Where(x => x.OwnerId == ownerId);
        }

        public static IEnumerable<MountRecord> GetMountsInPaddock(long mapId)
        {
            return Mounts.Values.Where(x => x.MapId == mapId);
        }

        public MountClientData GetMountClientData()
        {
            return new MountClientData(Id, ModelId, new int[0], new int[0], Name, Sex, (int)OwnerId,
                Experience, Experience, 0, Level, true, 1000, false,
                0, 1000, 0, 0, 100, 100, 0, 0, 0, 0, 0, 0, false, 0, 0, 0,
                Sterilized ? 0 : -1, 0, false, Effects.GetObjectEffects().OfType<ObjectEffectInteger>().ToArray());
        }
    }
}
