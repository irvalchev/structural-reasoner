using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Tms;
using Razr.CS3.mqm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    static class TmsHelperExtensions
    {
        /// <summary>
        /// Gets the unique identifier of the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetUId(this Node node)
        {
            return node == null ? "StartOfTime-" : string.Format("{0}-{1}", node.Component.Name, node.Attribute == null ? "" : node.Attribute.Name);
        }

        /// <summary>
        /// Gets the unique identifier of the edge
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetUId(this Edge edge)
        {
            return String.Format("{0}-{1}-{2}", edge.Network.UId, edge.StartNode.GetUId(), edge.EndNode.GetUId());
        }

        public static string GetTmsRelationsDomainName(this RelationFamily calculus)
        {
            return string.Format("{0}_{1}", calculus.Name, TmsManager.DomainNameCalculusPostfix);
        }

        public static string GetTmsCompositionConstraintName(this RelationFamily calculus)
        {
            return string.Format("{0}_{1}", calculus.Name, TmsManager.CTNamePostfixCalculusComposition);
        }

        /// <summary>
        /// Gets the TMS decision variable name for the node. This should be used only for attribute nodes
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns></returns>
        public static string GetDVarName(this Node node)
        {
            return String.Format("{0}-dvar", node.GetUId());
        }

        /// <summary>
        /// Gets the name of the decision variable representing the chosen relation for a qualitative edge
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static string GetDVarName(this QualitativeEdge edge)
        {
            return String.Format("{0}-dvar", edge.GetUId());
        }

        /// <summary>
        /// Gets the name of the decision variable representing whether a specific relation holds for a qualitative edge
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static string GetDVarName(this QualitativeEdge edge, QualitativeRelation relation)
        {
            return String.Format("{0}-{1}-dvar", edge.GetUId(), relation.Name);
        }

        /// <summary>
        /// Gets the name of the decision variable which is used to test whether the constraint is satisfied. 
        /// </summary>
        /// <param name="constraint">The soft constraints for which is the decision variable</param>
        /// <returns></returns>
        public static string GetSatDVarName(this ConfigurationConstraint constraint)
        {
            // Hack: Currently the name of the start and end node is used for uniques identifier, which is not applicable for non-binary constraints
            if (constraint.EqualActiveConstraint == null)
            {
                return String.Format("{0}-{1}-{2}-satisfied", constraint.DomainConstraint.Name, constraint.AllowedRelations[0].GetStartNode(constraint).GetUId(), constraint.AllowedRelations[0].GetEndNode(constraint).GetUId());
            }
            else
            {
                // If the constraint is not active in the network the decision variable of its equal constraint is returned
                return constraint.EqualActiveConstraint.GetSatDVarName();
            }
        }

        /// <summary>
        /// Finds the list of satisfaction variables for the current tree.
        /// Takes in consideration the branches of the tree (if any), but does not return their decision variables.
        /// If the tree has configuration constraints returns a list of their decision variables names.
        /// </summary>
        /// <param name="constraintTree"></param>
        /// <returns></returns>
        public static List<string> GetSatDVarNames(this ConfigurationConstraintTree constraintTree)
        {
            List<string> dVarNames = new List<string>();

            if (constraintTree.IsCombination)
            {
                IEnumerable<IEnumerable<string>> dvarsCarthesianProduct = constraintTree.GetSatDVarParts();

                // Create the decision variables based on the included branch variables
                foreach (var dVarList in dvarsCarthesianProduct)
                {
                    dVarNames.Add(constraintTree.GetSatDVarName(dVarList));
                }
            }
            else
            {
                foreach (var constraint in constraintTree.ConfigurationConstraints)
                {
                    dVarNames.Add(constraint.GetSatDVarName());
                }
            }

            return dVarNames;
        }

        /// <summary>
        /// Generates the satisfaction decision variable name for the current constraint tree, when using a specific decision variables of the branches
        /// </summary>
        /// <param name="constraintTree"></param>
        /// <param name="parts">An ordered list of the chosen decision variables of the included branches</param>
        /// <returns></returns>
        public static string GetSatDVarName(this ConfigurationConstraintTree constraintTree, IEnumerable<string> parts)
        {
            return string.Format("{0}-{1}-satisfied", constraintTree.DerivedFrom.Name, parts.Aggregate((x, y) => x + "-" + y));
        }

        /// <summary>
        /// Generates a list containing the decision variables of the branches, which can be used to derive a single decision variable of the constraint tree.
        /// <para>Basically, this generates the carthesian product of the decision variables of the branches</para>
        /// This can be used to generate constraints of sort:
        /// Tree-Constraint-1-Satisfied = Branch-A-Satisfied OR Branch-B-Satisfied
        /// </summary>
        /// <param name="constraintTree"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<string>> GetSatDVarParts(this ConfigurationConstraintTree constraintTree)
        {
            if (constraintTree.IsCombination)
            {
                List<List<string>> branchDvarLists = new List<List<string>>();
                IEnumerable<IEnumerable<string>> dvarsCarthesianProduct = new[] { Enumerable.Empty<string>() };

                // For each branch we find its satisfaction decision variables
                foreach (var branch in constraintTree.ConstraintBranches)
                {
                    List<String> branchDvars = branch.GetSatDVarNames();
                    if (branchDvars.Count > 0)
                    {
                        branchDvarLists.Add(branch.GetSatDVarNames());
                    }
                }

                // Then the carthesian product of the decision variables is calculated
                dvarsCarthesianProduct = branchDvarLists.Aggregate(
                  dvarsCarthesianProduct,
                  (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));

                return dvarsCarthesianProduct;
            }

            // if this is not a combination constraint tree return empty list
            return new List<List<string>>();
        }

        /// <summary>
        /// Gets the decision variable for the metric relation part
        /// </summary>
        /// <param name="metricPart"></param>
        /// <returns></returns>
        public static string GetDVarName(this MetricRelationPart metricPart)
        {
            return String.Format("metric_val-{0}", metricPart.GetValue());
        }

        /// <summary>
        /// Finds the name of the constraint type which includes only one variable from a domain
        /// </summary>
        /// <param name="domain">The domain</param>
        /// <param name="value">The value of the domain which is (the only one) included in the constraint type</param>
        /// <returns>The TMS name of the constraint type</returns>
        public static string GetIndividualValueCTName(this GKODomainAbstract domain, string value)
        {
            return string.Format("Domain_{0}-val_{1}", domain.Name, value);
        }

        /// <summary>
        /// Finds the name of the constraint type which includes information for the powerset of the calculus
        /// </summary>
        /// <param name="calculus">The current calculus</param>
        /// <returns>The TMS name of the constraint type</returns>
        public static string GetPwSetCTName(this RelationFamily calculus)
        {
            return string.Format("Calculus_{0}-Powerset", calculus.Name);
        }

        /// <summary>
        /// Gets the name of the constraint type which specifies that exactly one out of N (the current number) boolean variables is set
        /// </summary>
        /// <returns>The TMS name of the constraint type</returns>
        public static string GetExactlyOneCTName(this int numberOfVariables)
        {
            return string.Format("One_out_of-{0}", numberOfVariables);
        }

        /// <summary>
        /// Gets the name of the constraint type which specifies that all N (the current number) boolean variables are set
        /// </summary>
        /// <returns>The TMS name of the constraint type</returns>
        public static string GetAndCTName(this int numberOfVariables)
        {
            return string.Format("And-{0}", numberOfVariables);
        }

        /// <summary>
        /// Gets the name of the constraint type which specifies that at least one out of N (the current number) boolean variables is set
        /// </summary>
        /// <returns>The TMS name of the constraint type</returns>
        public static string GetOrCTName(this int numberOfVariables)
        {
            return string.Format("Or-{0}", numberOfVariables);
        }

        /// <summary>
        /// Finds the index of the set which includes a set of relations from a calculus
        /// </summary>
        /// <param name="calculus">The calculus</param>
        /// <param name="relations">The set of relations included in the set</param>
        /// <returns>The index of the set of relations in the powerset<returns>
        public static int GetPowersetIndex(this RelationFamily calculus, List<BinaryRelation> relations)
        {
            int setIx = 0;

            foreach (var rel in relations)
            {
                // The IdInCalculus bit specifies whether the relation is included in the constraint
                setIx += (int)Math.Pow(2, rel.IdInCalculus);
            }

            return setIx;
        }

        /// <summary>
        /// Creates a TMS domain out of the current domain
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="cdaStarTms">The CS3 instance where to create the domain</param>
        /// <returns></returns>
        public static IDomain CreateIDomain(this GKODomainAbstract domain, ICDAStar cdaStarTms)
        {
            return cdaStarTms.DefDomain(domain.Name, domain.GetValues().ToArray());
        }

        /// <summary>
        /// Creates a TMS constraint type out of the current constraint type
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="cdaStarTms">The CS3 instance where to create the constraint type</param>
        /// <param name="domainsMap">A map from the SR domains to the CS3 domains</param>
        /// <returns></returns>
        public static IConstraintType CreateIConstraintType(this GKOConstraintType ct, ICDAStar cdaStarTms, Dictionary<string, IDomain> domainsMap)
        {
            IDomain[] signature = new IDomain[ct.Signature.Count];
            string[][] tuples = new string[ct.Tuples.Count][];

            for (int i = 0; i < ct.Signature.Count; i++)
            {
                signature[i] = domainsMap[ct.Signature[i].Name];
            }

            for (int i = 0; i < ct.Tuples.Count; i++)
            {
                tuples[i] = ct.Tuples[i].ToArray();
            }

            return cdaStarTms.DefConstraintType(ct.Name, signature, tuples);
        }

        /// <summary>
        /// Creates a TMS variable
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="cdaStarTms">The CS3 instance where to create the variable</param>
        /// <param name="domainsMap">A map from the SR domains to the CS3 domains</param>
        /// <returns></returns>
        public static IVariable CreateIVariable(this TmsDecisionVariable variable, ICDAStar cdaStarTms, Dictionary<string, IDomain> domainsMap)
        {
            return cdaStarTms.DefVariable(variable.Name, domainsMap[variable.Domain.Id]);
        }

        /// <summary>
        /// Creates a TMS constraint
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="cdaStarTms">The CS3 instance where to create the constraint</param>
        /// <param name="variablesMap">A map for finding CS3 variables by name</param>
        /// <param name="constraintTypesMap">A map for finding CS3 constraint types by name</param>
        /// <returns></returns>
        public static IConstraint CreateIConstraint(this TmsConstraint constraint, ICDAStar cdaStarTms, Dictionary<string, IVariable> variablesMap, Dictionary<string, IConstraintType> constraintTypesMap)
        {
            IVariable[] variables = new IVariable[constraint.VariableTuple.Count];

            for (int i = 0; i < constraint.VariableTuple.Count; i++)
            {
                variables[i] = variablesMap[constraint.VariableTuple[i]];
            }

            return cdaStarTms.DefConstraint(constraintTypesMap[constraint.ConstraintType], variables);
        }

    }
}
