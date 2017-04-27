using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public abstract class Edge
    {
        public Edge(ConstraintNetwork network, Node startNode, Node endNode)
        {
            if (network == null)
            {
                throw new ArgumentNullException("The constraint network of the edge cannot be null!");
            }

            this.Constraints = new List<ConfigurationConstraint>();
            this.ImpliedConstraints = new List<ConfigurationConstraint>();
            this.Network = network;
            this.StartNode = startNode;
            this.EndNode = endNode;
        }

        /// <summary>
        /// The configuration constraints applied on the edge
        /// </summary>
        public List<ConfigurationConstraint> Constraints { get; private set; }

        /// <summary>
        /// List of constraints relevant for the problem, but implied by the constraints in the <see cref="Constraints"/> 
        /// </summary>
        public List<ConfigurationConstraint> ImpliedConstraints;

        public List<ConfigurationConstraint> AllConstraints
        {
            get
            {
                return this.Constraints.Union(this.ImpliedConstraints).ToList();
            }
        }

        /// <summary>
        /// The constraint network of which the edge is part of
        /// </summary>
        public ConstraintNetwork Network { get; set; }

        /// <summary>
        /// The start node of the edge
        /// </summary>
        public Node StartNode { get; private set; }

        /// <summary>
        /// The end node of the edge
        /// </summary>
        public Node EndNode { get; private set; }
    }
}
