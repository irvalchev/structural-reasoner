using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class MetricSolution
    {
        internal MetricSolution()
        {
            SolutionsData = new List<SolutionDataMetric>();
        }

        public List<SolutionDataMetric> SolutionsData { get; internal set; }

        /// <summary>
        /// The combination of all attributes' assignments in the included  solved constraint networks
        /// </summary>
        public Dictionary<Node, int> AttributeAssignments
        {
            get
            {
                Dictionary<Node, int> result = new Dictionary<Node, int>();

                foreach (var solution in SolutionsData)
                {
                    if (solution.Solution != null)
                    {
                        foreach (var key in solution.Solution.Keys)
                        {
                            result.Add(key, solution.Solution[key]);
                        }
                    }
                }
                return result;
            }
        }

        public string ToString()
        {
            StringBuilder description = new StringBuilder();

            foreach (var item in AttributeAssignments)
            {
                description.AppendFormat("{0}-{1}   Value = {2}", item.Key.Component.Name, item.Key.Attribute.Name, item.Value);
                description.AppendLine();
            }

            return description.ToString();
        }
    }

    public class QualitativeSolution
    {
        internal QualitativeSolution(SolutionDataQualitative solutionInfo)
        {
            if (solutionInfo == null)
            {
                throw new ArgumentNullException("The solution information has to be set");
            }

            this.SolutionData = solutionInfo;
        }

        public SolutionDataQualitative SolutionData { get; internal set; }

        public GKOStructuringContext StructuredComponent
        {
            get
            {
                return SolutionData.SolvedNetwork.Context;
            }
        }

        public RelationFamily StructuringCalculus
        {
            get
            {
                return SolutionData.SolvedNetwork.RelationFamily;
            }
        }
    }

    public class ConfigurationStructure
    {
        internal ConfigurationStructure()
        {
            MetricStructure = new MetricSolution();
            QualitativeStructure = new List<QualitativeSolution>();
        }

        public MetricSolution MetricStructure { get; set; }
        public List<QualitativeSolution> QualitativeStructure { get; set; }

        /// <summary>
        /// Adds a new constraint network solution
        /// </summary>
        /// <param name="solutionInfo"></param>
        internal void AddSolutionInformation(SolutionData solutionInfo)
        {
            if (solutionInfo is SolutionDataQualitative)
            {
                QualitativeStructure.Add(new QualitativeSolution((SolutionDataQualitative)solutionInfo));
            }
            else if (solutionInfo is SolutionDataMetric)
            {
                MetricStructure.SolutionsData.Add((SolutionDataMetric)solutionInfo);
            }
        }

        public QualitativeSolution GetQualitativeSolution(GKOStructuringContext structuredComponent, RelationFamily calculus)
        {
            return QualitativeStructure.SingleOrDefault(x => x.StructuredComponent.Id == structuredComponent.Id && x.StructuringCalculus.Name == calculus.Name);
        }

    }
}
