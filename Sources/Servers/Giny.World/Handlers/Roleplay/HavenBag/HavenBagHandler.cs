using Giny.Core.Network.Messages;
using Giny.Protocol.Messages;
using Giny.Protocol.Enums;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Network;
using Giny.World.Managers.Maps;
using Giny.ORM;
using Giny.Protocol.Types;
using Giny.World.Records.Characters;
using System.Linq;

namespace Giny.World.Handlers.Roleplay.HeavenBag
{
    class HeavenBagHandler
    {
        [MessageHandler]
        public static void HandleEnterHavenBagRequest(EnterHavenBagRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (character.Fighting || character.Busy || character.InHavenbag)
                return;

            if (message.havenBagOwner != character.Id)
            {
                // Guest entry : invited players (or guildmates when the share flag
                // allows it) can enter someone else's haven bag.
                var ownerClient = WorldServer.Instance.GetClient(x => x.Character != null && x.Character.Id == message.havenBagOwner);
                var owner = ownerClient != null ? ownerClient.Character : null;

                bool allowed = owner != null
                    && (owner.HavenBagGuests.Contains(character.Id)
                        || ((owner.HavenBagShare & (byte)HavenBagShareBitEnum.HAVEN_BAG_GUILD) != 0
                            && character.HasGuild && character.Record.GuildId == owner.Record.GuildId));

                if (!allowed)
                {
                    character.ReplyWarning("Vous n'avez pas accès à ce havre-sac.");
                    return;
                }

                character.HavenbagReturnMap = character.Map;
                character.HavenbagReturnCellId = character.CellId;
                character.HavenBagOwnerId = owner.Id;
                character.Teleport(owner.HavenBagThemeMapId);
                return;
            }

            character.HavenbagReturnMap = character.Map;
            character.HavenbagReturnCellId = character.CellId;
            character.HavenBagOwnerId = character.Id;

            character.Teleport(Character.HavenbagMapId);
        }

        [MessageHandler]
        public static void HandleExitHavenBagRequest(ExitHavenBagRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag || character.Fighting || character.Busy)
                return;

            if (character.HavenbagReturnMap != null)
                character.Teleport(character.HavenbagReturnMap, character.HavenbagReturnCellId);
            else
                character.Teleport(character.Record.SpawnPointMapId);
        }

        [MessageHandler]
        public static void HandleHavenBagFurnituresRequest(HavenBagFurnituresRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            var furnitures = HavenbagFurnitureRecord.GetFurnitures(character.Id)
                .Select(x => new HavenBagFurnitureInformation(x.CellId, x.FurnitureId, x.Orientation))
                .ToArray();

            client.Send(new HavenBagFurnituresMessage(furnitures));
        }

        [MessageHandler]
        public static void HandleEditHavenBagRequest(EditHavenBagRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag || character.Fighting || character.Busy)
                return;

            client.Send(new EditHavenBagStartMessage());
        }

        [MessageHandler]
        public static void HandleEditHavenBagFinished(EditHavenBagFinishedMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag || character.Fighting || character.Busy)
                return;

            // Replace the whole placement (official protocol sends the
            // complete furniture list once the edition is validated).
            foreach (var furniture in HavenbagFurnitureRecord.GetFurnitures(character.Id))
            {
                furniture.RemoveLater();
            }

            foreach (var info in message.furnituresInfos)
            {
                HavenbagFurnitureRecord.New(character.Id, 0, info.funitureId, info.cellId, info.orientation);
            }

            client.Send(new HavenBagFurnituresMessage(message.furnituresInfos));
        }

        [MessageHandler]
        public static void HandleChangeThemeRequest(ChangeThemeRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag || character.Fighting || character.Busy)
                return;

            if (!HavenbagThemes.Maps.TryGetValue(message.theme, out int mapId))
            {
                character.ReplyWarning("Theme de havre-sac invalide.");
                return;
            }

            character.HavenBagThemeId = message.theme;
            character.HavenBagThemeMapId = mapId;
            character.Teleport(mapId);
        }

        [MessageHandler]
        public static void HandleChangeHavenBagRoomRequest(ChangeHavenBagRoomRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag || character.Fighting || character.Busy)
                return;

            character.HavenBagRoomId = message.roomId;
            client.Send(new HavenBagRoomUpdateMessage(message.roomId,
                new HavenBagRoomPreviewInformation[] { new HavenBagRoomPreviewInformation(message.roomId, character.HavenBagThemeId) }));
        }

        [MessageHandler]
        public static void HandleKickHavenBagRequest(KickHavenBagRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (!character.InHavenbag)
                return;

            var guestClient = WorldServer.Instance.GetClient(x => x.Character != null && x.Character.Id == message.guestId);

            if (guestClient == null)
                return;

            var guest = guestClient.Character;

            if (guest.InHavenbag && guest.HavenBagOwnerId == character.Id)
            {
                guest.HavenBagOwnerId = 0;
                guest.Teleport(guest.Record.SpawnPointMapId);
            }
        }

        [MessageHandler]
        public static void HandleEditHavenBagCancelRequest(EditHavenBagCancelRequestMessage message, WorldClient client)
        {
            // Edition cancelled : nothing to do, persisted placements stay untouched.
        }
    }
}
