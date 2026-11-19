using Giny.World.Records.Spells;
﻿using Giny.World.Managers.Fights.Fighters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Fights.AI
{
    public class MonsterBrain
    {
        private AIFighter Fighter
        {
            get;
            set;
        }

        private SummonAction SummonAction
        {
            get;
            set;
        }
        private MarkAction MarkAction
        {
            get;
            set;
        }
        private BuffAction BuffAction
        {
            get;
            set;
        }
        private HealAction HealAction
        {
            get;
            set;
        }
        private MoveToTarget MoveToTarget
        {
            get;
            set;
        }
        private CastOnEnemyAction CastOnEnemyAction
        {
            get;
            set;
        }
        private FleeAction FleeAction
        {
            get;
            set;
        }
        public MonsterBrain(AIFighter fighter)
        {
            this.Fighter = fighter;
            this.SummonAction = new SummonAction(Fighter);
            this.MarkAction = new MarkAction(Fighter);
            this.BuffAction = new BuffAction(Fighter);
            this.HealAction = new HealAction(Fighter);
            this.MoveToTarget = new MoveToTarget(Fighter);
            this.CastOnEnemyAction = new CastOnEnemyAction(Fighter);
            this.FleeAction = new FleeAction(Fighter);
        }


        /// <summary>
        /// AI archetype deduced from the monster spellbook categories.
        /// </summary>
        private enum MonsterArchetype
        {
            Melee,      // short range : closes in before casting
            Kiter,      // long range : casts then keeps distance
            Healer,     // healing spells : prioritizes heals/buffs, does not flee
            Summoner,   // summon heavy : summons first, stays away
        }

        private MonsterArchetype? m_archetype;

        private MonsterArchetype Archetype
        {
            get
            {
                if (m_archetype == null)
                {
                    var spells = Fighter.GetSpells().ToArray();
                    var categories = spells.Aggregate(SpellCategoryEnum.None, (acc, x) => acc | x.Category);

                    if (categories.HasFlag(SpellCategoryEnum.Healing))
                    {
                        m_archetype = MonsterArchetype.Healer;
                    }
                    else if (categories.HasFlag(SpellCategoryEnum.Summon) && !categories.HasFlag(SpellCategoryEnum.Agressive))
                    {
                        m_archetype = MonsterArchetype.Summoner;
                    }
                    else if (spells.Where(x => x.Category.HasFlag(SpellCategoryEnum.Agressive))
                        .Any(x => x.Levels.Max(l => l.MaxRange) >= 8))
                    {
                        m_archetype = MonsterArchetype.Kiter;
                    }
                    else
                    {
                        m_archetype = MonsterArchetype.Melee;
                    }
                }
                return m_archetype.Value;
            }
        }

        public void Play()
        {
            switch (Archetype)
            {
                case MonsterArchetype.Healer:
                    // Support : heals and buffs first, stays with allies (no flee).
                    HealAction.Execute();
                    BuffAction.Execute();
                    SummonAction.Execute();
                    MarkAction.Execute();
                    CastOnEnemyAction.Execute();
                    MoveToTarget.Execute();
                    break;
                case MonsterArchetype.Summoner:
                    SummonAction.Execute();
                    BuffAction.Execute();
                    MarkAction.Execute();
                    CastOnEnemyAction.Execute();
                    MoveToTarget.Execute();
                    break;
                case MonsterArchetype.Kiter:
                    // Long range : casts then moves away, never closes in.
                    CastOnEnemyAction.Execute();
                    FleeAction.Execute();
                    break;
                default:
                    SummonAction.Execute();
                    MarkAction.Execute();
                    BuffAction.Execute();
                    CastOnEnemyAction.Execute();
                    MoveToTarget.Execute();
                    FleeAction.Execute();
                    break;
            }
        }
    }
}