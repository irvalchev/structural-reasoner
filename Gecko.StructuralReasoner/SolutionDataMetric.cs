using Gecko.StructuralReasoner.ConstraintRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class SolutionDataMetric : SolutionData
    {
        internal SolutionDataMetric(ConstraintNetwork network)
            : base(network)
        {
        }

        private Dictionary<Node, int> solution;

        public Dictionary<Node, int> Solution
        {
            get
            {
                if (solution == null && this.TmsSolution != null)
                {
                    ParseTmsSolution();
                }
                return solution;
            }
            set { solution = value; }
        }

        /// <summary>
        /// Translates the TMS solution to a normal solution
        /// </summary>
        private void ParseTmsSolution()
        {
            Solution = new Dictionary<Node, int>();

            // ToDo: This might throw an exception
            foreach (var node in SolvedNetwork.Nodes)
            {
                string nodeValue = TmsSolution.NormalVariables[node.GetDVarName()];

                Solution.Add(node, int.Parse(nodeValue));
            }
        }
    }
}
