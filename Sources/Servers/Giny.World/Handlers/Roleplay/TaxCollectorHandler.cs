using Giny.Core.Network.Messages;
using Giny.Protocol.Messages;
using Giny.Protocol.Types;
using Giny.World.Managers.Entities.TaxCollectors;
using Giny.World.Records.Guilds;
using Giny.World.Managers.Fights;
using Giny.World.Managers.Fights.Fighters;
using Giny.World.Network;
using System.Linq;

namespace Giny.World.Handlers.Roleplay
{
    class TaxCollectorHandler
    {
        [MessageHandler]
        public static void HandleGameRolePlayTaxCollectorFightRequestMessage(
            GameRolePlayTaxCollectorFightRequestMessage message, WorldClient client)
        {
            var character = client.Character;

            if (character.Busy || character.Map == null)
            {
                return;
            }
            var taxCollector = character.Map.Instance.GetEntities<TaxCollector>().FirstOrDefault();

            if (taxCollector == null)
            {
                character.ReplyError("Aucun percepteur sur cette carte.");
                return;
            }
            if (character.HasGuild && character.Record.GuildId == taxCollector.GuildId && taxCollector.GuildId != 0)
            {
                character.ReplyError("Impossible d'attaquer le percepteur de votre guilde.");
                return;
            }

            var cell = character.Map.GetCell(taxCollector.CellId);
            var fight = FightManager.Instance.CreateFightTaxCollector(character, taxCollector, character.Map, cell);

            foreach (var fighter in fight.CreateMonsterFighters())
            {
                fight.BlueTeam.AddFighter(fighter);
            }
            fight.RedTeam.AddFighter(character.CreateFighter(fight.RedTeam));

            fight.StartPlacement();

            // Guild call for help : online members of the owning guild are
            // notified that their tax collector is under attack.
            var guild = GuildRecord.GetGuilds().FirstOrDefault(x => x.Id == taxCollector.GuildId);

            if (guild != null)
            {
                var attacked = new TaxCollectorAttackedMessage(0, 0, (short)character.Map.Position.X,
                    (short)character.Map.Position.Y, character.Map.Id, (short)character.Map.SubareaId, null);

                foreach (var member in guild.Members)
                {
                    var memberClient = WorldServer.Instance.GetClient(x => x.Character != null && x.Character.Id == member.CharacterId);

                    if (memberClient != null && memberClient.Character.Id != character.Id)
                    {
                        memberClient.Send(attacked);
                        memberClient.Character.ReplyWarning("Le percepteur de votre guilde est attaque sur la map " + character.Map.Id + " !");
                    }
                }
            }
        }

        /// <summary>
        /// Finds the tax collector the player is interacting with on their map.
        /// </summary>
        private static TaxCollector GetCollectorOnMap(WorldClient client)
        {
            return client.Character.Map.Instance.GetEntities<TaxCollector>()
                .FirstOrDefault(x => x.GuildId != 0 && client.Character.HasGuild && x.GuildId == client.Character.Record.GuildId);
        }

        [MessageHandler]
        public static void HandleAddTaxCollectorOrderedSpell(AddTaxCollectorOrderedSpellMessage message, WorldClient client)
        {
            var collector = GetCollectorOnMap(client);
            if (collector == null)
                return;

            if (!collector.OrderedSpells.Contains(message.spell.spellId))
            {
                collector.OrderedSpells.Add(message.spell.spellId);
            }
            client.Send(new TaxCollectorOrderedSpellUpdatedMessage(collector.Id,
                collector.OrderedSpells.Select((x, i) => new TaxCollectorOrderedSpell(x, (byte)i)).ToArray()));
        }

        [MessageHandler]
        public static void HandleRemoveTaxCollectorOrderedSpell(RemoveTaxCollectorOrderedSpellMessage message, WorldClient client)
        {
            var collector = GetCollectorOnMap(client);
            if (collector == null || message.slot >= collector.OrderedSpells.Count)
                return;

            collector.OrderedSpells.RemoveAt(message.slot);
            client.Send(new TaxCollectorOrderedSpellUpdatedMessage(collector.Id,
                collector.OrderedSpells.Select((x, i) => new TaxCollectorOrderedSpell(x, (byte)i)).ToArray()));
        }

        [MessageHandler]
        public static void HandleMoveTaxCollectorOrderedSpell(MoveTaxCollectorOrderedSpellMessage message, WorldClient client)
        {
            var collector = GetCollectorOnMap(client);
            if (collector == null || message.movedFrom >= collector.OrderedSpells.Count || message.movedTo >= collector.OrderedSpells.Count)
                return;

            var spell = collector.OrderedSpells[message.movedFrom];
            collector.OrderedSpells.RemoveAt(message.movedFrom);
            collector.OrderedSpells.Insert(message.movedTo, spell);
            client.Send(new TaxCollectorOrderedSpellUpdatedMessage(collector.Id,
                collector.OrderedSpells.Select((x, i) => new TaxCollectorOrderedSpell(x, (byte)i)).ToArray()));
        }
    }
}
