using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.ConstraintRepresentation;

namespace Gecko.StructuralReasoner
{
    public class ConfigurationConstraint
    {
        //private Relation activeRelation;

        public ConfigurationConstraint(DomainConstraint domainConstraint)
        {
            if (domainConstraint == null)
            {
                throw new ArgumentNullException("The domain constraint is not set!");
            }

            this.DomainConstraint = domainConstraint;
        }

        public DomainConstraint DomainConstraint { get; private set; }

        ///// <summary>
        ///// The current active relation
        ///// </summary>
        //public Relation ActiveRelation
        //{
        //    get
        //    {
        //        return activeRelation;
        //    }
        //    set
        //    {
        //        if (!allowedRelations.Contains(value))
        //        {
        //            throw new InvalidOperationException("The active relation cannot be set to a relation not in the list of allowed ones!");
        //        }
        //        else
        //        {
        //            activeRelation = value;
        //        }
        //    }
        //}

        // Rem: The property just takes the relation family of the first allowed relation

        /// <summary>
        /// The relation family of the allowed relations.
        /// </summary>
        public RelationFamily RelationFamily
        {
            get
            {
                return (AllowedRelations != null && AllowedRelations.Count > 0) ? AllowedRelations[0].RelationFamily : null;
            }
        }

        /// <summary>
        /// Specifies whether the constraint is soft or hard
        /// </summary>
        public bool CanBeViolated
        {
            get
            {
                return this.DomainConstraint.CanBeViolated;
            }
        }

        private ConfigurationConstraintTree owningTree;
        /// <summary>
        /// The direct ConfigurationConstraintTree owning the configuration constraint
        /// </summary>
        public ConfigurationConstraintTree OwningTree
        {
            get { return owningTree; }
            set { owningTree = value; }
        }

        /// <summary>
        /// A constraint equal to the current one, which is active in a constraint network.
        /// If this is not set either the configuration constraint is not part of a constraint network or it is active in its network
        /// </summary>
        public ConfigurationConstraint EqualActiveConstraint { get; set; }

        /// <summary>
        /// The root of the ConfigurationConstraintTree owning the configuration constraint
        /// </summary>
        public ConfigurationConstraintTree OwningTreeRoot
        {
            get
            {
                ConfigurationConstraintTree root = this.OwningTree;

                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                return root;
            }
        }

        private List<BinaryRelation> allowedRelations;
        /// <summary>
        /// The possible relations between the relation parts.
        /// This is the domain of the ActiveRelation
        /// </summary>
        public List<BinaryRelation> AllowedRelations
        {
            get
            {
                if (allowedRelations == null)
                {
                    allowedRelations = new List<BinaryRelation>();
                }

                return allowedRelations;
            }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("The list of allowed relations cannot be null!");
                }
                if (value.GroupBy(x => x.RelationFamily).Count() > 1 ||
                    (value.Count() > 1 && value.Any(x => x.RelationFamily == null)))
                {
                    throw new InvalidOperationException("Cannot assign a list of allowed relations for a constraint when they do not have a common relations family!");
                }
                else
                {
                    allowedRelations = value;
                }
            }
        }

        /// <summary>
        /// The parts of the relation. They specify the values of the relation components
        /// </summary>
        public List<IRelationPart> RelationParts { get; private set; }

        public void SetRelationParts(List<IRelationPart> relationParts)
        {
            for (int i = 0; i < relationParts.Count; i++)
            {
                if (this.AllowedRelations.Any(x => relationParts[i].GetType() != x.Signature[i]))
                {
                    throw new ArgumentException("The provided relation components do not match the relation signatures!");
                }
            }

            this.RelationParts = relationParts;
        }

        /// <summary>
        /// Gets an ordered list of nodes implied by the configuration constraint. Metric relaition parts are excluded
        /// </summary>
        /// <returns></returns>
        public List<Node> GetIncludedNodes()
        {
            List<Node> result = new List<Node>();

            foreach (var relPart in this.RelationParts)
            {
                if (relPart is ComponentRelationPart)
                {
                    result.Add((relPart as ComponentRelationPart).ToNode());
                }
                else if (relPart is AttributeRelationPart)
                {
                    result.Add((relPart as AttributeRelationPart).ToNode());
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns the values of the metric parts using special values (e.g. number of components in the context)
        /// </summary>
        /// <param name="context">The structuring context including the constraint</param>
        internal void AssignSpecialMetricValues(GKOStructuringContext context)
        {
            foreach (var part in this.RelationParts)
            {
                if (part is MetricRelationPart)
                {
                    MetricRelationPart asMetricPart = part as MetricRelationPart;
                    if (asMetricPart.IsSpecialValue)
                    {
                        asMetricPart.AssignFromSpecialValue(context);
                    }
                }
            }
        }

        public override bool Equals(System.Object obj)
        {
            ConfigurationConstraint compared;

            if (obj == null)
            {
                return false;
            }

            compared = obj as ConfigurationConstraint;
            if ((System.Object)compared == null)
            {
                return false;
            }

            return this.Equals(compared);
        }

        public bool Equals(ConfigurationConstraint compared)
        {
            if (compared == null)
            {
                return false;
            }

            if (this.RelationParts.Count != compared.RelationParts.Count)
            {
                return false;
            }

            for (int i = 0; i < this.RelationParts.Count; i++)
            {
                if (this.RelationParts[i].GetType().Equals(compared.RelationParts[i].GetType()))
                {
                    if (this.RelationParts[i] is MetricRelationPart && !(this.RelationParts[i] as MetricRelationPart).Equals(compared.RelationParts[i]))
                    {
                        return false;
                    }
                    if (this.RelationParts[i] is AttributeRelationPart && !(this.RelationParts[i] as AttributeRelationPart).Equals(compared.RelationParts[i]))
                    {
                        return false;
                    }
                    if (this.RelationParts[i] is ComponentRelationPart && !(this.RelationParts[i] as ComponentRelationPart).Equals(compared.RelationParts[i]))
                    {
                        return false;
                    }
                }
            }

            return this.AllowedRelations.Intersect(compared.AllowedRelations).Count() == this.AllowedRelations.Count && this.DomainConstraint.CanBeViolated == compared.DomainConstraint.CanBeViolated;
        }

        public string ToString()
        {
            StringBuilder description = new StringBuilder();

            description.AppendFormat("{0} ({1} constraint):", this.DomainConstraint.Name, this.DomainConstraint.CanBeViolated ? "soft" : "hard");
            description.AppendLine();

            for (int i = 0; i < this.allowedRelations.Count; i++)
            {
                description.AppendLine(this.allowedRelations[i].ToString(this));
                if (i != this.allowedRelations.Count - 1)
                {
                    description.AppendLine("OR");
                }
            }

            return description.ToString();
        }

        //public override int GetHashCode()
        //{
        //    // Overflow is allowed
        //    unchecked
        //    {
        //        int hash = 17;
        //        // Suitable nullity checks etc, of course :)
        //        hash = hash * 23 + this.Component.GetHashCode();
        //        if (this.Attribute != null)
        //        {
        //            hash = hash * 23 + this.Attribute.GetHashCode();
        //        }
        //        return hash;
        //    }
        //}
    }
}
