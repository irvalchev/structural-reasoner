using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public class SolutionDataQualitative : SolutionData
    {
        internal SolutionDataQualitative(ConstraintNetwork network)
            : base(network)
        {
        }

        private Dictionary<Tuple<GKOComponent, GKOComponent>, BinaryRelation> solution;

        public Dictionary<Tuple<GKOComponent, GKOComponent>, BinaryRelation> Solution
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
            Solution = new Dictionary<Tuple<GKOComponent, GKOComponent>, BinaryRelation>();

            // ToDo: This might throw an exception
            foreach (var edge in SolvedNetwork.Edges.Values)
            {
                QualitativeRelation relation = this.SolvedNetwork.RelationFamily.Relations.Single(x => x.Name == TmsSolution.NormalVariables[(edge as QualitativeEdge).GetDVarName()]) as QualitativeRelation;

                Solution.Add(new Tuple<GKOComponent, GKOComponent>(edge.StartNode.Component, edge.EndNode.Component), relation);
            }
        }
    }
}
