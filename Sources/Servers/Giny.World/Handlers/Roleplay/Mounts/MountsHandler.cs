using Giny.Core.Network.Messages;
using Giny.ORM;
using Giny.Protocol.Messages;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Network;
using Giny.World.Records.Mounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Handlers.Roleplay.Mounts
{
    class MountsHandler
    {
        [MessageHandler]
        public static void HandleMountToggleRidingRequestMessage(MountToggleRidingRequestMessage message, WorldClient client)
        {
            client.Character.ToggleRiding();
        }

        [MessageHandler]
        public static void HandleMountInformationRequestMessage(MountInformationRequestMessage message, WorldClient client)
        {
            var mount = MountRecord.GetMountRecord((long)message.id);

            if (mount == null || mount.OwnerId != client.Character.Id)
            {
                client.Send(new MountDataErrorMessage(0));
                return;
            }

            client.Send(new MountDataMessage(mount.GetMountClientData()));
        }

        [MessageHandler]
        public static void HandleMountInformationInPaddockRequestMessage(MountInformationInPaddockRequestMessage message, WorldClient client)
        {
            var mount = MountRecord.GetMountsInPaddock(message.mapRideId)
                .FirstOrDefault(x => x.OwnerId == client.Character.Id);

            if (mount == null)
            {
                client.Send(new MountDataErrorMessage(0));
                return;
            }

            client.Send(new MountDataMessage(mount.GetMountClientData()));
        }

        [MessageHandler]
        public static void HandleMountRenameRequestMessage(MountRenameRequestMessage message, WorldClient client)
        {
            var mount = client.Character.Mount;

            if (mount == null)
            {
                client.Send(new MountDataErrorMessage(0));
                return;
            }

            mount.Name = message.name;
            mount.UpdateLater();

            client.Send(new MountRenamedMessage((int)mount.Id, mount.Name));
            client.Send(new MountDataMessage(mount.GetMountClientData()));
        }

        [MessageHandler]
        public static void HandleMountReleaseRequestMessage(MountReleaseRequestMessage message, WorldClient client)
        {
            var mount = client.Character.Mount;

            if (mount == null)
            {
                return;
            }

            client.Character.UnequipMount();
            mount.RemoveLater();

            client.Send(new MountReleasedMessage((int)mount.Id));
        }

        [MessageHandler]
        public static void HandleMountSterilizeRequestMessage(MountSterilizeRequestMessage message, WorldClient client)
        {
            var mount = client.Character.Mount;

            if (mount == null)
            {
                client.Send(new MountDataErrorMessage(0));
                return;
            }

            mount.Sterilized = !mount.Sterilized;
            mount.UpdateLater();

            client.Send(new MountDataMessage(mount.GetMountClientData()));
        }

        [MessageHandler]
        public static void HandleMountSetXpRatioRequestMessage(MountSetXpRatioRequestMessage message, WorldClient client)
        {
            var mount = client.Character.Mount;

            if (mount == null)
            {
                client.Send(new MountDataErrorMessage(0));
                return;
            }

            mount.XpRatio = message.xpRatio;
            mount.UpdateLater();
        }
    }
}
