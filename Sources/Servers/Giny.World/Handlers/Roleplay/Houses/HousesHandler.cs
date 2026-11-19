using Giny.Core.Network.Messages;
using Giny.Protocol.Messages;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Network;
using Giny.World.Records.Houses;
using Giny.ORM;

namespace Giny.World.Handlers.Roleplay.Houses
{
    class HousesHandler
    {
        [MessageHandler]
        public static void HandleHouseBuyRequest(HouseBuyRequestMessage message, WorldClient client)
        {
            var character = client.Character;
            var house = HouseRecord.GetHouse(character.CurrentHouseId);

            if (house == null)
            {
                character.Client.Send(new HouseBuyResultMessage(0, 0, false, false, 0));
                return;
            }

            if (house.OwnerId != 0 || (!house.OnSale && house.Price <= 0))
            {
                character.Client.Send(new HouseBuyResultMessage((int)house.Id, 0, false, false, 0));
                return;
            }

            if (!character.RemoveKamas(house.Price))
            {
                character.Client.Send(new HouseBuyResultMessage((int)house.Id, 0, false, false, 0));
                return;
            }

            house.OwnerId = character.Id;
            house.OnSale = false;
            house.UpdateLater();

            character.Client.Send(new HouseBuyResultMessage((int)house.Id, 0, false, true, house.Price));
            character.Reply("Maison achetée !");
        }

        [MessageHandler]
        public static void HandleHouseGuildShareRequest(HouseGuildShareRequestMessage message, WorldClient client)
        {
            var character = client.Character;
            var house = HouseRecord.GetHouse(message.houseId);

            if (house == null || house.OwnerId != character.Id || !character.HasGuild)
            {
                return;
            }

            house.GuildShared = message.enable;
            house.GuildRights = message.rights;
            house.UpdateLater();

            if (message.enable)
            {
                client.Send(new HouseGuildRightsMessage(message.houseId, message.instanceId, false,
                    character.Guild.GetGuildInformations(), message.rights));
            }
            else
            {
                client.Send(new HouseGuildNoneMessage(message.houseId, message.instanceId, false));
            }
        }
    }
}
