using Giny.Protocol.Messages;
using Giny.Protocol.Types;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Maps.Instances;
using Giny.World.Records.Maps;
using System.Linq;

namespace Giny.World.Managers.Maps.Elements
{
    public class MapInteractiveElement : MapElement
    {
        public MapInteractiveElement(MapInstance mapInstance, InteractiveElementRecord record) : base(record, mapInstance)
        {

        }

        public void Update()
        {
            foreach (var character in MapInstance.GetEntities<Character>())
            {
                character.Client.Send(new InteractiveElementUpdatedMessage(GetInteractiveElement(character)));
            }
        }
        protected virtual bool IsSkillEnabled(Character character, InteractiveSkillRecord skill)
        {
            return character.SkillsAllowed.Contains(skill.Record);
        }
        public InteractiveElement GetInteractiveElement(Character character)
        {
            var enabledSkills = Record.Skills.Where(x => IsSkillEnabled(character, x))
                .Select(x => new InteractiveElementSkill((int)x.SkillId, (int)x.Id)).ToArray();

            var disabledSkills = Record.Skills.Where(x => !IsSkillEnabled(character, x))
                .Select(x => new InteractiveElementSkill((int)x.SkillId, (int)x.Id)).ToArray();

            var elementType = Record.Skills.Count > 0 ? (int)Record.Skills[0].Type : -1;

            return new InteractiveElement((int)Record.Identifier, elementType, enabledSkills, disabledSkills, true);
        }
    }
}
