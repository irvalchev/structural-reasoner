using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class ConfigurationConstraintTree
    {
        /// <summary>
        /// Creates a constraint tree for a set of components from a domain constraint.
        /// </summary>
        /// <param name="parent">The parent of the current constraint tree</param>
        /// <param name="derivedFrom">The domain constraint from which the tree is derived</param>
        /// <param name="components">The components for which to apply the relevant DomainConstraint</param>
        /// <param name="log">The processing log</param>
        internal ConfigurationConstraintTree(ConfigurationConstraintTree parent, DomainConstraint derivedFrom, List<GKOComponent> components, Log log)
        {
            if (derivedFrom == null)
            {
                throw new ArgumentNullException("The derived from domain constraints has to be set for a configuration constraints tree!");
            }

            this.Parent = parent;

            // assigning the right DomainConstraint
            if (derivedFrom is ITransformDomainConstraint)
            {
                this.derivedFrom = (derivedFrom as ITransformDomainConstraint).TransformConstraint();
            }
            else
            {
                this.derivedFrom = derivedFrom;
            }

            if (this.DerivedFrom is DomainConstraintCombination)
            {
                this.CreateFromCombinationConstraint((DomainConstraintCombination)this.DerivedFrom, components, log);
            }
            else
            {
                this.CreateFromNormalConstraint(this.DerivedFrom, components, log);
            }
        }

        private void CreateFromCombinationConstraint(DomainConstraintCombination derivedFrom, List<GKOComponent> components, Log log)
        {
            this.ConstraintBranches = new List<ConfigurationConstraintTree>();

            foreach (var constraint in derivedFrom.DomainConstraints)
            {
                this.ConstraintBranches.Add(new ConfigurationConstraintTree(this, constraint, components, log));
            }
        }

        private void CreateFromNormalConstraint(DomainConstraint derivedFrom, List<GKOComponent> components, Log log)
        {
            this.ConfigurationConstraints = derivedFrom.GenerateConfigurationConstraints(components, log);

            foreach (var constraint in this.ConfigurationConstraints)
            {
                constraint.OwningTree = this;
            }
        }

        private DomainConstraint derivedFrom;
        public DomainConstraint DerivedFrom
        {
            get { return derivedFrom; }
        }

        /// <summary>
        /// Information whether the current constraint tree have branches or holds leave information about the included configuration constraints
        /// <para> True - This is a constraint tree and holds branches information</para>
        /// <para> False - This is a leave node and holds leave information about the included configuration constraints</para>
        /// </summary>
        public bool IsCombination
        {
            get { return this.DerivedFrom is DomainConstraintCombination; }
        }

        /// <summary>
        /// Information whether the current constraint tree have only one active branch containing constraints
        /// <para> True - This is either a leave tree containing only configuration constraints, or at most one of the branches has configuration constraints in it</para>
        /// <para> False - This is a tree containing several branches with configuration constraints in them</para>
        /// </summary>
        public bool HasSingleActiveBranch
        {
            get { return this.DerivedFrom is DomainConstraintCombination; }
        }

        /// <summary>
        /// The relation family of the constraint
        /// </summary>
        public RelationFamily RelationFamily
        {
            get
            {
                return this.DerivedFrom.RelationFamily;
            }
        }

        /// <summary>
        /// The combination type of the branches when the tree is for DomainConstraintCombination
        /// </summary>
        public DomainConstraintCombinationType CombinationType
        {
            get
            {
                if (this.IsCombination)
                {
                    return (this.derivedFrom as DomainConstraintCombination).Type;
                }
                else
                {
                    // This is just a dummy output, it is not neededs
                    return DomainConstraintCombinationType.And;
                }
            }
        }

        /// <summary>
        /// Specifies whether the constraint tree represents a domain constraint which is valid for TCSP
        /// Note: The current check do not allow disjunctions, while in practice they are allowed for TCSP, if each branch constraint the same two Nodes
        /// </summary>
        public bool IsValidForTcsp
        {
            get
            {
                if (this.IsCombination)
                {
                    return this.CombinationType == DomainConstraintCombinationType.And && this.ConstraintBranches.All(x => x.IsValidForTcsp);
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// The ConfigurationConstraintTree owning the current one
        /// </summary>
        public ConfigurationConstraintTree Parent { get; set; }

        /// <summary>
        /// The branches of the current tree
        /// </summary>
        public List<ConfigurationConstraintTree> ConstraintBranches { get; set; }

        /// <summary>
        /// Holds information about the included configuration constraints.
        /// Null if this is not a leave node in the parent constraint tree.
        /// </summary>
        public List<ConfigurationConstraint> ConfigurationConstraints { get; set; }

        /// <summary>
        /// Finds all configuration constraints in the leaves of the tree
        /// </summary>
        /// <returns></returns>
        public List<ConfigurationConstraint> GetAllConfigurationConstraints()
        {
            // ToDo: This can be optimized by not loading recursively all the constraints each time
            if (this.IsCombination)
            {
                List<ConfigurationConstraint> result = new List<ConfigurationConstraint>();

                foreach (var branch in this.ConstraintBranches)
                {
                    result.AddRange(branch.GetAllConfigurationConstraints());
                }

                return result;
            }
            else
            {
                return this.ConfigurationConstraints;
            }
        }

        /// <summary>
        /// Finds the clusters of nodes which should be in the same constraint network.
        /// This is based on the configuration constraints included in the branches.
        /// When only one branch has constraints each constraints generates its own cluster.
        /// Several clusters can be combined if they have duplicated node (which is not the StartOfTime)
        /// When several branches have constraints - the carthesian product (without duplicates) of the nodes in the constraints of the branches is generated, so even if the constraints do not use the same nodes they are all included in the same cluster.
        /// This is needed because there are satisfaction decision variables which depend on several nodes from different constraints.
        /// </summary>
        /// <returns></returns>
        public List<List<Node>> GetNodeClusters()
        {
            List<List<Node>> clusters = new List<List<Node>>();

            if (this.IsCombination)
            {
                List<List<Node>>[] branchClusters = new List<List<Node>>[this.ConstraintBranches.Count];
                List<List<Node>>[] nonEmptyBranchClusters;

                for (int i = 0; i < this.ConstraintBranches.Count; i++)
                {
                    branchClusters[i] = this.ConstraintBranches[i].GetNodeClusters();
                }

                nonEmptyBranchClusters = branchClusters.Where(x => x.Count > 0).ToArray();

                if (nonEmptyBranchClusters.Length == 1)
                {
                    clusters = nonEmptyBranchClusters[0];
                }
                else if (nonEmptyBranchClusters.Length > 1)
                {
                    List<Node> masterCluster = new List<Node>();

                    // When several branches are active then all nodes are added in the same cluster
                    // This is because the decision variables of the problem "span" over all nodes in this case
                    for (int i = 0; i < nonEmptyBranchClusters.Length; i++)
                    {
                        masterCluster.AddRange(nonEmptyBranchClusters[i].SelectMany(x => x));
                    }

                    clusters.Add(masterCluster);
                }
            }
            else
            {
                foreach (ConfigurationConstraint constraint in this.ConfigurationConstraints)
                {
                    List<Node> constraintNodes = constraint.GetIncludedNodes();
                    clusters.Add(constraintNodes);
                }
            }

            ConstraintNetwork.NormalizeClusters(clusters);
            return clusters;
        }

        /// <summary>
        /// Generates TmsConstraints for the current constraint tree (including branches).
        /// Note: It does not generate constraints for the included configuration constraints, rather it only deals with branch combination logic
        /// </summary>
        /// <returns></returns>
        public List<TmsConstraint> GenerateTmsConstraints()
        {
            List<TmsConstraint> constraints = new List<TmsConstraint>();

            // Only trees with branches generate constraints. The configuration constraints are not processed in this function
            if (this.IsCombination)
            {
                GKODomainAbstract boolDomain = TmsManager.GenerateBoolDomain();
                IEnumerable<IEnumerable<string>> constraintDescriptions = this.GetSatDVarParts();

                // Adding the constraints from each branch
                foreach (var branch in this.ConstraintBranches)
                {
                    constraints.AddRange(branch.GenerateTmsConstraints());
                }

                // Generating the TMS constraints for the current level
                foreach (var constraintDescription in constraintDescriptions)
                {
                    TmsConstraint constraint = new TmsConstraint();
                    string currentDvarName = this.GetSatDVarName(constraintDescription);

                    // The first elements of the constraint are the decision variables of the branches
                    constraint.VariableTuple = constraintDescription.ToList();
                    // The last element is the decision variable of the constraint represented by the current tree
                    constraint.VariableTuple.Add(currentDvarName);

                    switch (this.CombinationType)
                    {
                        case DomainConstraintCombinationType.And:
                            constraint.ConstraintType = constraintDescription.Count().GetAndCTName();
                            break;
                        case DomainConstraintCombinationType.Or:
                            constraint.ConstraintType = constraintDescription.Count().GetOrCTName();
                            break;
                        case DomainConstraintCombinationType.ExactlyOne:
                            constraint.ConstraintType = constraintDescription.Count().GetExactlyOneCTName();
                            break;
                        default:
                            throw new Exception("The chosen constraint combination type is not recognized!");
                    }

                    constraints.Add(constraint);

                    // This prohibits the constraint to be violated if it is hard constraint
                    if (!this.DerivedFrom.CanBeViolated)
                    {
                        constraints.Add(TmsConstraint.GenerateSingleValueConstraint(boolDomain, currentDvarName, TmsManager.TrueValue));
                    }
                }
            }

            return constraints;
        }

        /// <summary>
        /// Finds the TMS decision variables needed for the current constraint tree (including branches) which are not part of the utility function.
        /// Note: It DOES generate variables for the included configuration constraints
        /// </summary>
        /// <returns></returns>
        internal List<TmsDecisionVariable> GetNonUtilityDecisionVariables()
        {
            List<TmsDecisionVariable> variables = new List<TmsDecisionVariable>();

            // When the current tree is not the root then add its decision variables as non-utility ones
            if (this.Parent != null)
            {
                List<string> currentDVarNames = this.GetSatDVarNames();
                GKODomainAbstract boolDomain = TmsManager.GenerateBoolDomain();

                foreach (var var in currentDVarNames)
                {
                    variables.Add(new TmsDecisionVariable(boolDomain, var));
                }
            }

            // Adding the variables from the branches
            if (this.IsCombination)
            {
                foreach (var branch in this.ConstraintBranches)
                {
                    variables.AddRange(branch.GetNonUtilityDecisionVariables());
                }
            }

            return TmsDecisionVariable.GetDistinctVariables(variables, false);
        }

        /// <summary>
        /// Finds the TMS decision variables needed for the current constraint tree (branches are not relevant) which are not part of the utility function.
        /// Note: It DOES generate the satisfaction decision variables for the included configuration constraints
        /// </summary>
        /// <returns></returns>
        internal List<TmsDecisionVariable> GetUtilityDecisionVariables()
        {
            List<TmsDecisionVariable> variables = new List<TmsDecisionVariable>();

            // Only when the current tree is the root its variables are considered utility ones
            if (this.Parent == null)
            {
                List<string> currentDVarNames = this.GetSatDVarNames();
                GKODomainAbstract boolDomain = TmsManager.GenerateBoolDomain();

                foreach (var var in currentDVarNames)
                {
                    variables.Add(new TmsDecisionVariable(boolDomain, var) { UtilityFactor = 1 });
                }
            }

            return TmsDecisionVariable.GetDistinctVariables(variables, true);
        }

        /// <summary>
        /// Assigns the values of the metric parts using special values (e.g. number of components in the context)
        /// </summary>
        /// <param name="context">The structuring context including the constraint tree</param>
        internal void AssignSpecialMetricValues(GKOStructuringContext context)
        {
            if (!this.IsCombination)
            {
                // If the constraint tree holds constraints then assign their metric values
                foreach (var constraint in this.ConfigurationConstraints)
                {
                    constraint.AssignSpecialMetricValues(context);
                }
            }
            else
            {
                // Else the constraint tree has branches and assign the metric special part for each of them
                foreach (var branch in this.ConstraintBranches)
                {
                    branch.AssignSpecialMetricValues(context);
                }
            }
        }

        /// <summary>
        /// Switches a constraint with another constraint
        /// </summary>
        /// <param name="constraintToSwitch">The constraint to be removed from the tree</param>
        /// <param name="newConstraint">The new constraint to be added to the tree</param>
        internal void SwitchConstraint(ConfigurationConstraint constraintToSwitch, ConfigurationConstraint newConstraint)
        {
            newConstraint.OwningTree = this;
            this.ConfigurationConstraints.Remove(constraintToSwitch);
            this.ConfigurationConstraints.Add(newConstraint);
        }

    }
}
