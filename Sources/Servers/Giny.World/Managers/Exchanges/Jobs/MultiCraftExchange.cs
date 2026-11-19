using Giny.Core.Network.Messages;
using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.Protocol.Messages;
using Giny.Protocol.Types;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Formulas;
using Giny.World.Managers.Items;
using Giny.World.Managers.Items.Collections;
using Giny.World.Records.Characters;
using Giny.World.Records.Items;
using Giny.World.Records.Jobs;
using Giny.World.Records.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Exchanges.Jobs
{
    /// <summary>
    /// Cooperative craft exchange. The customer provides the ingredients
    /// (and an optional payment), the crafter performs the craft with his job.
    /// </summary>
    public class MultiCraftExchange : Exchange
    {
        public override ExchangeTypeEnum ExchangeType => CrafterView ? ExchangeTypeEnum.MULTICRAFT_CRAFTER : ExchangeTypeEnum.MULTICRAFT_CUSTOMER;

        private MultiCraftSession Session
        {
            get;
            set;
        }
        private Character Other
        {
            get;
            set;
        }
        private bool CrafterView
        {
            get;
            set;
        }

        private MultiCraftExchange(Character character, Character other, MultiCraftSession session, bool crafterView) : base(character)
        {
            this.Other = other;
            this.Session = session;
            this.CrafterView = crafterView;
        }

        public static MultiCraftExchange[] Create(Character crafter, Character customer, SkillRecord skill)
        {
            MultiCraftSession session = new MultiCraftSession(crafter, customer, skill);

            return new MultiCraftExchange[]
            {
                new MultiCraftExchange(crafter, customer, session, true),
                new MultiCraftExchange(customer, crafter, session, false),
            };
        }

        public override void Open()
        {
            if (CrafterView)
            {
                Character.Client.Send(new ExchangeStartOkMulticraftCrafterMessage((int)Session.Skill.Id));
                Character.Client.Send(new ExchangeOkMultiCraftMessage(Session.Crafter.Id, Session.Customer.Id, 1));
            }
            else
            {
                Character.Client.Send(new ExchangeStartOkMulticraftCustomerMessage((int)Session.Skill.Id, (byte)Session.CrafterJob.Level));
                Character.Client.Send(new ExchangeOkMultiCraftMessage(Session.Crafter.Id, Session.Customer.Id, 0));
            }
        }

        public override void Close()
        {
            Send(new ExchangeLeaveMessage()
            {
                dialogType = (byte)DialogType,
                success = Succes,
            });

            Session.Crafter.Dialog = null;
            Session.Customer.Dialog = null;
        }

        private void Send(NetworkMessage message)
        {
            Session.Crafter.Client.Send(message);
            Session.Customer.Client.Send(message);
        }

        public override void MoveItem(int uid, int quantity)
        {
            if (CrafterView)
            {
                if (Session.CrafterCanUseResources)
                {
                    Session.CrafterItems.MoveItem(uid, quantity);
                }
            }
            else
            {
                Session.CustomerItems.MoveItem(uid, quantity);
            }
        }

        public override void MoveKamas(long quantity)
        {
            if (!CrafterView && quantity <= Character.Record.Kamas)
            {
                Session.Payment = quantity;
                Send(new ExchangeCraftPaymentModifiedMessage(quantity));
            }
        }

        public void SetCrafterCanUseResources(bool allow)
        {
            Session.CrafterCanUseResources = allow;

            if (!allow)
            {
                Session.CrafterItems.Clear();
            }

            Send(new ExchangeMultiCraftCrafterCanUseHisRessourcesMessage(allow));
        }

        public override void Ready(bool ready, short step)
        {
            if (!CrafterView || !ready)
            {
                return;
            }

            IEnumerable<CharacterItemRecord> items = Session.CustomerItems.GetItems();

            if (Session.CrafterCanUseResources)
            {
                items = items.Concat(Session.CrafterItems.GetItems());
            }

            int[] gids = items.Select(x => x.GId).ToArray();
            int[] quantities = items.Select(x => x.Quantity).ToArray();

            RecipeRecord recipe = RecipeRecord.GetRecipeRecord(gids, quantities);

            if (recipe == null || recipe.JobId != Session.CrafterJob.JobId || recipe.SkillId != Session.Skill.Id)
            {
                OnCraftResulted(CraftResultEnum.CRAFT_FAILED);
                return;
            }

            if (recipe.ResultRecord.Level > Session.CrafterJob.Level)
            {
                OnCraftResulted(CraftResultEnum.CRAFT_FAILED);
                return;
            }

            PerformCraft(recipe, Session.CustomerItems.GetItems());
        }

        private void PerformCraft(RecipeRecord recipe, IEnumerable<CharacterItemRecord> customerItems)
        {
            Dictionary<int, int> customerRemoved = Session.CustomerItems.GetItems().ToDictionary(x => x.UId, x => x.Quantity);
            Dictionary<int, int> crafterRemoved = new Dictionary<int, int>();

            if (Session.CrafterCanUseResources)
            {
                crafterRemoved = Session.CrafterItems.GetItems().ToDictionary(x => x.UId, x => x.Quantity);
            }

            Session.Customer.Inventory.RemoveItems(customerRemoved);
            Session.Crafter.Inventory.RemoveItems(crafterRemoved);

            CharacterItemRecord result = ItemsManager.Instance.CreateCharacterItem(recipe.ResultRecord, Session.Customer.Id, 1);
            Session.Customer.Inventory.AddItem(result);

            if (Session.Payment > 0)
            {
                Session.Customer.RemoveKamas(Session.Payment);
                Session.Crafter.AddKamas(Session.Payment);
                Session.Payment = 0;
            }

            int exp = JobFormulas.Instance.GetCraftExperience(recipe.ResultRecord.Level, Session.CrafterJob.Level, recipe.ResultRecord.CraftXpRatio);
            Session.Crafter.AddJobExp(Session.CrafterJob.JobId, exp);

            Session.CustomerItems.Clear();
            Session.CrafterItems.Clear();

            OnCraftResulted(CraftResultEnum.CRAFT_SUCCESS, result.GetObjectItemNotInContainer());
        }

        private void OnCraftResulted(CraftResultEnum result)
        {
            Send(new ExchangeCraftResultMessage((byte)result));
        }
        private void OnCraftResulted(CraftResultEnum result, ObjectItemNotInContainer item)
        {
            Send(new ExchangeCraftResultWithObjectDescMessage()
            {
                craftResult = (byte)result,
                objectInfo = item,
            });
        }

        public override void MoveItemPriced(int objectUID, int quantity, long price)
        {
            throw new NotImplementedException();
        }

        public override void ModifyItemPriced(int objectUID, int quantity, long price)
        {
            throw new NotImplementedException();
        }

        public override void OnNpcGenericAction(NpcActionsEnum action)
        {
            throw new NotImplementedException();
        }

        private class MultiCraftSession
        {
            public Character Crafter;
            public Character Customer;
            public SkillRecord Skill;
            public CharacterJob CrafterJob;
            public MultiCraftItemCollection CustomerItems;
            public MultiCraftItemCollection CrafterItems;
            public long Payment;
            public bool CrafterCanUseResources;

            public MultiCraftSession(Character crafter, Character customer, SkillRecord skill)
            {
                this.Crafter = crafter;
                this.Customer = customer;
                this.Skill = skill;
                this.CrafterJob = crafter.GetJob(skill.ParentJobId);
                this.CustomerItems = new MultiCraftItemCollection(customer, crafter);
                this.CrafterItems = new MultiCraftItemCollection(crafter, customer);
            }
        }
    }
}
