using Giny.World.Managers.Criterias;
using Giny.World.Managers.Entities.Characters;
using Giny.World.Managers.Generic;
using Giny.World.Managers.Maps.Instances;
using Giny.World.Records.Maps;
using System.Linq;

namespace Giny.World.Managers.Maps.Elements
{
    public abstract class MapElement : IGenericAction
    {
        public InteractiveElementRecord Record
        {
            get;
            private set;
        }

        protected MapInstance MapInstance
        {
            get;
            private set;
        }
        /// <summary>
        /// Skill currently being used on this element.
        /// </summary>
        public InteractiveSkillRecord Skill
        {
            get;
            set;
        }
        public GenericActionEnum ActionIdentifier
        {
            get => Skill.ActionIdentifier;
            set => Skill.ActionIdentifier = value;
        }
        public string Param1
        {
            get => Skill.Param1;
            set => Skill.Param1 = value;
        }
        public string Param2
        {
            get => Skill.Param2;
            set => Skill.Param2 = value;
        }
        public string Param3
        {
            get => Skill.Param3;
            set => Skill.Param3 = value;
        }
        public string Criteria
        {
            get => Skill.Criteria;
            set => Skill.Criteria = value;
        }

        public MapElement(InteractiveElementRecord record, MapInstance mapInstance)
        {
            this.Record = record;
            this.MapInstance = mapInstance;
            this.Skill = record.Skills.FirstOrDefault();
        }

        public virtual bool CanUse(Character character)
        {
            return true;
            /* short[] zone = new Square(0, 1).GetCells(this.Record.CellId, character.Map);
            return zone.Length == 0 || zone.Contains(character.Record.CellId); */
        }
    }
}
