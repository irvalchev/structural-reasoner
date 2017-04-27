using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public class MetricDomainRelationPart : IDomainRelationPart
    {
        private MetricRelationPart relPart;

        /// <summary>
        /// Creates a new metric relation part
        /// </summary>
        /// <param name="value">The metric value.
        /// </param>
        /// <param name="metricDomainVal">The Domain for the value</param>
        public MetricDomainRelationPart(int value, GKOIntDomain metricDomainVal)
        {
            this.relPart = new MetricRelationPart(value, metricDomainVal);
        }

        /// <summary>
        /// Creates a new metric relation part with a special value
        /// </summary>
        /// <param name="value">The value. It will e interpretted at runtime
        /// </param>
        public MetricDomainRelationPart(SpecialMetricValue value)
        {
            this.relPart = new MetricRelationPart((int)value, StructuralRelationsManager.MetricDomain);
        }

        public List<IRelationPart> GeneratePossibleRelationParts(List<GKOComponent> componentsList, Log log)
        {
            var parts = new List<IRelationPart>();

            parts.Add(this.relPart);

            return parts;
        }
    }
}
