using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public class QualitativeEdge : Edge
    {
        public QualitativeEdge(ConstraintNetwork network, Node startNode, Node endNode) : base(network,startNode, endNode)
        {
            if (startNode == null || endNode == null)
            {
                throw new ArgumentNullException("The start and end nodes must be specified!");
            }
        }

        /// <summary>
        /// The current active relations between the nodes represented by the edge
        /// <para> The start node and end node are related using this relation: 
        /// StartNode ActiveRelation EndNode, i.e. ActiveRelation(StartNode,  EndNode)</para>
        /// </summary>
        public BinaryRelation ActiveRelation { get; set; }

    }
}
