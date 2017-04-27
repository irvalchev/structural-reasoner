using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Relations.MetricRelations;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public enum ConstraintNetworkType
    {
        MetricNetwork,
        QualitativeNetwork
    }

    public class ConstraintNetwork
    {
        #region static

        internal static ConstraintNetwork GenerateQualitativeConstraintNetwork(GKOStructuringContext strContext, RelationFamily calculus, Log log)
        {
            ConstraintNetwork resultNetwork = new ConstraintNetwork(calculus, strContext.Id + "_" + calculus.Name, log);
            List<ConfigurationConstraintTree> constraintTrees = strContext.StructuralConstraints.Where(x => x.RelationFamily == calculus).ToList();

            // Creating the constraints restricting that self edges should be labeled with the Equals relation from the calculus
            DomainConstraint equalsDomainConstraint = DomainConstraint.SelfEqualsConstraint(calculus);
            ConfigurationConstraintTree equalsConstraint = new ConfigurationConstraintTree(null, equalsDomainConstraint, strContext.Components.Where(x => x.Active).ToList(), log);

            // Adding the constraints restricting that self edges should be labeled with the Equals relation from the calculus
            constraintTrees.Add(equalsConstraint);

            resultNetwork.Context = strContext;
            // Adding all components as nodes in the constraint network
            strContext.Components.Where(x => x.Active).ToList().ForEach(x => resultNetwork.Nodes.Add(new Node(x)));

            // Creating all edges in the constraint network, so it is a complete graph
            foreach (var startNode in resultNetwork.Nodes)
            {
                foreach (var endNode in resultNetwork.Nodes)
                {
                    QualitativeEdge edge = new QualitativeEdge(resultNetwork, startNode, endNode);
                    resultNetwork.Edges.Add(new Tuple<Node, Node>(startNode, endNode), edge);
                }
            }

            // Each constraint tree has a number of configuration constraints
            foreach (var constraintTree in constraintTrees)
            {
                List<ConfigurationConstraint> constraints = constraintTree.GetAllConfigurationConstraints();

                // ToDo: Add check for equal constraints to avoid redundancy

                // Adding all constraints as edges in the networks
                constraints.ForEach(x =>
                    {
                        // Hack: this uses the fact that each constraint has at least one allowed relation and all have the same signature and logic (i.e. start and end node)
                        Node startNode = x.AllowedRelations[0].GetStartNode(x);
                        Node endNode = x.AllowedRelations[0].GetEndNode(x);
                        Tuple<Node, Node> edgeKey = new Tuple<Node, Node>(startNode, endNode);
                        QualitativeEdge edge = resultNetwork.Edges[edgeKey] as QualitativeEdge;

                        // Adding the constraint to the edge
                        edge.Constraints.Add(x);
                    });

            }

            return resultNetwork;
        }

        /// <summary>
        /// Generates the metric constraint network for the structured components
        /// </summary>
        /// <param name="structuringContexts">The list of structured components to generate the networks for. 
        /// They must include the metric relation family in the list of included ones</param>
        /// <returns></returns>
        internal static List<ConstraintNetwork> GenerateMetricConstraintNetworks(List<GKOStructuringContext> structuringContexts, TmsManager tms, StructuralReasonerOptions options, Log log)
        {
            List<ConstraintNetwork> constraintNetworks = new List<ConstraintNetwork>();
            List<List<Node>> clusters = new List<List<Node>>();
            List<ConfigurationConstraintTree> constraintTrees = new List<ConfigurationConstraintTree>();
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            // Adding all constraint trees to the list
            foreach (var context in structuringContexts)
            {
                context.StructuralConstraints.ForEach(x =>
                    {
                        // Additionally assign the special metric values based on the context of the constraints
                        x.AssignSpecialMetricValues(context);
                        constraintTrees.Add(x);
                    });
            }
            stopwatch.Stop();
            log.AddItem(LogType.Info, String.Format("Assigning special metric values finished ({0} ms).", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            // Combine the nodes in the constraint trees in linked clusters,
            // this serve as base for generating the constraint networks
            foreach (var constraintTree in constraintTrees)
            {
                clusters.AddRange(constraintTree.GetNodeClusters());
                ConstraintNetwork.NormalizeClusters(clusters);
            }
            stopwatch.Stop();
            log.AddItem(LogType.Info, String.Format("Finding node clusters finished ({0} ms).", stopwatch.ElapsedMilliseconds));

            #region TCSP specific
            // These constraints limit the domain of the decision variables for TCSP
            if (options.MetricReasoningAlgorithm == MetricReasoningAlgorithm.Tcsp)
            {
                stopwatch.Restart();
                List<Node> nodes = clusters.SelectMany(x => x).Where(x => x != null).Distinct().ToList();

                // Add min and max constraint for the constrained attributes
                foreach (var constrAttribute in nodes)
                {
                    List<GKOComponent> compList = new List<GKOComponent>() { constrAttribute.Component };
                    DomainConstraint minConstraint = DomainConstraint.MinValueConstraint(constrAttribute.Attribute, tms.MetricDomain);
                    DomainConstraint maxConstraint = DomainConstraint.MaxValueConstraint(constrAttribute.Attribute, tms.MetricDomain);
                    ConfigurationConstraintTree minTree = new ConfigurationConstraintTree(null, minConstraint, compList, log);
                    ConfigurationConstraintTree maxTree = new ConfigurationConstraintTree(null, maxConstraint, compList, log);

                    constraintTrees.Add(minTree);
                    constraintTrees.Add(maxTree);
                }

                stopwatch.Stop();
                log.AddItem(LogType.Info, String.Format("Creating Min/Max constraints for TCSP finished ({0} ms).", stopwatch.ElapsedMilliseconds));
            }
            #endregion

            stopwatch.Restart();

            // Generating a new constraint network for each found node cluster
            foreach (var nodeCluster in clusters)
            {
                ConstraintNetwork network = new ConstraintNetwork(StructuralRelationsManager.MetricRelationsFamily, log);
                network.Nodes = nodeCluster;

                constraintNetworks.Add(network);
            }

            // Adding all configuration constraints in the constraint networks
            foreach (var constraintTree in constraintTrees)
            {
                ConfigurationConstraint[] treeConstraints = constraintTree.GetAllConfigurationConstraints().ToArray();

                for (int i = 0; i < treeConstraints.Length; i++)
                {
                    ConstraintNetwork.ApplyMetricConstraint(treeConstraints[i], constraintNetworks, log);
                }
            }

            // UId has to be assigned to the networks
            for (int i = 0; i < constraintNetworks.Count; i++)
            {
                constraintNetworks[i].UId = "MetricConstraintNetwork_" + (i + 1).ToString();
            }

            stopwatch.Stop();
            log.AddItem(LogType.Info, String.Format("Generating the constraint networks finished ({0} ms).", stopwatch.ElapsedMilliseconds));

            return constraintNetworks;
        }

        /// <summary>
        /// Combines and removes duplicates from a list of clusters. The changes take immediate effect on the parameter list
        /// </summary>
        /// <param name="clusters">The clusters that will be combined</param>
        internal static void NormalizeClusters(List<List<Node>> clusters)
        {
            // Combining clusters
            // The loop is performed until there are no two clusters which overlap (except on the StartOfTime node, i.e. null)
            while (clusters.Any(x => clusters.Any(y => x != y && x.Any(z => z != null && y.Contains(z)))))
            {
                // We take the first cluster which is overlapped by others
                List<Node> cluster = clusters.First(x => clusters.Any(y => x != y && x.Any(z => z != null && y.Contains(z))));
                // Then find the overlapped clusters
                List<List<Node>> overlappedClusters = clusters.Where(y => cluster != y && cluster.Any(z => z != null && y.Contains(z))).ToList();

                foreach (var overlappedCluster in overlappedClusters)
                {
                    // The overlapped clusters are combined
                    cluster.AddRange(overlappedCluster);
                    // The redundant cluster is removed from the set
                    clusters.Remove(overlappedCluster);
                }
            }

            // Removing duplicated entries in the clusters
            foreach (var cluster in clusters)
            {
                List<Node> distinct = cluster.Distinct().ToList();
                cluster.Clear();
                cluster.AddRange(distinct);
            }
        }

        /// <summary>
        /// Adds a configuration constraint 
        /// </summary>
        /// <param name="constraint">The constraint to add</param>
        /// <param name="constrNetworks">The list of possible constraint networks where the constraint can e added</param>
        /// <param name="log"></param>
        private static void ApplyMetricConstraint(ConfigurationConstraint constraint, List<ConstraintNetwork> constrNetworks, Log log)
        {
            IMetricRelation relation = constraint.AllowedRelations[0] as IMetricRelation;

            if (relation == null)
            {
                log.AddItem(LogType.Warning, String.Format("A constraint ({0}) is skipped during the generation of a metric constraint network, because it is {1} relation and not recognized as metric.", constraint.DomainConstraint.Name, constraint.AllowedRelations[1].RelationFamily.Name));
            }
            else
            {
                ConstraintNetwork constraintNetwork = constrNetworks.Single(x => constraint.GetIncludedNodes().All(y => x.Nodes.Contains(y)));
                Node startNode = (relation as BinaryRelation).GetStartNode(constraint);
                Node endNode = (relation as BinaryRelation).GetEndNode(constraint);
                Tuple<Node, Node> edgeKey = new Tuple<Node, Node>(startNode, endNode);
                Tuple<Node, Node> reverseEdgeKey = new Tuple<Node, Node>(endNode, startNode);
                List<Interval> intervals = relation.GetDifferenceIntervals(constraint);

                // Check if the edge already exists
                if (constraintNetwork.Edges.ContainsKey(edgeKey))
                {
                    // Adding the current constraint to the edge
                    MetricEdge edge = constraintNetwork.Edges[edgeKey] as MetricEdge;
                    ConfigurationConstraint equalConstraint = edge.Constraints.SingleOrDefault(x => x.Equals(constraint));

                    // Add the constraint if an equal one doesn't already exist
                    if (equalConstraint == null)
                    {
                        edge.AddConstraintIntervals(constraint, intervals, log);
                    }
                    else
                    {
                        // Adding the constraints as implied by a previous one
                        edge.ImpliedConstraints.Add(constraint);
                        constraint.EqualActiveConstraint = equalConstraint;
                    }
                }
                // Check if the reverse of the edge exists
                else if (constraintNetwork.Edges.ContainsKey(reverseEdgeKey))
                {
                    // Adding the inverse constraint to the mirror edge
                    MetricEdge edge = constraintNetwork.Edges[reverseEdgeKey] as MetricEdge;
                    IMetricRelation asMetricConstraint = (IMetricRelation)constraint.AllowedRelations[0];
                    ConfigurationConstraint inverseConstraint = asMetricConstraint.GetInverseConstraint(constraint);
                    IMetricRelation inverseAsMetricConstraint = (IMetricRelation)inverseConstraint.AllowedRelations[0];
                    ConfigurationConstraint equalConstraint = edge.Constraints.SingleOrDefault(x => x.Equals(inverseConstraint));

                    // Since the inverse constraint is used, it should be included instead of the current one in the constraint tree
                    constraint.OwningTree.SwitchConstraint(constraint, inverseConstraint);

                    // Add the constraint if an equal one doesn't already exist
                    if (equalConstraint == null)
                    {
                        edge.AddConstraintIntervals(inverseConstraint, inverseAsMetricConstraint.GetDifferenceIntervals(inverseConstraint), log);
                    }
                    else
                    {
                        // Adding the constraints as implied by a previous one
                        edge.ImpliedConstraints.Add(inverseConstraint);
                        inverseConstraint.EqualActiveConstraint = equalConstraint;
                    }
                }
                // Adding the new edge with the found intervals
                else
                {
                    MetricEdge edge = new MetricEdge(constraintNetwork, edgeKey.Item1, edgeKey.Item2);

                    edge.AddConstraintIntervals(constraint, intervals, log);
                    constraintNetwork.Edges.Add(edgeKey, edge);
                }
            }
        }

        #endregion

        /// <summary>
        /// Use with caution. The UId should always be assigned in the end
        /// </summary>
        /// <param name="relFamily">The relation family for which is the network</param>
        private ConstraintNetwork(RelationFamily relFamily, Log sturcturalReasonerLog)
        {
            Nodes = new List<Node>();
            Edges = new Dictionary<Tuple<Node, Node>, Edge>();
            this.RelationFamily = relFamily;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uId">The unique identifier of the network</param>
        internal ConstraintNetwork(RelationFamily relFamily, string uId, Log sturcturalReasonerLog)
            : this(relFamily, sturcturalReasonerLog)
        {
            this.UId = uId;
        }

        private string uId;
        public string UId
        {
            get { return uId; }
            set { uId = value; }
        }

        /// <summary>
        /// The log of the structural reasoner using the constraint network
        /// </summary>
        internal Log SturcturalReasonerLog { get; private set; }

        /// <summary>
        /// The component defining the context of the operation. 
        /// It is not set for metric constraint networks
        /// </summary>
        public GKOStructuringContext Context { get; private set; }

        public RelationFamily RelationFamily { get; private set; }

        /// <summary>
        /// The set of nodes
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Dictionary containing the edges. 
        /// <para>The key of the dictionary is the ordered tuple - StartNode, DestinationNode</para>
        /// </summary>
        public Dictionary<Tuple<Node, Node>, Edge> Edges { get; set; }

        /// <summary>
        /// The type of the network - qualitative or metric
        /// </summary>
        public ConstraintNetworkType Type
        {
            get
            {
                if (this.RelationFamily == StructuralRelationsManager.MetricRelationsFamily)
                {
                    return ConstraintNetworkType.MetricNetwork;
                }
                else
                {
                    return ConstraintNetworkType.QualitativeNetwork;
                }
            }
        }

        /// <summary>
        /// Solves the constraint network
        /// </summary>
        internal SolutionData Solve(StructuralReasonerOptions options, TmsManager tms)
        {
            if (this.Type == ConstraintNetworkType.QualitativeNetwork)
            {
                return this.SolveQualitative(options, tms);
            }
            else if (this.Type == ConstraintNetworkType.MetricNetwork)
            {
                return this.SolveMetric(options, tms);
            }

            return null;
        }

        internal SolutionDataQualitative SolveQualitative(StructuralReasonerOptions options, TmsManager tms)
        {
            SolutionDataQualitative solutionInformation = new SolutionDataQualitative(this);
            List<TmsDecisionVariable> tmsVariables = new List<TmsDecisionVariable>(); // Normal TMS variables 
            List<TmsDecisionVariable> tmsUtilityVariables = new List<TmsDecisionVariable>(); // Utility TMS variables
            List<TmsConstraint> tmsConstraints = new List<TmsConstraint>();
            HashSet<ConfigurationConstraintTree> relevantConstraintTrees = new HashSet<ConfigurationConstraintTree>();
            GKODomainAbstract satisfiedDomain = tms.BoolDomain;
            GKODomainAbstract calculusDomain = tms.CalculiDomains.Single(x => x.Name == this.RelationFamily.GetTmsRelationsDomainName());

            solutionInformation.StartTime = DateTime.Now;

            //try
            //{
            solutionInformation.Stopwatch.Start();
            // For each edge adds the decion variables of the edge and generates the TMS constraints for each configuration constraint
            foreach (var edge in this.Edges.Values)
            {
                QualitativeEdge qualEdge = (edge as QualitativeEdge);
                string edgeDVar = qualEdge.GetDVarName();

                // Adding the edge decision variable
                tmsVariables.Add(new TmsDecisionVariable(calculusDomain, edgeDVar));

                // Adding the bool dvar for each relation used in a constraint of the edge
                // They are mapped to the edge dvar later with a constraint
                foreach (var relation in edge.Constraints.SelectMany(x => x.AllowedRelations).Distinct())
                {
                    tmsVariables.Add(new TmsDecisionVariable(satisfiedDomain, qualEdge.GetDVarName((QualitativeRelation)relation)));
                }

                foreach (var constraint in edge.Constraints)
                {
                    // ToDo: Make the GetTmsConstraints more consistent - now it is actually working on a constraint lvl for qualitative relations
                    // Adding the TMS constraints derivde by the relation
                    tmsConstraints.AddRange(constraint.AllowedRelations[0].GetTmsConstraints(constraint, edge));
                }

                // Adding all configuration trees, from which the configuration constraints in edge are derived, in a set. This set is used later to generate the constraints combination and utility TMS variables and TMS constraints
                foreach (var constraint in edge.AllConstraints)
                {
                    relevantConstraintTrees.Add(constraint.OwningTreeRoot);
                }
            }

            // Generating the constraints and variables for each constraint tree
            foreach (var constraintTree in relevantConstraintTrees)
            {
                tmsConstraints.AddRange(constraintTree.GenerateTmsConstraints());
                tmsVariables.AddRange(constraintTree.GetNonUtilityDecisionVariables());
                tmsUtilityVariables.AddRange(constraintTree.GetUtilityDecisionVariables());
            }

            // Since some decision variables can be duplicated, we have to find the distinct ones
            // Additionally, for utility variables their utility factor has to be recalculated as needed
            tmsVariables = TmsDecisionVariable.GetDistinctVariables(tmsVariables, false);
            tmsUtilityVariables = TmsDecisionVariable.GetDistinctVariables(tmsUtilityVariables, true);

            // Generating the composition constraints
            foreach (var nodeA in this.Nodes)
            {
                foreach (var nodeB in this.Nodes)
                {
                    foreach (var nodeC in this.Nodes)
                    {
                        QualitativeEdge edgeAB = (QualitativeEdge)this.Edges[new Tuple<Node, Node>(nodeA, nodeB)];
                        QualitativeEdge edgeBC = (QualitativeEdge)this.Edges[new Tuple<Node, Node>(nodeB, nodeC)];
                        QualitativeEdge edgeAC = (QualitativeEdge)this.Edges[new Tuple<Node, Node>(nodeA, nodeC)];
                        string edgeABDVar = edgeAB.GetDVarName();
                        string edgeBCDVar = edgeBC.GetDVarName();
                        string edgeACDVar = edgeAC.GetDVarName();
                        TmsConstraint compositionConstraint = new TmsConstraint();

                        // Creating the constraint restricting the allowed relations by the edge
                        compositionConstraint.ConstraintType = this.RelationFamily.GetTmsCompositionConstraintName();
                        compositionConstraint.VariableTuple = new List<string>() { edgeABDVar, edgeBCDVar, edgeACDVar };

                        tmsConstraints.Add(compositionConstraint);
                    }
                }
            }

            solutionInformation.Stopwatch.Stop();
            solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS variables and constraints generated ({0} ms)", solutionInformation.Stopwatch.ElapsedMilliseconds));

            solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS solution process started", solutionInformation.Stopwatch.ElapsedMilliseconds));
            solutionInformation.Log.AddItem(LogType.Info, string.Format("Variables (excl. soft): {0}", tmsVariables.Count));
            solutionInformation.Log.AddItem(LogType.Info, string.Format("Variables for soft constraints: {0}", tmsUtilityVariables.Count));
            solutionInformation.Log.AddItem(LogType.Info, string.Format("Constraints: {0}", tmsConstraints.Count));

            solutionInformation.Stopwatch.Restart();

            this.SolveWithTms(solutionInformation, tms, tmsVariables, tmsUtilityVariables, tmsConstraints);

            solutionInformation.Stopwatch.Stop();
            solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS solution process complete ({0} ms)", solutionInformation.Stopwatch.ElapsedMilliseconds));
            //}
            //catch (Exception e)
            //{
            //    solutionInformation.Log.AddItem(LogType.Error, e.ToString());
            //}

            solutionInformation.SolutionFinished();

            return solutionInformation;
        }

        internal SolutionDataMetric SolveMetric(StructuralReasonerOptions options, TmsManager tms)
        {
            SolutionDataMetric solutionInformation = new SolutionDataMetric(this);
            solutionInformation.StartTime = DateTime.Now;

            //try
            //{
            if (options.MetricReasoningAlgorithm == MetricReasoningAlgorithm.Tcsp)
            {
                Tcsp tcsp = new Tcsp(this);
                Stp solution = null;

                if (this.Edges.Values.Any(x => x.AllConstraints.Any(y => !y.OwningTreeRoot.IsValidForTcsp)))
                {
                    solutionInformation.Log.AddItem(LogType.Error, string.Format("Disjunctive domain constraints are not allowed in TCSP!", this.UId));
                }
                else
                {
                    solutionInformation.Log.AddItem(LogType.Info, string.Format("Solving metric network {0} with TCSP", this.UId));
                    solution = tcsp.Solve();
                    solutionInformation.Log.AddItem(LogType.Info, string.Format("TCSP solving process complete ({0} ms)", (DateTime.Now - solutionInformation.StartTime).TotalMilliseconds));
                }

                if (solution != null && solution.IsConsistent)
                {
                    solutionInformation.SolutionFound = true;
                    solutionInformation.Solution = solution.GetSolution();
                }
                else
                {
                    solutionInformation.SolutionFound = false;
                }
            }
            else
            {
                List<TmsDecisionVariable> tmsVariables = new List<TmsDecisionVariable>(); // Normal TMS variables 
                List<TmsDecisionVariable> tmsUtilityVariables = new List<TmsDecisionVariable>(); // Utility TMS variables
                List<TmsConstraint> tmsConstraints = new List<TmsConstraint>();
                HashSet<ConfigurationConstraintTree> relevantConstraintTrees = new HashSet<ConfigurationConstraintTree>();
                GKODomainAbstract satisfiedDomain = tms.BoolDomain;
                GKODomainAbstract metricDomain = tms.MetricDomain;

                solutionInformation.Log.AddItem(LogType.Info, string.Format("Solving metric network {0} with CDA* started", this.UId));

                solutionInformation.Stopwatch.Start();

                // Adding all node decision variables
                foreach (var node in this.Nodes)
                {
                    // The biginning of time is not relevant for the CDA*
                    if (node != null)
                    {
                        tmsVariables.Add(new TmsDecisionVariable(metricDomain, node.GetDVarName()));
                    }
                }

                // Processing configuration constraints in edges
                foreach (var edge in this.Edges.Values)
                {
                    foreach (var constraint in edge.Constraints)
                    {
                        List<MetricRelationPart> metricParts = constraint.RelationParts.Where(x => x is MetricRelationPart).Select(x => (MetricRelationPart)x).ToList();
                        TmsConstraint tmsConstraint = new TmsConstraint();

                        // Creating the TMS constraint implied by the configuration constraint 
                        tmsConstraints.AddRange(constraint.AllowedRelations[0].GetTmsConstraints(constraint, edge));

                        // Each metric part in the constraint has its own decision variable which has to be created
                        // Additionally, there is a hard constraint restricting the value of the variable to the single selected values
                        foreach (var metricPart in metricParts)
                        {
                            // If the value of the metric part is already used it will have the same decision variable name
                            if (!tmsVariables.Any(x => x.Name == metricPart.GetDVarName()))
                            {
                                tmsVariables.Add(new TmsDecisionVariable(metricDomain, metricPart.GetDVarName()));
                                tmsConstraints.Add(TmsConstraint.GenerateSingleValueConstraint(StructuralRelationsManager.MetricDomain, metricPart.GetDVarName(), metricPart.GetValue()));
                            }
                        }
                    }

                    // Adding all configuration trees, from which the configuration constraints in edge are derived, in a set. This set is used later to generate the constraints combination and utility TMS variables and TMS constraints
                    foreach (var constraint in edge.AllConstraints)
                    {
                        relevantConstraintTrees.Add(constraint.OwningTreeRoot);
                    }
                }

                // Generating the constraints and variables for each constraint tree
                foreach (var constraintTree in relevantConstraintTrees)
                {
                    tmsConstraints.AddRange(constraintTree.GenerateTmsConstraints());
                    tmsVariables.AddRange(constraintTree.GetNonUtilityDecisionVariables());
                    tmsUtilityVariables.AddRange(constraintTree.GetUtilityDecisionVariables());
                }

                // Since some decision variables can be duplicated, we have to find the distinct ones
                // Additionally, for utility variables their utility factor has to be recalculated as needed
                tmsVariables = TmsDecisionVariable.GetDistinctVariables(tmsVariables, false);
                tmsUtilityVariables = TmsDecisionVariable.GetDistinctVariables(tmsUtilityVariables, true);

                // It is possible that a variable is defined in both lists so it has to be removed from this one
                tmsVariables = tmsVariables.Where(x => !tmsUtilityVariables.Any(y => x.Name == y.Name)).ToList();

                solutionInformation.Stopwatch.Stop();
                solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS variables and constraints generated ({0} ms)", solutionInformation.Stopwatch.ElapsedMilliseconds));

                solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS solution process started", solutionInformation.Stopwatch.ElapsedMilliseconds));
                solutionInformation.Log.AddItem(LogType.Info, string.Format("Variables (excl. soft): {0}", tmsVariables.Count));
                solutionInformation.Log.AddItem(LogType.Info, string.Format("Variables for soft constraints: {0}", tmsUtilityVariables.Count));
                solutionInformation.Log.AddItem(LogType.Info, string.Format("Constraints: {0}", tmsConstraints.Count));

                solutionInformation.Stopwatch.Restart();

                this.SolveWithTms(solutionInformation, tms, tmsVariables, tmsUtilityVariables, tmsConstraints);

                solutionInformation.Stopwatch.Stop();
                solutionInformation.Log.AddItem(LogType.Info, string.Format("TMS solution process complete ({0} ms)", solutionInformation.Stopwatch.ElapsedMilliseconds));
            }
            //}
            //catch (Exception e)
            //{
            //    solutionInformation.Log.AddItem(LogType.Error, e.ToString());
            //}

            solutionInformation.SolutionFinished();

            return solutionInformation;
        }

        /// <summary>
        /// Solves the configuration problem using TMS
        /// </summary>
        /// <param name="solutionInformation">The solution information to be updated</param>
        /// <param name="tmsVariables">The normal decision variables</param>
        /// <param name="tmsSoftVariables">The decision variables used for soft constraints</param>
        /// <param name="tmsConstraints">The constraints to e satisfied</param>
        private void SolveWithTms(SolutionData solutionInformation, TmsManager tms, List<TmsDecisionVariable> tmsVariables, List<TmsDecisionVariable> tmsSoftVariables, List<TmsConstraint> tmsConstraints)
        {
            // ToDo:
            tms.Solve(tmsVariables, tmsSoftVariables, tmsConstraints, solutionInformation);
        }

        public override string ToString()
        {
            StringBuilder description = new StringBuilder();

            description.Append("--------------------NODES--------------------");
            description.AppendLine();
            foreach (var item in this.Nodes)
            {
                description.AppendFormat("{0} |  DVAR: {1}", item.GetUId(), this.Type == ConstraintNetworkType.MetricNetwork ? item.GetDVarName() : "NA");
                description.AppendLine();
            }

            description.AppendLine();
            description.Append("--------------------EDGES--------------------");
            description.AppendLine();
            foreach (var item in this.Edges)
            {
                description.AppendFormat("{0} |  DVAR: {1} |  Constraints:{2}", item.Value.GetUId(), this.Type == ConstraintNetworkType.QualitativeNetwork ? (item.Value as QualitativeEdge).GetDVarName() : "NA", item.Value.Constraints.Count);
                description.AppendLine();
            }

            return description.ToString();
        }

        public string PrintConstraints()
        {
            StringBuilder description = new StringBuilder();

            foreach (var item in this.Edges)
            {
                description.AppendLine("--------------------------------------------");
                description.AppendLine("Start Node (A): " + item.Key.Item1.GetUId());
                description.AppendLine("End Node (B): " + item.Key.Item2.GetUId());
                description.AppendLine();

                if (item.Value.Constraints.Count > 0)
                {
                    description.AppendLine(string.Format("CONSTRAINTS ({0}): ", item.Value.Constraints.Count));

                    for (int i = 0; i < item.Value.Constraints.Count; i++)
                    {
                        description.Append(string.Format("[{0}]", i + 1) + item.Value.Constraints[i].ToString());
                        description.AppendLine();
                    }
                }
                else
                {
                    description.AppendLine("NO CONSTRAINTS");
                    description.AppendLine();
                }
            }

            return description.ToString();
        }

    }
}
