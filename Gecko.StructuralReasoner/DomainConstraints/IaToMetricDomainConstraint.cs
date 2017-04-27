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
    /// Interval Algebra to Metric domain constraint.
    /// </summary>
    public class IaToMetricDomainConstraint : DomainConstraint, ITransformDomainConstraint
    {
        /// <summary>
        /// Cretes a new domain constraint which constraints which requires a relation between the matched components.
        /// </summary>
        /// <param name="name">The unique name of the domain constraint.</param>
        /// <param name="requiredRelation">The relation between the matched components.</param>
        /// <param name="canBeViolated">Set to true for soft constraint, false for hard constraint</param>
        /// <param name="comp1IntervalStart">The attribute for the start of the first interval</param>
        /// <param name="comp1IntervalEnd">The attribute for the end of the first interval</param>
        /// <param name="comp2IntervalStart">The attribute for the start of the second interval</param>
        /// <param name="comp2IntervalEnd">The attribute for the end of the second interval</param>
        public IaToMetricDomainConstraint(string name, BinaryRelation requiredRelation, bool canBeViolated, GKOAttribute comp1IntervalStart, GKOAttribute comp1IntervalEnd, GKOAttribute comp2IntervalStart, GKOAttribute comp2IntervalEnd)
            : base(name, requiredRelation, canBeViolated)
        {
            if (requiredRelation.RelationFamily != StructuralRelationsManager.IntervalAlgebra)
            {
                throw new ArgumentNullException("The required relation has to be from the Interval Algebra!");
            }
            if (comp1IntervalStart == null || comp2IntervalStart == null || comp1IntervalEnd == null || comp2IntervalEnd == null)
            {
                throw new ArgumentNullException("All interval attributes should be set!");
            }

            this.Comp1IntervalStart = comp1IntervalStart;
            this.Comp1IntervalEnd = comp1IntervalEnd;
            this.Comp2IntervalStart = comp2IntervalStart;
            this.Comp2IntervalEnd = comp2IntervalEnd;
        }

        public GKOAttribute Comp1IntervalStart { get; set; }
        public GKOAttribute Comp1IntervalEnd { get; set; }
        public GKOAttribute Comp2IntervalStart { get; set; }
        public GKOAttribute Comp2IntervalEnd { get; set; }

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
            List<DomainConstraint> metricDomainConstraints = new List<DomainConstraint>();
            DomainConstraint constr1 = null;
            DomainConstraint constr2 = null;
            QualitativeRelation requiredRelation = this.allowedRelations[0] as QualitativeRelation;

            // The metric relations that might be needed
            BinaryRelation lessThanRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.LessThan);
            BinaryRelation greaterThanRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.GreaterThan);
            BinaryRelation equalsRelation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.Equals);

            // The attribute domain relation parts that might be needed
            AttributeDomainRelationPart attrComp1IntervalStart = this.GenerateRelationPart(this.DomainRelationParts[0], this.Comp1IntervalStart);
            AttributeDomainRelationPart attrComp1IntervalEnd = this.GenerateRelationPart(this.DomainRelationParts[0], this.Comp1IntervalEnd);
            AttributeDomainRelationPart attrComp2IntervalStart = this.GenerateRelationPart(this.DomainRelationParts[1], this.Comp2IntervalStart);
            AttributeDomainRelationPart attrComp2IntervalEnd = this.GenerateRelationPart(this.DomainRelationParts[1], this.Comp2IntervalEnd);

            #region IA to Metric
            switch (requiredRelation.Name)
            {
                case IntervalAlgebraRelationNames.After:
                    // A After B	 |  A.Start > B.End
                    constr1 = new DomainConstraint(this.Name, greaterThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    break;
                case IntervalAlgebraRelationNames.Before:
                    // A Before B	
                    //      A.End < B.Start 
                    constr1 = new DomainConstraint(this.Name, lessThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalStart };

                    metricDomainConstraints.Add(constr1);
                    break;
                case IntervalAlgebraRelationNames.During:
                    // A During B	
                    //      A.Start > B.Start
                    //      A.End < B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", greaterThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };
                    constr2 = new DomainConstraint(this.Name + "_part2", lessThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Equals:
                    // A Equals B	
                    //      A.Start = B.Start
                    //      A.End = B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };
                    constr2 = new DomainConstraint(this.Name + "_part2", equalsRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.FinishedBy:
                    // A Finished-by B	
                    //      A.End = B.End
                    //      A.Start < B.Start
                    constr1 = new DomainConstraint(this.Name + "_part1", equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };
                    constr2 = new DomainConstraint(this.Name + "_part2", lessThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Finishes:
                    // A Finishes B	
                    //      A.End = B.End
                    //      A.Start > B.Start
                    constr1 = new DomainConstraint(this.Name + "_part1", equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };
                    constr2 = new DomainConstraint(this.Name + "_part2", greaterThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Includes:
                    // A Includes B	
                    //      A.Start < B.Start
                    //      A.End > B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", lessThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };
                    constr2 = new DomainConstraint(this.Name + "_part2", greaterThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Meets:
                    // A Meets B	
                    //      A.End = B.Start
                    constr1 = new DomainConstraint(this.Name, equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalStart };

                    metricDomainConstraints.Add(constr1);
                    break;
                case IntervalAlgebraRelationNames.MetBy:
                    // A Met-by B	
                    //      A.Start = B.End
                    constr1 = new DomainConstraint(this.Name, equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    break;
                case IntervalAlgebraRelationNames.OverlappedBy:
                    // A Overlapped-by B	
                    //      A.End > B.End
                    //      A.Start < B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", greaterThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };
                    constr2 = new DomainConstraint(this.Name + "_part2", lessThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Overlaps:
                    // A Overlaps B	
                    //      A.End < B.End
                    //      A.End > B.Start 
                    constr1 = new DomainConstraint(this.Name + "_part1", lessThanRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };
                    constr2 = new DomainConstraint(this.Name + "_part2", greaterThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalStart };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.StartedBy:
                    // A Started-by B	
                    //      A.Start = B.Start
                    //      A.End > B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };
                    constr2 = new DomainConstraint(this.Name + "_part2", greaterThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
                case IntervalAlgebraRelationNames.Starts:
                    //A Starts B	
                    //    A.Start = B.Start
                    //    A.End < B.End
                    constr1 = new DomainConstraint(this.Name + "_part1", equalsRelation, this.CanBeViolated);
                    constr1.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalStart, attrComp2IntervalStart };
                    constr2 = new DomainConstraint(this.Name + "_part2", lessThanRelation, this.CanBeViolated);
                    constr2.DomainRelationParts = new List<IDomainRelationPart>() { attrComp1IntervalEnd, attrComp2IntervalEnd };

                    metricDomainConstraints.Add(constr1);
                    metricDomainConstraints.Add(constr2);
                    break;
            }
            #endregion

            if (metricDomainConstraints.Count == 0)
            {
                // The mapping was not found so return null
                return null;
            }
            else if (metricDomainConstraints.Count == 1)
            {
                // If only one domain constraint can represent the IA then just return it
                return metricDomainConstraints[0];
            }
            else
            {
                // If the IA constraint has to be expressed with two metric constraints then use domain constraints combination
                return new DomainConstraintCombination(this.Name, metricDomainConstraints, DomainConstraintCombinationType.And, this.CanBeViolated);
            }
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
                throw new InvalidOperationException(string.Format("The Interval Algebra to metric domain constraint '{0}' has invalid relation parts", this.Name));
            }

            return attributePart;
        }

    }
}
