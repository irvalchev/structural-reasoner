using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class ProblemDescription
    {
        public ProblemDescription(GKOConfiguration configuration, DomainConstraintsSource constraintSource)
        {
            this.Configuration = configuration;
            this.ConstraintSource = constraintSource;
        }

        public GKOConfiguration Configuration { get; set; }

        public DomainConstraintsSource ConstraintSource { get; set; }
    }

    public class RandomProblemGenerator
    {
        public RandomProblemGenerator()
        {
            this.MaxCombinationParts = 2;
            this.AllowZeroIntervals = false;
            this.CombinationConstraintsPercentage = 0;
            this.SoftConstraintsEnabled = false;
            this.RelationFamily = StructuralRelationsManager.MetricRelationsFamily;
            this.ConstraintsToComponentsRatio = 1;

            
        }

        public ProblemDescription GeneratedProblem { get; set; }

        # region Options and Calculated Fields

        /// <summary>
        /// The maximum number of combination parts in a single domain constraint combination that can be generated in the random problem
        /// </summary>
        public int MaxCombinationParts { get; set; }

        /// <summary>
        /// Specifies whether zero-length intervals are allowed for the metric IA transformations
        /// </summary>
        public bool AllowZeroIntervals { get; set; }

        /// <summary>
        /// The RelationFamily for which should be the problem
        /// </summary>
        public RelationFamily RelationFamily { get; set; }

        /// <summary>
        /// The number of components to include in the problem
        /// </summary>
        public int NumberOfComponents { get; set; }

        /// <summary>
        /// The maximum metric value of the integer domain.
        /// Currently the number of components is used.
        /// </summary>
        public int MaxMetricValue
        {
            get
            {
                return this.NumberOfComponents;
            }
        }

        /// <summary>
        /// Specifies whether the constraints are ALL soft or ALL hard.
        /// <para>Note: When hard constraints are enabled, at most one constraint can be active between each pair of components. Otherwise, most of the problems would be unsolvable</para>
        /// <para>Note: TCSP can be solved only when Metric relation family is used, all constraints are hard, and combination constraints are not allowed</para>
        /// <para>Note: Combination constraints can be used only when soft constraints are enabled. This is so, because all* (some exceptions for AND combinations are possible) non-root constraints in a combination constraint are considered soft</para>
        /// </summary>
        public bool SoftConstraintsEnabled { get; set; }

        /// <summary>
        /// The ratio between components and number of constraints.
        /// <example>
        /// When N Components are generated and ratio R is specified,
        /// then Round(N*R) constraints will be created
        /// </example>
        /// </summary>
        public double ConstraintsToComponentsRatio { get; set; }

        /// <summary>
        /// Specifies the percentage of combination constraints in the list of all constraints
        /// </summary>
        public int CombinationConstraintsPercentage { get; set; }

        /// <summary>
        /// Specifies whether combination constraints are included. Based on the <see cref="CombinationConstraintsPercentage"/>
        /// <para>Note: Combination constraints can be used only when soft constraints are enabled</para>
        /// </summary>
        public bool CombinationConstraintsEnabled
        {
            get
            {
                return this.CombinationConstraintsPercentage > 0;
            }
        }

        /// <summary>
        /// Specifies the number of constraints that has to be generated. Based on <see cref="NumberOfComponents"/> and <see cref="ConstraintsToComponentsRatio"/>
        /// </summary>
        public int NumberOfConstraints
        {
            get
            {
                return (int)Math.Round(this.NumberOfComponents * this.ConstraintsToComponentsRatio);
            }
        }

        /// <summary>
        /// The number of the actually generated constraints. It is possible that less constraints are generated, when it is not possible to place some of them.
        /// </summary>
        public int NumberOfActualConstraints { get; set; }

        /// <summary>
        /// Specifies the number of combination constraints that has to be generated. Based on <see cref="NumberOfConstraints"/> and <see cref="CombinationConstraintsPercentage"/>
        /// </summary>
        public int NumberOfCombinationConstraints
        {
            get
            {
                // Some basic validation adjustments
                if (this.CombinationConstraintsPercentage > 100)
                {
                    this.CombinationConstraintsPercentage = 100;
                }
                else if (this.CombinationConstraintsPercentage < 0)
                {
                    this.CombinationConstraintsPercentage = 0;
                }

                return (int)Math.Round(this.NumberOfConstraints * ((double)this.CombinationConstraintsPercentage / 100));
            }
        }

        /// <summary>
        /// The number of the actually generated constraints. It is possible that less constraints are generated, when it is not possible to place some of them.
        /// </summary>
        public int NumberOfActualCombinationConstraints { get; set; }

        # endregion

        /// <summary>
        /// Generates a problem using the current options
        /// </summary>
        public void GenerateProblem()
        {
            if (this.NumberOfComponents < 2)
            {
                throw new InvalidOperationException("Cannot create a random problem with less than 2 components!");
            }
            if (!this.SoftConstraintsEnabled && this.CombinationConstraintsEnabled)
            {
                throw new InvalidOperationException("Cannot generate a problem that uses hard constraints and includes combination domain constraints simultaneously!");
            }
            else
            {
                SimpleKnowledgeBase kb = GenerateKnowledgeBase();
                SimpleDomainConstraintsSource constraintsSource = new SimpleDomainConstraintsSource(kb);
                int actualConstraints = 0;
                int actualCombinationConstraints = 0;
                // This dictionary holds information which node is connected to what other nodes
                // The dictionary works on GKOCOmponent level, because the current random problems include at most one attribute for each component
                Dictionary<GKOComponent, List<GKOComponent>> nodeConnections = new Dictionary<GKOComponent, List<GKOComponent>>();

                // Creating the combination constraints first
                for (int i = 0; i < this.NumberOfCombinationConstraints; i++)
                {
                    Random random = new Random();
                    bool createConstraint = true;
                    DomainConstraintCombination combinationConstraint = null;
                    List<DomainConstraint> includedConstraints = new List<DomainConstraint>();
                    DomainConstraintCombinationType combinationType = (DomainConstraintCombinationType)random.Next(1, 4);

                    for (int j = 0; j < random.Next(2, this.MaxCombinationParts + 1); j++)
                    {
                        DomainConstraint newConstraint = GenerateDomainConstraint(kb, nodeConnections);

                        if (newConstraint == null)
                        {
                            createConstraint = false;
                            break;
                        }
                        else
                        {
                            includedConstraints.Add(newConstraint);
                        }
                    }

                    if (createConstraint)
                    {
                        combinationConstraint = new DomainConstraintCombination(Guid.NewGuid().ToString(), includedConstraints, combinationType, this.SoftConstraintsEnabled);
                    }

                    if (combinationConstraint != null)
                    {
                        actualConstraints++;
                        actualCombinationConstraints++;
                        constraintsSource.DomainConstraints.Add(combinationConstraint);
                    }
                    else
                    {
                        // Currently this shouldn't be reachable
                        break;
                    }
                }

                // Creating the normal constraints
                for (int i = 0; i < this.NumberOfConstraints - this.NumberOfCombinationConstraints; i++)
                {
                    DomainConstraint constraint = GenerateDomainConstraint(kb, nodeConnections);

                    if (constraint != null)
                    {
                        actualConstraints++;
                        constraintsSource.DomainConstraints.Add(constraint);
                    }
                    else
                    {
                        break;
                    }
                }

                this.GeneratedProblem = new ProblemDescription(this.GenerateConfiguration(kb), constraintsSource);
                this.NumberOfActualCombinationConstraints = actualCombinationConstraints;
                this.NumberOfActualConstraints = actualConstraints;
            }
        }

        /// <summary>
        /// Transforms the problem to equal Metric problem. If not possible an exception is thrown.
        /// <para>Do not change any options of the problem generator before calling this function in order to guarantee corectness and equal problem.
        /// </para>
        /// <para>
        /// Note: The options will change after successfull generation to depict the new problem
        /// </para>
        /// <para>
        /// Note:  In the process combination AND constraints can be generated.
        /// </para>
        /// </summary>
        public void TransformToMetric()
        {
            // If no problem is available or the problem is already metric, then do nothing
            if (this.GeneratedProblem == null || this.RelationFamily == StructuralRelationsManager.MetricRelationsFamily)
            {
                return;
            }
            if (this.RelationFamily != StructuralRelationsManager.IntervalAlgebra && this.RelationFamily != StructuralRelationsManager.IntervalAlgebra)
            {
                throw new InvalidOperationException("The Calculus of the original problem cannot be translated to metric relations!");
            }
            else
            {
                SimpleKnowledgeBase kb = GenerateKnowledgeBase();
                SimpleDomainConstraintsSource constraintsSource = new SimpleDomainConstraintsSource(kb);
                int constraintsCount;
                int combinationConstraintsCount;

                // For IA add the interval inequality constraint
                if (this.RelationFamily == StructuralRelationsManager.IntervalAlgebra)
                {
                    constraintsSource.DomainConstraints.Add(DomainConstraint.IntervalStartBeforeEndConstraint(kb.Attributes[1], kb.Attributes[2], this.AllowZeroIntervals));
                }

                foreach (var constraint in this.GeneratedProblem.ConstraintSource.DomainConstraints)
                {
                    constraintsSource.DomainConstraints.Add(GenerateEqualMetricConstraint(kb, constraint));
                }

                constraintsCount = constraintsSource.DomainConstraints.Count;
                combinationConstraintsCount = constraintsSource.DomainConstraints.Count(x => x is DomainConstraintCombination);

                kb.StructuringContexts.ForEach(x =>
                {
                    x.IncludedRelationFamilies.Clear();
                    x.IncludedRelationFamilies.Add(StructuralRelationsManager.MetricRelationsFamily);
                });
                this.GeneratedProblem = new ProblemDescription(this.GenerateConfiguration(kb), constraintsSource);
                // Changing the options to represent adequately the new problem
                this.RelationFamily = StructuralRelationsManager.MetricRelationsFamily;
                this.NumberOfComponents = kb.Components.Count;
                this.ConstraintsToComponentsRatio = constraintsCount / this.NumberOfComponents;
                this.CombinationConstraintsPercentage = (int)Math.Round(((double)combinationConstraintsCount / constraintsCount) * 100, 0);
                this.NumberOfActualCombinationConstraints = combinationConstraintsCount;
                this.NumberOfActualConstraints = constraintsCount;
            }
        }

        /// <summary>
        /// Generates a configuration for a sample knowledge base
        /// </summary>
        /// <param name="kb"></param>
        /// <returns></returns>
        private GKOConfiguration GenerateConfiguration(KnowledgeBase kb)
        {
            GKOConfiguration result = new GKOConfiguration();

            result.Components = kb.Components;
            result.StructuringContexts = kb.StructuringContexts;

            return result;
        }

        private DomainConstraint GenerateDomainConstraint(KnowledgeBase kb, Dictionary<GKOComponent, List<GKOComponent>> nodeConnections)
        {
            DomainConstraint domainConstraint = null;
            List<GKOComponent> possibleStartComponents;

            if (this.SoftConstraintsEnabled)
            {
                possibleStartComponents = kb.Components;
            }
            else
            {
                // this.NumberOfComponents - 1 is used because the x component should not be in the list
                possibleStartComponents = kb.Components.Where(x => !nodeConnections.ContainsKey(x) ||
                    nodeConnections[x].Count < this.NumberOfComponents - 1).ToList();
            }

            // It is possible that no more constraints can be created
            if (possibleStartComponents.Count != 0)
            {
                Random random = new Random();
                List<GKOComponent> possibleEndComponents;
                BinaryRelation constraintRelation;
                GKOComponent startComponent;
                GKOComponent endComponent;
                List<IDomainRelationPart> constraintRelationParts = new List<IDomainRelationPart>();

                // Choosing the first component for the constraint
                startComponent = possibleStartComponents[random.Next(0, possibleStartComponents.Count)];

                // Find all components which can serve as second element in the relation constraint
                if (this.SoftConstraintsEnabled || !nodeConnections.ContainsKey(startComponent))
                {
                    possibleEndComponents = new List<GKOComponent>(kb.Components);
                }
                else
                {
                    possibleEndComponents = kb.Components.Where(x => !nodeConnections[startComponent].Contains(x)).ToList();
                }
                // Cannot create constraint to the same node
                possibleEndComponents.Remove(startComponent);

                if (possibleEndComponents.Count == 0)
                {
                    throw new ApplicationException("No suitable second component is found to generate a domain constraint!. This should not have happened! Please, revise the random problem generation code!");
                }

                // Choosing the second component for the constraint
                endComponent = possibleEndComponents[random.Next(0, possibleEndComponents.Count)];

                // Creating the constraint relation and the domain relation parts
                if (this.RelationFamily == StructuralRelationsManager.MetricRelationsFamily)
                {
                    // Only non-unary (in component sense) relations are used for random problems
                    List<BinaryRelation> possibleRelations = this.RelationFamily.Relations.Where(x =>
                        (x.Signature.Count == 2 && x.Signature.All(y => y == typeof(MetricRelationPart)) ||
                        x.Signature.Count == 3)).ToList();
                    constraintRelation = possibleRelations[random.Next(0, possibleRelations.Count)];

                    // Adding the first relation part. It is matched to the startComponent.Attributes["Position"] at runtime
                    constraintRelationParts.Add(new AttributeDomainRelationPart(new ComponentFilter() { Filter = startComponent }, kb.Attributes[0]));
                    // Adding the second relation part. It is matched to the endComponent.Attributes["Position"] at runtime
                    constraintRelationParts.Add(new AttributeDomainRelationPart(new ComponentFilter() { Filter = endComponent }, kb.Attributes[0]));

                    // Hack: When the chosen constraint is ternary then the third part should be metric
                    if (constraintRelation.Signature.Count == 3)
                    {
                        constraintRelationParts.Add(new MetricDomainRelationPart(random.Next(kb.IntDomains[0].MinValue, kb.IntDomains[0].MaxValue), kb.IntDomains[0]));
                    }
                }
                else
                {
                    constraintRelation = this.RelationFamily.Relations[random.Next(0, this.RelationFamily.Relations.Count)];

                    // Adding the first relation part. It is matched to the startComponent at runtime
                    constraintRelationParts.Add(new ComponentDomainRelationPart(new ComponentFilter() { Filter = startComponent }));
                    // Adding the second relation part. It is matched to the endComponent at runtime
                    constraintRelationParts.Add(new ComponentDomainRelationPart(new ComponentFilter() { Filter = endComponent }));
                }

                // Creating the domain constraint
                // Rem: This has to be changed if the constraint combinations are allowed when using hard constraints
                domainConstraint = new DomainConstraint(Guid.NewGuid().ToString(), constraintRelation, this.SoftConstraintsEnabled);
                domainConstraint.DomainRelationParts = constraintRelationParts;

                // Updating the nodes connection information
                if (!nodeConnections.ContainsKey(startComponent))
                {
                    nodeConnections.Add(startComponent, new List<GKOComponent>());
                }
                if (!nodeConnections.ContainsKey(endComponent))
                {
                    nodeConnections.Add(endComponent, new List<GKOComponent>());
                }
                nodeConnections[startComponent].Add(endComponent);
                nodeConnections[endComponent].Add(startComponent);
            }

            return domainConstraint;
        }

        private DomainConstraint GenerateEqualMetricConstraint(KnowledgeBase kb, DomainConstraint constraint)
        {
            DomainConstraint result;

            if (constraint is DomainConstraintCombination)
            {
                DomainConstraintCombination asCombinationConstraint = constraint as DomainConstraintCombination;
                List<DomainConstraint> includedConstraints = new List<DomainConstraint>();

                foreach (var constraintPart in asCombinationConstraint.DomainConstraints)
                {
                    includedConstraints.Add(this.GenerateEqualMetricConstraint(kb, constraintPart));
                }

                result = new DomainConstraintCombination(asCombinationConstraint.Name, includedConstraints, asCombinationConstraint.Type, asCombinationConstraint.CanBeViolated);
            }
            else
            {
                ITransformDomainConstraint transformationConstraint = null;

                // Since reference checks are performed the components and attributes has to be changed
                constraint.DomainRelationParts.ForEach(x =>
                    {
                        if (x is ComponentDomainRelationPart)
                        {
                            (x as ComponentDomainRelationPart).Filter.Filter = kb.Components.Single(y => y.Id == (x as ComponentDomainRelationPart).Filter.Filter.Id);
                        }
                        if (x is AttributeDomainRelationPart)
                        {
                            (x as AttributeDomainRelationPart).Filter.Filter = kb.Components.Single(y => y.Id == (x as AttributeDomainRelationPart).Filter.Filter.Id);
                            (x as AttributeDomainRelationPart).Attribute = kb.Attributes.Single(y => y.Id == (x as AttributeDomainRelationPart).Attribute.Id);
                        }
                    });

                if (this.RelationFamily == StructuralRelationsManager.IntervalAlgebra)
                {
                    transformationConstraint = new IaToMetricDomainConstraint(constraint.Name, constraint.allowedRelations[0], constraint.CanBeViolated, kb.Attributes[1], kb.Attributes[2], kb.Attributes[1], kb.Attributes[2]);
                    ((IaToMetricDomainConstraint)transformationConstraint).DomainRelationParts = new List<IDomainRelationPart>(constraint.DomainRelationParts);
                }
                // Currently, the only other possible option is Point Algebra
                else
                {
                    transformationConstraint = new PaToMetricDomainConstraint(constraint.Name, constraint.allowedRelations[0], constraint.CanBeViolated, kb.Attributes[0], kb.Attributes[0]);
                    ((PaToMetricDomainConstraint)transformationConstraint).DomainRelationParts = new List<IDomainRelationPart>(constraint.DomainRelationParts);
                }

                result = transformationConstraint.TransformConstraint();
            }

            return result;
        }

        /// <summary>  
        /// <para> ----------------------------------INT DOMAINS: </para>
        /// <para> 0: Metric domain: [StructuralRelationsManager.MetricDomain.MinValue, this.MaxMetricValue]  </para>
        /// <para> ----------------------------------ATTRIBUTES: </para>
        /// <para> 0: Position (Int) - metric attribute  </para>
        /// <para> 1: Start (Int) - metric attribute  </para>
        /// <para> 2: End (Int) - metric attribute  </para>
        /// <para> ----------------------------------COMPONENT TYPES: </para>
        /// <para> 0: ComponentType1 - all attributes  </para>
        /// <para> ----------------------------------COMPONENTS: </para>
        /// <para> n Components (n is <see cref="NumberOfComponents"/>) </para>
        /// <para> ----------------------------------STRUCTURING CONTEXTS: </para>
        /// <para> 0: All components. Relevant relation family is taken from the options </para>
        /// <para> ----------------------------------CONSTRAINT TYPES: </para>
        /// <para> Not set </para>
        /// </summary>
        private SimpleKnowledgeBase GenerateKnowledgeBase()
        {
            SimpleKnowledgeBase kb = new SimpleKnowledgeBase();
            GKOIntDomain metricDomain = new GKOIntDomain();

            metricDomain.Id = TmsManager.DomainNameMetric;
            metricDomain.Name = TmsManager.DomainNameMetric;
            metricDomain.StepWidth = 1;
            metricDomain.MinValue = StructuralRelationsManager.MetricDomain.MinValue;
            metricDomain.MaxValue = this.MaxMetricValue;

            // Adding metric domain
            kb.IntDomains.Add(metricDomain);

            // Creating attributes
            kb.Attributes.Add(new GKOAttribute()
            {
                Id = "Position",
                Name = "Position",
                Type = "",
                Domain = kb.IntDomains[0].Name
            });
            kb.Attributes.Add(new GKOAttribute()
            {
                Id = "Start",
                Name = "Start",
                Type = "",
                Domain = kb.IntDomains[0].Name
            });
            kb.Attributes.Add(new GKOAttribute()
            {
                Id = "End",
                Name = "End",
                Type = "",
                Domain = kb.IntDomains[0].Name
            });

            // Creating component type/s
            kb.ComponentTypes.Add(new GKOComponentType()
            {
                Id = "ComponentType1",
                Name = "ComponentType1",
                Attributes = kb.Attributes
            });

            // Creating the components
            for (int i = 0; i < this.NumberOfComponents; i++)
            {
                Dictionary<GKOAttribute, string> attributeValues = new Dictionary<GKOAttribute, string>();
                attributeValues.Add(kb.Attributes[0], "0");
                attributeValues.Add(kb.Attributes[1], "0");
                attributeValues.Add(kb.Attributes[2], "0");

                kb.Components.Add(new GKOComponent()
                {
                    Active = true,
                    Id = "Component_" + (i + 1),
                    Name = "Component_" + (i + 1),
                    Type = kb.ComponentTypes[0],
                    AttributeValues = attributeValues
                });
            }

            // Creating the structuring contet
            kb.StructuringContexts.Add(new GKOStructuringContext("Structuring Context", new List<RelationFamily>() { this.RelationFamily })
            {
                Active = true,
                Components = kb.Components
            });

            return kb;
        }

    }
}
