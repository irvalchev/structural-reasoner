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
    public class DomainConstraint
    {
        internal List<BinaryRelation> allowedRelations;

        /// <summary>
        /// Cretes a new domain constraint with name and softness information. Can be used in the derived classes
        /// </summary>
        /// <param name="name">The unique name of the domain constraint.</param>
        /// <param name="canBeViolated">Set to true for soft constraint, false for hard constraint</param>
        protected DomainConstraint(string name, bool canBeViolated)
        {
            this.DomainRelationParts = new List<IDomainRelationPart>();
            this.allowedRelations = new List<BinaryRelation>();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("The name of the domain constraint is required!");
            }
            else
            {
                this.Name = name;
                this.CanBeViolated = canBeViolated;
            }
        }

        /// <summary>
        /// Cretes a new domain constraint which constraints the possible relations between the matched components.
        /// If only one relation is given as parameter it will be treaten as required. 
        /// If a list is given, then one of the relations must hold (not all).
        /// All of the relations must be from the same relation family.
        /// </summary>
        /// <param name="name">The unique name of the domain constraint.</param>
        /// <param name="allowedRelations">The list of allowed relations between the components. It is an OR list.</param>
        /// <param name="canBeViolated">Set to true for soft constraint, false for hard constraint</param>
        public DomainConstraint(string name, List<BinaryRelation> allowedRelations, bool canBeViolated)
        {
            this.DomainRelationParts = new List<IDomainRelationPart>();
            this.allowedRelations = new List<BinaryRelation>();

            // Check if more than one relation families or relation without family exist in the list, which is prohibited
            if (allowedRelations.Any(x => x.RelationFamily == null) || allowedRelations.GroupBy(x => x.RelationFamily).Count() > 1)
            {
                throw new ArgumentException("All relations must be from the same relation family!");
            }
            if (allowedRelations.Count > 1 && allowedRelations[0].RelationFamily == StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.MetricRelationsName))
            {
                throw new ArgumentException("Metric relations cannot be used in a combination (no ORs)!");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("The name of the domain constraint is required!");
            }
            else
            {
                this.allowedRelations = allowedRelations;
                this.Name = name;
                this.CanBeViolated = canBeViolated;
            }
        }

        /// <summary>
        /// Cretes a new domain constraint which requires a relation to hold between the matched components.
        /// </summary>
        /// <param name="name">The unique name of the domain constraint.</param>
        /// <param name="requiredRelation">The relation between the matched components.</param>
        /// <param name="canBeViolated">Set to true for soft constraint, false for hard constraint</param>
        public DomainConstraint(string name, BinaryRelation requiredRelation, bool canBeViolated)
        {
            this.DomainRelationParts = new List<IDomainRelationPart>();
            this.allowedRelations = new List<BinaryRelation>();

            if (requiredRelation == null)
            {
                throw new ArgumentNullException("The required relation must be set!");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("The name of the domain constraint is required!");
            }
            else
            {
                this.allowedRelations.Add(requiredRelation);
                this.Name = name;
                this.CanBeViolated = canBeViolated;
            }
        }

        /// <summary>
        /// The unique name of the domain constraint
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies whether the constraint is hard or soft
        /// </summary>
        public bool CanBeViolated { get; set; }

        /// <summary>
        /// The relation family used y the constraint
        /// </summary>
        public RelationFamily RelationFamily
        {
            get
            {
                if (this is DomainConstraintCombination)
                {
                    DomainConstraintCombination asCombination = (DomainConstraintCombination)this;

                    // All of the included constraints should have the same relation family
                    return asCombination.DomainConstraints[0].RelationFamily;
                }
                else
                {
                    // All relations in a DomainConstraint should be from the same RelationFamily, so simply take the first one
                    return this.allowedRelations[0].RelationFamily;
                }
            }
        }

        public List<IDomainRelationPart> DomainRelationParts { get; set; }

        #region Static

        /// <summary>
        /// Creates a domain constraint restricting an attribute to be less than the MetricDomain min value + 1
        /// </summary>
        /// <returns></returns>
        public static DomainConstraint MinValueConstraint(GKOAttribute attribute, GKOIntDomain metricDomain)
        {
            BinaryRelation greaterOrEquals = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.GreaterOrEqualsN);
            DomainConstraint minConstraint = new DomainConstraint("Metric min value constraint", greaterOrEquals, false);

            minConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(new ComponentFilter(), attribute));
            minConstraint.DomainRelationParts.Add(new MetricDomainRelationPart(metricDomain.MinValue, metricDomain));

            return minConstraint;
        }

        /// <summary>
        /// Creates a domain constraint restricting an attribute to be greater than the MetricDomain max value - 1
        /// </summary>
        /// <returns></returns>
        public static DomainConstraint MaxValueConstraint(GKOAttribute attribute, GKOIntDomain metricDomain)
        {
            BinaryRelation lessOrEquals = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.LessOrEqualsN);
            DomainConstraint maxConstraint = new DomainConstraint("Metric max value constraint", lessOrEquals, false);

            maxConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(new ComponentFilter(), attribute));
            maxConstraint.DomainRelationParts.Add(new MetricDomainRelationPart(metricDomain.MaxValue, metricDomain));

            return maxConstraint;
        }

        /// <summary>
        /// Creates a domain constraint restricting that the self edge of a node is restricted to use the equals relation of the family
        /// </summary>
        /// <returns></returns>
        public static DomainConstraint SelfEqualsConstraint(RelationFamily calculus)
        {
            DomainConstraint equalsConstraint = new DomainConstraint("Self equals", calculus.EqualsRelation, false);

            equalsConstraint.DomainRelationParts.Add(new ComponentDomainRelationPart(new ComponentFilter()));
            equalsConstraint.DomainRelationParts.Add(new ComponentDomainRelationPart(new ComponentFilter() { SameAsDomainRelationPartNr = 1 }));

            return equalsConstraint;
        }

        /// <summary>
        /// Creates a domain constraint restricting the start of an interval to be before (or equal) to its end
        /// </summary>
        /// <param name="startAttribute">The start of interval attribute </param>
        /// <param name="endAttribute">The end of interval attribute </param>
        /// <param name="allowZeroIntervals">Specifies whether to use "Less than" or "Less or equals" relations, which enables/disables the generation of zero-length intervals</param>
        /// <returns></returns>
        public static DomainConstraint IntervalStartBeforeEndConstraint(GKOAttribute startAttribute, GKOAttribute endAttribute, bool allowZeroIntervals)
        {
            BinaryRelation relation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, allowZeroIntervals ? MetricRelationNames.LessOrEquals : MetricRelationNames.LessThan);
            DomainConstraint intervalConstraint = new DomainConstraint("Interval start/end constraint", relation, false);

            intervalConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(new ComponentFilter(), startAttribute));
            intervalConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(
                new ComponentFilter() { SameAsDomainRelationPartNr = 1 },
                endAttribute));

            return intervalConstraint;
        }

        #endregion

        /// <summary>
        /// Generates the configuration constraints matching the domain constraint for a given list of components
        /// </summary>
        /// <param name="componentsList">The list of components</param>
        /// <returns>A list of configuration constraints representing the domain constraint</returns>
        internal virtual List<ConfigurationConstraint> GenerateConfigurationConstraints(List<GKOComponent> componentsList, Log log)
        {
            List<ConfigurationConstraint> constraints = new List<ConfigurationConstraint>();

            if (DomainRelationParts != null && DomainRelationParts.Count > 0 && componentsList != null && componentsList.Count > 0)
            {
                List<List<IRelationPart>> possibleParts = new List<List<IRelationPart>>();
                IEnumerable<IEnumerable<IRelationPart>> partsCarthesianProduct = new[] { Enumerable.Empty<IRelationPart>() };

                for (int i = 0; i < DomainRelationParts.Count; i++)
                {
                    possibleParts.Add(DomainRelationParts[i].GeneratePossibleRelationParts(componentsList, log));
                }

                // Calculating the carthesian product of the different relation parts
                partsCarthesianProduct = possibleParts.Aggregate(
                  partsCarthesianProduct,
                  (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    // ToDo: Check if the filter works properly
                    // This prohibits relations including the same elements (components/attributes)
                    where CheckNewRelationPart(accseq.ToList(), item, log)
                    select accseq.Concat(new[] { item }));

                foreach (var relationPartsAssignment in partsCarthesianProduct)
                {
                    ConfigurationConstraint constraint = new ConfigurationConstraint(this);

                    try
                    {
                        constraint.AllowedRelations = this.allowedRelations;
                        constraint.SetRelationParts(relationPartsAssignment.ToList());
                        constraints.Add(constraint);
                    }
                    catch (Exception ex)
                    {
                        log.AddItem(LogType.Warning, "A configuration constraint could not be created: " + ex.ToString());
                    }
                }
            }

            return constraints;
        }

        /// <summary>
        /// Checks whether an item should be added to the carthesian product when generating a configuration constraints
        /// <para>If the component of the new item matches the component of old ones the configuration constraint is ivalid</para>
        /// <para>EXCEPT: When the ComponentFilter generating the newItem actually specifies that it should be the same component as a previous one. 
        /// In this case only equal components are returned as true</para>
        /// </summary>
        /// <param name="currTuple">The current relation tuple</param>
        /// <param name="newItem">The new item to add to the tuple</param>
        /// <returns></returns>
        private bool CheckNewRelationPart(List<IRelationPart> currTuple, IRelationPart newItem, Log log)
        {
            ComponentDomainRelationPart itemDomRelCompPart = this.DomainRelationParts[currTuple.Count] as ComponentDomainRelationPart;
            AttributeDomainRelationPart itemDomRelAttrPart = this.DomainRelationParts[currTuple.Count] as AttributeDomainRelationPart;

            // If it is not component or attribute relation part there are no constraints
            if (itemDomRelCompPart == null && itemDomRelAttrPart == null)
            {
                return true;
            }
            else
            {
                int? samePartNr = null;

                // Finds whether the component part is equal to another component
                if (itemDomRelCompPart != null)
                {
                    samePartNr = itemDomRelCompPart.Filter.SameAsDomainRelationPartNr;
                }
                else if (itemDomRelAttrPart != null)
                {
                    samePartNr = itemDomRelAttrPart.Filter.SameAsDomainRelationPartNr;
                }

                // If the component part should equal to other component - it (the component) is found and checked if it matches the current one
                if (samePartNr.HasValue)
                {
                    if (currTuple.Count < samePartNr.Value || samePartNr.Value < 1)
                    {
                        log.AddItem(LogType.Warning, string.Format("The 'component same as filter' has wrong value({0} when the maximum is {1}) for domain constraint {2}. A configuration constraint cannot be generated.", samePartNr.Value, currTuple.Count, this.Name));
                        return false;
                    }
                    else
                    {
                        ComponentRelationPart equalCompPart = currTuple[samePartNr.Value - 1] as ComponentRelationPart;
                        AttributeRelationPart equalAttrPart = currTuple[samePartNr.Value - 1] as AttributeRelationPart;

                        if (equalCompPart == null && equalAttrPart == null)
                        {
                            log.AddItem(LogType.Warning, string.Format("The 'component same as filter' points to relation part with no component included (i.e. metric part) for domain constraint {0}. A configuration constraint cannot be generated.", this.Name));
                            return false;
                        }
                        else
                        {
                            ComponentRelationPart newItemCompPart = newItem as ComponentRelationPart;
                            AttributeRelationPart newItemAttrPart = newItem as AttributeRelationPart;

                            if (newItemCompPart == null && newItemAttrPart == null)
                            {
                                log.AddItem(LogType.Warning, string.Format("The 'component same as filter' points to relation part with no component included for domain constraint {0}. A configuration constraint cannot be generated.", this.Name));
                                return false;
                            }
                            else
                            {
                                GKOComponent componentToComapre = null;

                                // The component from the new relation part is extracted
                                if (newItemCompPart != null)
                                {
                                    componentToComapre = newItemCompPart.Component;
                                }
                                else if (newItemAttrPart != null)
                                {
                                    componentToComapre = newItemAttrPart.Component;
                                }

                                // And finally the new component is compared with the one from the relation part it should match
                                if (equalCompPart != null)
                                {
                                    return equalCompPart.Component.Id == componentToComapre.Id;
                                }
                                else if (equalAttrPart != null)
                                {
                                    return equalAttrPart.Component.Id == componentToComapre.Id;
                                }

                                // this should not be reached
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    // If the value is not set to match other component then specificly exclude matching the same components
                    return !currTuple.Any(x =>
                                ((x is ComponentRelationPart) && (x as ComponentRelationPart).Equals(newItem)) ||
                                ((x is AttributeRelationPart) && (x as AttributeRelationPart).Equals(newItem))
                                );
                }
            }
        }

    }
}
