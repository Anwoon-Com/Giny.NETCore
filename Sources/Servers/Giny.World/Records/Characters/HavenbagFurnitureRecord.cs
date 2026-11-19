using Giny.ORM;
using Giny.ORM.Attributes;
using Giny.ORM.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Records.Characters
{
    /// <summary>
    /// Persisted haven bag furniture placement.
    /// </summary>
    [Table("havenbag_furnitures")]
    public class HavenbagFurnitureRecord : IRecord
    {
        [Container]
        private static readonly ConcurrentDictionary<long, HavenbagFurnitureRecord> Furnitures = new ConcurrentDictionary<long, HavenbagFurnitureRecord>();

        [Primary]
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public int RoomId { get; set; }
        public int FurnitureId { get; set; }
        public short CellId { get; set; }
        public byte Orientation { get; set; }

        public static IEnumerable<HavenbagFurnitureRecord> GetFurnitures(long ownerId)
        {
            return Furnitures.Values.Where(x => x.OwnerId == ownerId);
        }

        public static HavenbagFurnitureRecord New(long ownerId, int roomId, int furnitureId, short cellId, byte orientation)
        {
            var record = new HavenbagFurnitureRecord()
            {
                Id = Furnitures.Keys.DefaultIfEmpty(0L).Max() + 1,
                OwnerId = ownerId,
                RoomId = roomId,
                FurnitureId = furnitureId,
                CellId = cellId,
                Orientation = orientation,
            };
            record.AddNow();
            return record;
        }
    }
}
