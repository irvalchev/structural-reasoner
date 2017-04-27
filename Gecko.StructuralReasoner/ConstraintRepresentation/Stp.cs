using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public class Stp
    {
        /// <summary>
        /// Creates a new STP
        /// </summary>
        /// <param name="edges">The edges for the STP. Nodes will be created implicitly based on the edges</param>
        public Stp(Dictionary<Tuple<Node, Node>, Interval> edges)
        {
            this.SetEdges(edges);
        }

        /// <summary>
        /// The set of nodes
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// True if the network is consistent, otherwise False
        /// </summary>
        public bool IsConsistent
        {
            get
            {
                foreach (var node in Nodes)
                {
                    if (GetWeight(node, node) < 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// The number of self edges with negative value
        /// </summary>
        public int InconsistenciesCount
        {
            get
            {
                int count = 0;

                foreach (var node in Nodes)
                {
                    if (GetWeight(node, node) < 0)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Dictionary containing the weights between the nodes. 
        /// <para>The weight of each edge is the maximum difference between Destination and Start nodes</para>
        /// <para>The key of the dictionary is the ordered tuple - <StartNode, DestinationNode></para>
        /// </summary>
        public Dictionary<Tuple<Node, Node>, int> EdgeWeights { get; set; }

        /// <summary>
        /// Applies the Directed Path Consistency (DPC) to the network
        /// </summary>
        /// <param name="orderedNodes">The order of the nodes. The list must contain the same items as the nodes in the network</param>
        public void ApplyDPC(List<Node> orderedNodes)
        {
            for (int k = orderedNodes.Count - 1; k > 0; k--)
            {
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < k; j++)
                    {
                        // w_ij = min(w_ij, w_ik + w_kj)
                        int weight = Math.Min(GetWeight(orderedNodes[i], orderedNodes[j]),
                            GetWeight(orderedNodes[i], orderedNodes[k]) + GetWeight(orderedNodes[k], orderedNodes[j]));

                        SetWeight(orderedNodes[i], orderedNodes[j], weight);

                        // When an inconsistency is reached there is no point in continuing
                        if (i == j && weight < 0)
                        {
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Appliesh the All Pairs Shortest Path (APSP) algorithm to the network
        /// </summary>
        public void ApplyAPSP()
        {
            for (int k = 0; k < Nodes.Count; k++)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    for (int j = 0; j < Nodes.Count; j++)
                    {
                        int weightIJ = GetWeight(Nodes[i], Nodes[j]);
                        int weightIK = GetWeight(Nodes[i], Nodes[k]);
                        int weightKJ = GetWeight(Nodes[k], Nodes[j]);
                        // w_ij = min(w_ij, w_ik + w_kj)
                        int weight = Math.Min(weightIJ, weightIK + weightKJ);

                        SetWeight(Nodes[i], Nodes[j], weight);

                        // When an inconsistency is reached there is no point in continuing
                        if (i == j && weight < 0)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public Dictionary<Node, int> GetSolution()
        {
            if (!this.IsConsistent)
            {
                throw new InvalidOperationException("Cannot generate a solution for an inconsistent STP");
            }
            else
            {
                Dictionary<Node, int> solution = new Dictionary<Node, int>();
                Node beginOfTime = new Node(new GKOComponent());

                // Assigning 0 as the begining of time value
                solution.Add(beginOfTime, 0);

                foreach (var node in this.Nodes.Where(x => x != null))
                {
                    Interval allowedValues = new Interval(StructuralRelationsManager.MetricDomain.MinValue, StructuralRelationsManager.MetricDomain.MaxValue);

                    foreach (var exploredNode in solution)
                    {
                        // Finds the interval between the current node and the ones already explored
                        // Hack: The beginning of time is usually null, but dictionary does not allow it as a key, so it is translated in this statement
                        Interval relativeInterval = this.GetEdgeInterval((exploredNode.Key == beginOfTime) ? null : exploredNode.Key, node);
                        // The interval representing the actual supported value by the current node
                        Interval absoluteInterval = new Interval(relativeInterval.Start + exploredNode.Value, relativeInterval.End + exploredNode.Value);

                        // Intersects the current interval with the new one
                        allowedValues = Interval.Intersect(absoluteInterval, allowedValues);
                    }

                    if (allowedValues.IsEmpty)
                    {
                        throw new Exception("Cannot find a consistent assignment for an STP node");
                    }

                    // Assigning the first value of the interval as the active for the node
                    solution.Add(node, allowedValues.Start);
                }

                // The begin of time is not needed in the final solution
                solution.Remove(beginOfTime);
                return solution;
            }
        }

        /// <summary>
        /// Sets the edges in the network. All previous edges information is removed
        /// </summary>
        /// <param name="edges">The active edges to be set</param>
        public void SetEdges(Dictionary<Tuple<Node, Node>, Interval> edges)
        {
            Nodes = new List<Node>();
            EdgeWeights = new Dictionary<Tuple<Node, Node>, int>();

            foreach (var edgeKey in edges.Keys)
            {
                Interval edgeInterval = edges[edgeKey];

                // From STP definition
                SetWeight(edgeKey.Item1, edgeKey.Item2, edgeInterval.End);
                SetWeight(edgeKey.Item2, edgeKey.Item1, -edgeInterval.Start);

                // Adding the nodes if needed
                if (!Nodes.Contains(edgeKey.Item1))
                {
                    Nodes.Add(edgeKey.Item1);
                }
                if (!Nodes.Contains(edgeKey.Item2))
                {
                    Nodes.Add(edgeKey.Item2);
                }
            }
        }

        /// <summary>
        /// Returns the interval for the difference endNode - startNode
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public Interval GetEdgeInterval(Node startNode, Node endNode)
        {
            int intervalStart = -GetWeight(endNode, startNode);
            int intervalEnd = GetWeight(startNode, endNode);

            return new Interval(intervalStart, intervalEnd);
        }

        public int GetWeight(Node startNode, Node DestinationNode)
        {
            CheckEdge(startNode, DestinationNode);
            return EdgeWeights[Tuple.Create(startNode, DestinationNode)];
        }

        public int SetWeight(Node startNode, Node DestinationNode, int weight)
        {
            return EdgeWeights[Tuple.Create(startNode, DestinationNode)] = weight;
        }

        /// <summary>
        /// Checks whether and edge exists and creates it if needed
        /// </summary>
        /// <param name="startNode">The start node of the edge</param>
        /// <param name="DestinationNode">The end node of the edge</param>
        private void CheckEdge(Node startNode, Node DestinationNode)
        {
            Tuple<Node, Node> edgeKey = Tuple.Create(startNode, DestinationNode);

            if (!EdgeWeights.ContainsKey(edgeKey))
            {
                int weight = 0; // The weight for self edge is 0
                if (startNode != DestinationNode)
                {
                    weight = StructuralRelationsManager.MetricDomain.MaxValue - 1;
                }

                EdgeWeights.Add(edgeKey, weight);
            }
        }

    }
}
