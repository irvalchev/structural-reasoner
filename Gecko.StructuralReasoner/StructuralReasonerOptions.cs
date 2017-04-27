using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public enum MetricReasoningAlgorithm
    {
        Tcsp,
        CdaStar
    }

    public class StructuralReasonerOptions
    {
        public const int DomainConstraintMaximumBranches = 5;

        internal StructuralReasonerOptions()
        {
            this.MetricReasoningAlgorithm = MetricReasoningAlgorithm.CdaStar;
            this.MaxMetricValue = StructuralRelationsManager.MetricDomain.MaxValue;
        }

        public MetricReasoningAlgorithm MetricReasoningAlgorithm { get; set; }

        private int maxMetricValue;
        /// <summary>
        /// The max value of the considered metric domain
        /// </summary>
        public int MaxMetricValue
        {
            get { return maxMetricValue; }
            set
            {
                if (TmsConfigured)
                {
                    throw new InvalidOperationException("The option cannot be changed once the TMS has been configured");
                }
                if (value <= StructuralRelationsManager.MetricDomain.MinValue || value > StructuralRelationsManager.MetricDomain.MaxValue)
                {
                    throw new InvalidOperationException(string.Format("The max value should be in range ({0}, {1}]", StructuralRelationsManager.MetricDomain.MinValue, StructuralRelationsManager.MetricDomain.MaxValue));
                }
                maxMetricValue = value;
            }
        }

        /// <summary>
        /// Specifies whether the TMS has been configured in the StructuralReasoner for which are the options
        /// </summary>
        internal bool TmsConfigured { get; set; }

    }
}
