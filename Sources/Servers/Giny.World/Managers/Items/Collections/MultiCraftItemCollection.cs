using Giny.Protocol.Messages;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Records.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Items.Collections
{
    /// <summary>
    /// Job item collection that notifies both the owner (remote = false)
    /// and the other side of the exchange (remote = true).
    /// </summary>
    public class MultiCraftItemCollection : JobItemCollection
    {
        private Character Observer
        {
            get;
            set;
        }

        public MultiCraftItemCollection(Character character, Character observer) : base(character)
        {
            this.Observer = observer;
        }

        public override void OnItemAdded(CharacterItemRecord item)
        {
            base.OnItemAdded(item);

            Observer.Client.Send(new ExchangeObjectAddedMessage()
            {
                @object = item.GetObjectItem(),
                remote = true,
            });
        }
        public override void OnItemRemoved(CharacterItemRecord item)
        {
            base.OnItemRemoved(item);

            Observer.Client.Send(new ExchangeObjectRemovedMessage()
            {
                objectUID = item.UId,
                remote = true,
            });
        }
        public override void OnItemQuantityChanged(CharacterItemRecord item, int quantity)
        {
            base.OnItemQuantityChanged(item, quantity);

            Observer.Client.Send(new ExchangeObjectModifiedMessage()
            {
                @object = item.GetObjectItem(),
                remote = true,
            });
        }
        public override void OnItemsRemoved(IEnumerable<CharacterItemRecord> items)
        {
            base.OnItemsRemoved(items);

            Observer.Client.Send(new ExchangeObjectsRemovedMessage()
            {
                objectUID = items.Select(x => x.UId).ToArray(),
                remote = true,
            });
        }
    }
}
