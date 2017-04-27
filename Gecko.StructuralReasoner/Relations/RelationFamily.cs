using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Relations
{
    public class RelationFamily
    {
        public RelationFamily(string name, List<BinaryRelation> relations)
        {
            if (relations == null || relations.Count < 2)
            {
                throw new ArgumentException("At least two relations must be included in a relation family!");
            }

            this.Name = name;
            this.Relations = relations;
        }

        /// <summary>
        /// Human-readable name of the relation family
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// The equals relation of the relation family
        /// </summary>
        public BinaryRelation EqualsRelation { get; set; }

        private List<BinaryRelation> relations;
        /// <summary>
        /// The relations included in the family.
        /// Do not change this after initialization! Otherwise the constraint types for the TMS will not match properly with the relations.
        /// </summary>
        public List<BinaryRelation> Relations
        {
            get
            {
                return relations;
            }
            private set
            {
                // Foreach relation in the list the family should be assigned
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i].RelationFamily != null && value[i].RelationFamily != this)
                    {
                        throw new InvalidOperationException("A relation is assigned to more than one families!");
                    }

                    value[i].IdInCalculus = i;
                    value[i].RelationFamily = this;
                }
                relations = value;
            }
        }

    }
}
