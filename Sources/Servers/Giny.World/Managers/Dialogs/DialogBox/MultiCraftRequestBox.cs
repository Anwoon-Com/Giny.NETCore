using Giny.Protocol.Custom.Enums;
using Giny.Protocol.Enums;
using Giny.Protocol.Messages;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Exchanges.Jobs;
using Giny.World.Records.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Dialogs.DialogBox
{
    public class MultiCraftRequestBox : RequestBox
    {
        private int SkillId
        {
            get;
            set;
        }

        public MultiCraftRequestBox(Character source, Character target, int skillId) : base(source, target)
        {
            this.SkillId = skillId;
        }

        protected override void OnOpen()
        {
            this.Send(new ExchangeRequestedTradeMessage(Source.Id, Target.Id, (byte)ExchangeTypeEnum.MULTICRAFT_CRAFTER));
        }
        protected override void OnAccept()
        {
            SkillRecord skill = SkillRecord.GetSkill((SkillTypeEnum)SkillId);

            var exchanges = MultiCraftExchange.Create(Target, Source, skill);

            Source.Dialog = exchanges[0];
            Target.Dialog = exchanges[1];

            Source.Dialog.Open();
            Target.Dialog.Open();
        }
        protected override void OnDeny()
        {
            Send(new ExchangeLeaveMessage(false, (byte)DialogTypeEnum.DIALOG_EXCHANGE));
        }
        protected override void OnCancel()
        {
            base.Deny();
        }
    }
}
