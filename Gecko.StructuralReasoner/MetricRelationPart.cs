using Gecko.StructuralReasoner.ConstraintRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    /// <summary>
    /// Special metric values which are calculated at runtime
    /// </summary>
    public enum SpecialMetricValue
    {
        /// <summary>
        /// Will be replaced with the number of components in the relevant structured element
        /// </summary>
        ComponentsCount = -10000,
        /// <summary>
        /// Will be replaced with the number of components + 1 in the relevant structured element
        /// </summary>
        ComponentsCountPlusOne = -10001
    }

    public class MetricRelationPart : IRelationPart
    {
        private int value;
        private GKOIntDomain metricDomain;

        public MetricRelationPart(int value, GKOIntDomain metricDomainVal)
        {
            if (metricDomainVal == null)
            {
                throw new NullReferenceException("The metric domain should be specified!");
            }
            this.metricDomain = metricDomainVal;
            this.SetValue(value);
        }

        public bool IsSpecialValue
        {
            get
            {
                return this.IsSpecialMetricValue(this.value);
            }
        }

        public void SetValue(int value)
        {
            if (!this.IsSpecialMetricValue(value) && !metricDomain.ValueInDomain(value))
            {
                throw new ArgumentOutOfRangeException("The value is not valid for the specified metric domain!");
            }

            this.value = value;
        }

        public string GetValue()
        {
            if (this.IsSpecialValue)
            {
                throw new InvalidOperationException("The operation is not supported for a special metric value. First a normal value according to the context has to be assigned.");
            }
            return value.ToString();
        }

        public int GetIntValue()
        {
            if (this.IsSpecialValue)
            {
                throw new InvalidOperationException("The operation is not supported for a special metric value. First a normal value according to the context has to be assigned.");
            }
            return value;
        }

        public SpecialMetricValue GetAsSpecialValue()
        {
            return (SpecialMetricValue)value;
        }

        /// <summary>
        /// Assigns the right value according to the context. If no special value is assigned no action will be performed
        /// </summary>
        /// <param name="context">The structured component which is the context of the relation parts</param>
        public void AssignFromSpecialValue(GKOStructuringContext context)
        {
            switch ((SpecialMetricValue)value)
            {
                case SpecialMetricValue.ComponentsCount:
                    value = context.Components.Count;
                    break;
                case SpecialMetricValue.ComponentsCountPlusOne:
                    value = context.Components.Count + 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool IsSpecialMetricValue(int value)
        {
            // Rem: this values have to be updated when new special metric value is created
            switch ((SpecialMetricValue)value)
            {
                case SpecialMetricValue.ComponentsCount:
                    return true;
                case SpecialMetricValue.ComponentsCountPlusOne:
                    return true;
                default:
                    return false;
            }
        }

        public Node ToNode()
        {
            throw new NotSupportedException("Cannot create a node for a metric relation part!");
        }

        public override bool Equals(System.Object obj)
        {
            MetricRelationPart part;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            part = obj as MetricRelationPart;
            if ((System.Object)part == null)
            {
                return false;
            }

            return this.Equals(part);
        }

        public bool Equals(MetricRelationPart part)
        {
            // If parameter is null return false:
            if ((object)part == null)
            {
                return false;
            }

            if (part.IsSpecialValue != this.IsSpecialValue)
            {
                return false;
            }

            if (part.IsSpecialValue && this.IsSpecialValue)
            {
                return part.GetAsSpecialValue() == this.GetAsSpecialValue();
            }

            return part.GetIntValue() == this.GetIntValue();
        }
    }
}
