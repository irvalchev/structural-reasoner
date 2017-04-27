using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Tms;
using System.Diagnostics;
using Gecko.StructuralReasoner.Logging;

namespace Gecko.StructuralReasoner.Relations.MetricRelations
{
    class NotEqualsN : BinaryRelation, IMetricRelation
    {
        public NotEqualsN()
            : base(
                     MetricRelationNames.NotEqualsN,
                     "B != n",
                     new List<Type>() { typeof(AttributeRelationPart), typeof(MetricRelationPart) }
             )
        { }

        public override string ToString(ConfigurationConstraint constraint)
        {
            List<Interval> intervals = this.GetDifferenceIntervals(constraint);
            return string.Format("B != {0}  i.e.  B in {1}", (constraint.RelationParts[1] as MetricRelationPart).GetIntValue(), intervals.Select(x => x.ToString()).Aggregate((a, b) => a + " or " + b));
        }

        public override List<TmsConstraint> GetTmsConstraints(ConfigurationConstraint constraint, Edge edge)
        {
            List<TmsConstraint> result = new List<TmsConstraint>();
            RelationFamily relFamily = constraint.AllowedRelations[0].RelationFamily;

            if (!(edge is MetricEdge))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("Edge {0} is not a metric edge. The requested TMS constraints will not be added.", edge.GetUId()));
            }
            else if (!edge.Constraints.Contains(constraint))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("The constraint {0} is not found in the edge {1}. The requested TMS constraints will not be added.", constraint.DomainConstraint.Name, edge.GetUId()));
            }
            else if (relFamily != StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.MetricRelationsName))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("The constraint {0} is qualitative while the needed relation for edge {1} is metric. The requested TMS constraints will not be added.", constraint.DomainConstraint.Name, edge.GetUId()));
            }
            else
            {
                string dVarA = edge.EndNode.GetDVarName();
                string dVarMetric = (constraint.RelationParts[1] as MetricRelationPart).GetDVarName();
                TmsConstraint tmsConstraint = new TmsConstraint();

                tmsConstraint.ConstraintType = TmsManager.CTNameNotEquals;
                tmsConstraint.VariableTuple = new List<string>() { dVarA, dVarMetric, constraint.GetSatDVarName() };

                result.Add(tmsConstraint);
            }

            return result;
        }

        public override Node GetStartNode(ConfigurationConstraint constraint)
        {
            return null;
        }

        public override Node GetEndNode(ConfigurationConstraint constraint)
        {
            return constraint.RelationParts[0].ToNode();
        }

        /// <summary>
        /// Finds the implied (by a constraint) difference between the value of the two nodes as a union of intervals
        /// </summary>
        /// <param name="constraint">The constraint implying the difference</param>
        /// <returns></returns>
        public List<Interval> GetDifferenceIntervals(ConfigurationConstraint constraint)
        {
            MetricRelationPart relPartN = constraint.RelationParts[1] as MetricRelationPart;

            // A - BeginOfTime is in [MIN, n-1] or [n+1, MAX]
            Interval interval1 = new Interval(StructuralRelationsManager.MetricDomain.MinValue, relPartN.GetIntValue() - 1);
            Interval interval2 = new Interval(relPartN.GetIntValue() + 1, StructuralRelationsManager.MetricDomain.MaxValue);
            List<Interval> intervals = new List<Interval>();

            intervals.Add(interval1);
            intervals.Add(interval2);

            return intervals;
        }

        public ConfigurationConstraint GetInverseConstraint(ConfigurationConstraint constraint)
        {
            throw new NotSupportedException("No inverse relation is available for relation " + this.Name);
        }

    }
}
