using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    /// <summary>
    /// Point Algebra to Metric domain constraint.
    /// </summary>
    public class PaToMetricDomainConstraint : DomainConstraint, ITransformDomainConstraint
    {
        /// <summary>
        /// Cretes a new domain constraint which constraints which requires a relation between the matched components.
        /// </summary>
        /// <param name="name">The unique name of the domain constraint.</param>
        /// <param name="requiredRelation">The relation between the matched components.</param>
        /// <param name="canBeViolated">Set to true for soft constraint, false for hard constraint</param>
        /// <param name="comp1Attribute">The attribute to constraint for the first component</param>
        /// <param name="comp2Attribute">The attribute to constraint for the second component</param>
        public PaToMetricDomainConstraint(string name, BinaryRelation requiredRelation, bool canBeViolated, GKOAttribute comp1Attribute, GKOAttribute comp2Attribute)
            : base(name, requiredRelation, canBeViolated)
        {
            if (requiredRelation.RelationFamily != StructuralRelationsManager.IntervalAlgebra)
            {
                throw new ArgumentNullException("The required relation has to be from the Interval Algebra!");
            }
            if (comp1Attribute == null || comp2Attribute == null)
            {
                throw new ArgumentNullException("All attributes should be set!");
            }

            this.Comp1Attribute = comp1Attribute;
            this.Comp2Attribute = comp2Attribute;
        }

        public GKOAttribute Comp1Attribute { get; set; }
        public GKOAttribute Comp2Attribute { get; set; }

        /// <summary>
        /// Should not be used!
        /// </summary>
        /// <param name="componentsList"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal override List<ConfigurationConstraint> GenerateConfigurationConstraints(List<GKOComponent> componentsList, Log log)
        {
            throw new NotImplementedException();
        }

        public DomainConstraint TransformConstraint()
        {
            DomainConstraint metricDomainConstraint = null;
            QualitativeRelation requiredRelation = this.allowedRelations[0] as QualitativeRelation;

            // The metric relations that might be needed
            BinaryRelation lessThanRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.LessThan);
            BinaryRelation greaterThanRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.GreaterThan);
            BinaryRelation equalsRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.Equals);

            switch (requiredRelation.Name)
            {
                case PointsAlgebraRelationNames.After:
                    // A After B	 |  A.Attr > B.Attr
                    metricDomainConstraint = new DomainConstraint(this.Name, greaterThanRelation, this.CanBeViolated);
                    break;
                case PointsAlgebraRelationNames.Before:
                    // A Before B	 |  A.Attr < B.Attr
                    metricDomainConstraint = new DomainConstraint(this.Name, lessThanRelation, this.CanBeViolated);
                    break;
                case PointsAlgebraRelationNames.Equals:
                    // A Equals B	 |  A.Attr = B.Attr
                    metricDomainConstraint = new DomainConstraint(this.Name, equalsRelation, this.CanBeViolated);
                    break;
            }

            // Setting the relation parts
            if (metricDomainConstraint != null)
            {
                AttributeDomainRelationPart attrComp1Part = this.GenerateRelationPart(this.DomainRelationParts[0], this.Comp1Attribute);
                AttributeDomainRelationPart attrComp2Part = this.GenerateRelationPart(this.DomainRelationParts[1], this.Comp2Attribute);

                metricDomainConstraint.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1Part, attrComp2Part };
            }

            return metricDomainConstraint;
        }

        /// <summary>
        /// Finds the AttributeDomainRelationPart for a specific attribute and IRelationPart
        /// </summary>
        /// <param name="relPart">The relation part for which to generate the attribute rel part</param>
        /// <param name="attribute">The attribute to apply</param>
        /// <returns></returns>
        private AttributeDomainRelationPart GenerateRelationPart(IDomainRelationPart relPart, GKOAttribute attribute)
        {
            AttributeDomainRelationPart attributePart;

            if (relPart is ComponentDomainRelationPart)
            {
                attributePart = new AttributeDomainRelationPart((relPart as ComponentDomainRelationPart).Filter, attribute);
            }
            else if (relPart is AttributeDomainRelationPart)
            {
                attributePart = (relPart as AttributeDomainRelationPart);
                attributePart.Attribute = attribute;
            }
            else
            {
                throw new InvalidOperationException(string.Format("The Point Algebra to metric domain constraint '{0}' has invalid relation parts", this.Name));
            }

            return attributePart;
        }

    }
}
