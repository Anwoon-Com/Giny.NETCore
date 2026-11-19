using Giny.Core.DesignPattern;
using Giny.ORM.Attributes;
using Giny.ORM.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Giny.World.Records.Houses
{
    [Table("houses")]
    public class HouseRecord : IRecord
    {
        [Container]
        private static Dictionary<long, HouseRecord> Houses = new Dictionary<long, HouseRecord>();

        [Primary]
        public long Id
        {
            get;
            set;
        }
        [Update]
        public long OwnerId
        {
            get;
            set;
        }
        [Update]
        public long Price
        {
            get;
            set;
        }
        [Update]
        public bool OnSale
        {
            get;
            set;
        }
        public bool GuildShared
        {
            get;
            set;
        }
        public int GuildRights
        {
            get;
            set;
        }

        public static HouseRecord GetHouse(long id)
        {
            return Houses.TryGetValue(id, out var result) ? result : null;
        }
        public static IEnumerable<HouseRecord> GetHouses()
        {
            return Houses.Values;
        }
    }
}
