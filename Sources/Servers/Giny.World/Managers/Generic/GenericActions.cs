using Giny.ORM;
using Giny.Protocol.Messages;
using Giny.Protocol.Types;
using Giny.World.Managers.Dialogs;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Fights;
using Giny.World.Managers.Maps.Elements;
using Giny.World.Managers.Monsters;
using Giny.World.Records.Bidshops;
using Giny.World.Records.Houses;
using Giny.World.Records.Characters;
using Giny.World.Records.Items;
using Giny.World.Records.Maps;
using Giny.World.Records.Monsters;
using Giny.World.Records.Npcs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Generic
{
    public class GenericActions
    {
        [GenericActionHandler(GenericActionEnum.ContinueDialog)]
        public static void HandleContinueDialog(Character character, IGenericAction parameter)
        {
            var messageId = int.Parse(parameter.Param1);

            if (character.IsInDialog<NpcTalkDialog>())
            {
                var dialog = character.GetDialog<NpcTalkDialog>();
                dialog.Npc.ContinueDialoging(character, messageId);
            }
        }
        [GenericActionHandler(GenericActionEnum.Unhandled)]
        public static void HandleUnhandled(Character character, IGenericAction parameter)
        {
            MapInteractiveElement element = parameter as MapInteractiveElement;

            if (element != null)
            {


                if (character.Client.Account.Role == Protocol.Custom.Enums.ServerRoleEnum.Administrator)
                {
                    character.ReplyWarning("This action is not handled. ElementId : " + element.Record.Identifier + " Bones : " + element.Record.BonesId);
                }
                else
                {
                    character.DisplayNotification("Oups ! On dirait que cet élement interactif n'est pas encore géré ! Si tu as besoin de l'utiliser, indique le dans le canal discord #elements. Bon jeu !");
                }
            }
        }
        [GenericActionHandler(GenericActionEnum.StartQuest)]
        public static void HandleStartQuest(Character character, IGenericAction parameter)
        {
            character.StartQuest(long.Parse(parameter.Param1));
        }
        [GenericActionHandler(GenericActionEnum.AddItem)]
        public static void HandleAddItem(Character character, IGenericAction parameter)
        {
            short itemId = short.Parse(parameter.Param1);
            int quantity = int.Parse(parameter.Param2);
            character.Inventory.AddItem(itemId, quantity);
            character.NotifyItemGained(itemId, quantity);
        }
        [GenericActionHandler(GenericActionEnum.RemoveItem)]
        public static void HandleRemoveItem(Character character, IGenericAction parameter)
        {
            short itemId = short.Parse(parameter.Param1);
            int quantity = int.Parse(parameter.Param2);

            CharacterItemRecord item = character.Inventory.GetFirstItem(itemId, quantity);

            if (item == null)
            {
                character.ReplyWarning("Unable to remove item to character.");
                return;
            }

            character.Inventory.RemoveItem(item.UId, quantity);
            character.NotifyItemLost(itemId, quantity);
        }
        [GenericActionHandler(GenericActionEnum.Teleport)]
        public static void HandleTeleportAction(Character character, IGenericAction parameter)
        {
            short cellId = -1;
            if (short.TryParse(parameter.Param2, out cellId))
            {
                character.Teleport(int.Parse(parameter.Param1), cellId);
            }
            else
            {
                character.Teleport(int.Parse(parameter.Param1));
            }
        }
        [GenericActionHandler(GenericActionEnum.OpenBank)]
        public static void HandleOpenBank(Character character, IGenericAction parameter)
        {
            character.OpenBank();
        }
        [GenericActionHandler(GenericActionEnum.EnterDreams)]
        public static void HandleEnterDreams(Character character, IGenericAction parameter)
        {
            Dreams.DreamsManager.Instance.EnterDreams(character);
        }

        [GenericActionHandler(GenericActionEnum.ExitDreams)]
        public static void HandleExitDreams(Character character, IGenericAction parameter)
        {
            Dreams.DreamsManager.Instance.ExitDreams(character);
        }

        [GenericActionHandler(GenericActionEnum.UsePortal)]
        public static void HandleUsePortal(Character character, IGenericAction parameter)
        {
            short cellId;
            if (short.TryParse(parameter.Param2, out cellId))
                character.Teleport(int.Parse(parameter.Param1), cellId);
            else
                character.Teleport(int.Parse(parameter.Param1));
        [GenericActionHandler(GenericActionEnum.EnterHouse)]
        public static void HandleEnterHouse(Character character, IGenericAction parameter)
        {
            var house = HouseRecord.GetHouse(long.Parse(parameter.Param1));

            if (house == null || (house.OwnerId != 0 && house.OwnerId != character.Id && !CanEnterAsGuildmate(character, house)))
            {
                character.ReplyWarning("Vous ne pouvez pas entrer dans cette maison.");
                return;
            }
            short cellId;
            if (short.TryParse(parameter.Param3, out cellId))
                character.Teleport(int.Parse(parameter.Param2), cellId);
            else
                character.Teleport(int.Parse(parameter.Param2));
        }
        /// <summary>
        /// Guild house : a guildmate can enter a guild-shared house.
        /// </summary>
        private static bool CanEnterAsGuildmate(Character character, HouseRecord house)
        {
            if (!house.GuildShared || !character.HasGuild)
            {
                return false;
            }
            var owner = CharacterRecord.GetCharacterRecord(house.OwnerId);
            return owner != null && owner.GuildId == character.Record.GuildId;
        }

        [GenericActionHandler(GenericActionEnum.BuyHouse)]
        public static void HandleBuyHouse(Character character, IGenericAction parameter)
        {
            var house = HouseRecord.GetHouse(long.Parse(parameter.Param1));

            if (house == null)
                return;

            character.CurrentHouseId = house.Id;

            character.Client.Send(new HousePropertiesMessage((int)house.Id, new int[0],
                new HouseInstanceInformations()
                {
                    hasOwner = house.OwnerId != 0,
                    isLocked = true,
                    price = house.Price,
                    secondHand = false,
                }));
        }
        [GenericActionHandler(GenericActionEnum.SellHouse)]
        public static void HandleSellHouse(Character character, IGenericAction parameter)
        {
            var house = HouseRecord.GetHouse(long.Parse(parameter.Param1));

            if (house == null || house.OwnerId != character.Id)
                return;

            long price;
            if (long.TryParse(parameter.Param2, out price) && price > 0)
            {
                house.Price = price;
            }
            house.OnSale = true;
            house.UpdateLater();
            character.Reply("Votre maison est en vente.");
        }
        [GenericActionHandler(GenericActionEnum.Collect)]
        public static void HandleCollect(Character character, IGenericAction parameter)
        {
            MapStatedElement element = parameter as MapStatedElement;

            if (element == null)
            {
                character.ReplyWarning("Unable to collect. Invalid interactive element.");
            }

            element.Use(character);
        }
        [GenericActionHandler(GenericActionEnum.Bidshop)]
        public static void HandleBidshop(Character character, IGenericAction parameter)
        {
            BidShopRecord record = BidShopRecord.GetBidShop(int.Parse(parameter.Param1));
            character.OpenBuyExchange(record);
        }
        [GenericActionHandler(GenericActionEnum.Zaap)]
        public static void HandleZaap(Character character, IGenericAction parameter)
        {
            character.OpenZaap((MapElement)parameter);
        }
        [GenericActionHandler(GenericActionEnum.Zaapi)]
        public static void HandleZaapi(Character character, IGenericAction parameter)
        {
            character.OpenZaapi((MapElement)parameter);
        }
        [GenericActionHandler(GenericActionEnum.LearnOrnament)]
        public static void HandleLearnOrnament(Character character, IGenericAction parameter)
        {
            character.LearnOrnament(short.Parse(parameter.Param1), true);
        }
        [GenericActionHandler(GenericActionEnum.LearnTitle)]
        public static void HandleLearnTitle(Character character, IGenericAction parameter)
        {
            character.LearnTitle(short.Parse(parameter.Param1));
        }

        [GenericActionHandler(GenericActionEnum.CreateGuild)]
        public static void HandleCreateGuild(Character character, IGenericAction parameter)
        {
            character.OpenGuildCreationDialog();
        }
        [GenericActionHandler(GenericActionEnum.LearnSpell)]
        public static void HandleLearnSpell(Character character, IGenericAction parameter)
        {
            character.LearnSpell(short.Parse(parameter.Param1), true);
        }
        [GenericActionHandler(GenericActionEnum.Paddock)]
        public static void HandlePaddock(Character character, IGenericAction parameter)
        {
            character.StableMountInPaddock();
        }
        [GenericActionHandler(GenericActionEnum.AddKamas)]
        public static void HandleAddKamas(Character character, IGenericAction parameter)
        {
            long amount = long.Parse(parameter.Param1);
            character.AddKamas(amount);
            character.OnKamasGained(amount);
        }

        [GenericActionHandler(GenericActionEnum.Craft)]
        public static void HandleCraft(Character character, IGenericAction parameter)
        {
            MapInteractiveElement element = parameter as MapInteractiveElement;

            if (element == null)
            {
                throw new Exception("Unable to craft. Invalid generic parameter.");
            }

            character.OpenCraftExchange(element.Skill.Record);
        }
        [GenericActionHandler(GenericActionEnum.Smithmagic)]
        public static void HandlSmithmagic(Character character, IGenericAction parameter)
        {
            MapInteractiveElement element = parameter as MapInteractiveElement;

            if (element == null)
            {
                throw new Exception("Unable to craft. Invalid generic parameter.");
            }

            character.OpenSmithmagicExchange(element.Skill.Record);
        }

        [GenericActionHandler(GenericActionEnum.RuneTrade)]
        public static void HandleRuneTrade(Character character, IGenericAction parameter)
        {
            character.OpenRuneTradeExchange();
        }

        [GenericActionHandler(GenericActionEnum.AddExperience)]
        public static void HandleAddExperience(Character character, IGenericAction parameter)
        {
            character.AddExperience(long.Parse(parameter.Param1), true);
        }

        [GenericActionHandler(GenericActionEnum.Notification)]
        public static void HandleNotification(Character character, IGenericAction parameter)
        {
            character.DisplayNotification(parameter.Param1);
        }

        [GenericActionHandler(GenericActionEnum.Fight)]
        public static void HandleFight(Character character, IGenericAction parameter)
        {
            IEnumerable<MonsterRecord> records = parameter.Param1.Split(',').Select(x => MonsterRecord.GetMonsterRecord(short.Parse(x)));

            if (records.Count() > 0)
            {
                FightContextual fight = FightManager.Instance.CreateFightContextual(character);

                var cell = character.Map.RandomWalkableCell();

                foreach (var record in records)
                {
                    Monster monster = new Monster(record, cell, MonstersManager.Instance.GetAdaptativeGrade(record, character.SafeLevel));

                    fight.BlueTeam.AddFighter(monster.CreateFighter(fight.BlueTeam));
                }

                fight.RedTeam.AddFighter(character.CreateFighter(fight.RedTeam));

                fight.StartPlacement();

            }
            else
            {
                character.ReplyWarning("Unable to create contextual fight. Empty monsters list.");
            }
        }

    }
}
