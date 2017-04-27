using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Relations.MetricRelations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    class Tcsp
    {
        private int edgesCounts;

        public Tcsp()
        {
            Nodes = new List<Node>();
            Edges = new Dictionary<Tuple<Node, Node>, MetricEdge>();
        }

        public Tcsp(ConstraintNetwork constraintNetwork)
        {
            Nodes = constraintNetwork.Nodes;
            Edges = new Dictionary<Tuple<Node, Node>, MetricEdge>();

            foreach (var edgeKey in constraintNetwork.Edges.Keys)
            {
                MetricEdge edge = (MetricEdge)constraintNetwork.Edges[edgeKey];

                edge.GenerateAllowedIntervals();
                Edges.Add(edgeKey, edge);
            }
        }

        /// <summary>
        /// The set of nodes
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// The number of STP examined during the search
        /// </summary>
        public int ExaminedStps { get; set; }

        /// <summary>
        /// Dictionary containing the edges. 
        /// <para>The key of the dictionary is the ordered tuple - <StartNode, DestinationNode></para>
        /// </summary>
        public Dictionary<Tuple<Node, Node>, MetricEdge> Edges { get; set; }

        /// <summary>
        /// Solves the TCSP. Returns null if no consistent solution is found
        /// </summary>
        public Stp Solve()
        {
            Stp solution;

            stopwatch1.Reset();
            stopwatch55.Reset();

            //The edges count values is precalculated for speed
            this.ExaminedStps = 0;
            this.edgesCounts = this.Edges.Count;
            solution = this.SolveTcsp(new Dictionary<Tuple<Node, Node>, MetricEdge>(this.Edges), new Dictionary<Tuple<Node, Node>, Interval>());

            Debug.WriteLine("Solution DPC time elapsed (ms): " + stopwatch1.ElapsedMilliseconds);
            Debug.WriteLine("Partial DPC time elapsed (ms): " + stopwatch55.ElapsedMilliseconds);

            return solution;
        }

        Stopwatch stopwatch1 = new Stopwatch();
        Stopwatch stopwatch55 = new Stopwatch();
        Stopwatch stopwatch999 = new Stopwatch();

        /// <summary>
        /// Recursively solves a TCSP
        /// </summary>
        /// <param name="unprocessedEdges">The TCSP unprocessed edges</param>
        /// <param name="selectedLabels">The current active edge intervals</param>
        /// <returns>The first consistent solution</returns>
        private Stp SolveTcsp(Dictionary<Tuple<Node, Node>, MetricEdge> unprocessedEdges, Dictionary<Tuple<Node, Node>, Interval> selectedLabels)
        {
            if (selectedLabels.Count == this.edgesCounts)
            {
                Stp completeStp;

                stopwatch1.Start();
                completeStp = new Stp(selectedLabels);
                completeStp.ApplyDPC(completeStp.Nodes);
                stopwatch1.Stop();
                this.ExaminedStps++;
                if (completeStp.IsConsistent)
                {
                    completeStp.ApplyAPSP();
                    return completeStp;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Ordering the edges has a huge impact on the performance
                // Here the Beginning of time edges are processed first
                Tuple<Node, Node> currEdgeKey = unprocessedEdges.Keys.OrderBy(x => x.Item1).First();
                MetricEdge currEdge = unprocessedEdges[currEdgeKey];

                // Removing the explored edge
                unprocessedEdges.Remove(currEdgeKey);

                // Explore all possible intervals in the selected edge
                foreach (var interval in currEdge.AllowedIntervals)
                {
                    Stp partialStp;

                    // inserting the interval data in the selected labels
                    selectedLabels.Add(currEdgeKey, interval);

                    stopwatch55.Start();
                    partialStp = new Stp(selectedLabels);
                    partialStp.ApplyDPC(partialStp.Nodes);
                    stopwatch55.Stop();

                    // If the partial assignment is consistent continue with the assignment
                    if (partialStp.IsConsistent)
                    {
                        Stp solution = SolveTcsp(unprocessedEdges, selectedLabels);

                        // If a solution is available return it
                        if (solution != null)
                        {
                            return solution;
                        }
                        else
                        {
                            // The current labeling has to be removed before continuing
                            selectedLabels.Remove(currEdgeKey);
                        }
                    }
                    else
                    {
                        // The current labeling has to be removed before continuing
                        selectedLabels.Remove(currEdgeKey);
                    }
                }

                // If no solution is found the edge is returned to the list of unprocessed ones
                unprocessedEdges.Add(currEdgeKey, currEdge);
                return null;
            }
        }

        public string ToString()
        {
            StringBuilder description = new StringBuilder();

            foreach (var item in this.Edges)
            {
                foreach (var interval in item.Value.AllowedIntervals)
                {
                    description.AppendFormat("{0} - {1}   Interval: [{2}, {3}]", item.Key.Item1 == null ? "0" : item.Key.Item1.Component.Name, item.Key.Item2 == null ? "0" : item.Key.Item2.Component.Name, interval.Start, interval.End);
                    description.AppendLine();
                }
            }

            return description.ToString();
        }

        #region Static
        ///// <summary>
        ///// Generates the TCSP corresponding to the given constraints
        ///// </summary>
        ///// <param name="constraints">The constraints to derive the TCSP from</param>
        ///// <returns>Disjoint TCSP networks corresponding to the given constraints</returns>
        //public static List<Tcsp> GenerateTcsp(List<ConfigurationConstraint> constraints)
        //{
        //    List<Tcsp> tcsps = new List<Tcsp>();
        //    // The constraint which use only one attribute are processed later
        //    List<ConfigurationConstraint> metricConstraints = constraints.Where(x =>
        //        x.AllowedRelations[0].Signature.Count(y =>
        //                y != typeof(AttributeRelationPart)) == 1).ToList();
        //    // All other are component interconnection constraints
        //    List<ConfigurationConstraint> componentConstraints = constraints.Except(metricConstraints).ToList();

        //    // Applying component constraint
        //    // All TCSP networks should be generated in this step
        //    ApplyConstraints(componentConstraints, tcsps);

        //    // Applying the begin of time constraints
        //    // No new networks are created or merged
        //    ApplyConstraints(metricConstraints, tcsps);

        //    foreach (Tcsp tcsp in tcsps)
        //    {
        //        foreach (MetricEdge edge in tcsp.Edges.Values)
        //        {
        //            edge.GenerateAllowedIntervals();
        //        }
        //    }

        //    return tcsps;
        //}

        ///// <summary>
        ///// Applies a set of configuration constraints. It might generate or merge TCSP networks
        ///// </summary>
        ///// <param name="componentConstraints">The configuration constraints</param>
        ///// <param name="tcsps">The initial TCSP networks. They might be changed during processing</param>
        //private static void ApplyConstraints(List<ConfigurationConstraint> componentConstraints, List<Tcsp> tcsps)
        //{
        //    foreach (var constraint in componentConstraints)
        //    {
        //        IMetricRelation relation = constraint.AllowedRelations[0] as IMetricRelation;

        //        if (relation == null)
        //        {
        //            Debug.WriteLine("A constraint is skipped during the generation of TCSP, because it is not recognized as metric");
        //        }
        //        else
        //        {
        //            Tcsp combinedTcsp;
        //            Node startNode = (relation as BinaryRelation).GetStartNode(constraint);
        //            Node endNode = (relation as BinaryRelation).GetEndNode(constraint);
        //            Tuple<Node, Node> edgeKey = new Tuple<Node, Node>(startNode, endNode);
        //            List<Interval> intervals = relation.GetDifferenceIntervals(constraint);


        //            #region Selecting TCSP
        //            // null is used for a special node representing the start of time
        //            // These constraints should be applied after all others
        //            if (startNode == null || endNode == null)
        //            {
        //                combinedTcsp = tcsps.SingleOrDefault(x => (startNode != null && x.Nodes.Contains(startNode)) || (endNode != null && x.Nodes.Contains(endNode)));

        //                if (combinedTcsp == null)
        //                {
        //                    Debug.WriteLine("Cannot find the TCSP for a begin of time constraint");
        //                    continue;
        //                }
        //                else
        //                {
        //                    // Add the null node if not in the TCSP
        //                    if (!combinedTcsp.Nodes.Contains(null))
        //                    {
        //                        combinedTcsp.Nodes.Add(null);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Tcsp startNodeTcsp = tcsps.SingleOrDefault(x => x.Nodes.Contains(startNode));
        //                Tcsp endNodeTcsp = tcsps.SingleOrDefault(x => x.Nodes.Contains(endNode));

        //                if (startNodeTcsp == null && endNodeTcsp == null)
        //                {
        //                    // No TCSP with these nodes exist so a new one is created
        //                    combinedTcsp = new Tcsp();
        //                    combinedTcsp.Nodes.Add(startNode);
        //                    combinedTcsp.Nodes.Add(endNode);
        //                    tcsps.Add(combinedTcsp);
        //                }
        //                else if (startNodeTcsp != null && endNodeTcsp == null)
        //                {
        //                    // Already a TCSP exist with the start node
        //                    combinedTcsp = startNodeTcsp;
        //                    combinedTcsp.Nodes.Add(endNode);
        //                }
        //                else if (startNodeTcsp == null && endNodeTcsp != null)
        //                {
        //                    // Already a TCSP exist with the end node
        //                    combinedTcsp = endNodeTcsp;
        //                    combinedTcsp.Nodes.Add(startNode);
        //                }
        //                else if (startNodeTcsp == endNodeTcsp)
        //                {
        //                    combinedTcsp = startNodeTcsp;
        //                }
        //                else
        //                {
        //                    // The start and end node TCSPs are different so they have to be merged
        //                    combinedTcsp = new Tcsp();
        //                    combinedTcsp.Nodes.AddRange(startNodeTcsp.Nodes);
        //                    combinedTcsp.Nodes.AddRange(endNodeTcsp.Nodes);

        //                    // Adding the edges from the merged TCSPs in the new one
        //                    foreach (var key in startNodeTcsp.Edges.Keys)
        //                    {
        //                        combinedTcsp.Edges.Add(key, startNodeTcsp.Edges[key]);
        //                    }
        //                    foreach (var key in endNodeTcsp.Edges.Keys)
        //                    {
        //                        combinedTcsp.Edges.Add(key, endNodeTcsp.Edges[key]);
        //                    }

        //                    // Removing the old TCSPs and adding the combined one
        //                    tcsps.Remove(startNodeTcsp);
        //                    tcsps.Remove(endNodeTcsp);
        //                    tcsps.Add(combinedTcsp);
        //                }
        //            }
        //            #endregion

        //            if (combinedTcsp.Edges.ContainsKey(edgeKey))
        //            {
        //                // Adding the current constraint to the edge
        //                MetricEdge edge = combinedTcsp.Edges[edgeKey];

        //                edge.AddConstraintIntervals(constraint, intervals);
        //            }
        //            else
        //            {
        //                // Adding the new edge with the found intervals
        //                MetricEdge edge = new MetricEdge();

        //                edge.AddConstraintIntervals(constraint, intervals);
        //                combinedTcsp.Edges.Add(edgeKey, edge);
        //            }
        //        }
        //    }
        //}
        #endregion

    }
}
