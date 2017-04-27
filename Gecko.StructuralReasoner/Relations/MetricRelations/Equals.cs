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
    class Equals : BinaryRelation, IMetricRelation
    {
        public Equals()
            : base(
                     MetricRelationNames.Equals,
                     "A = B",
                     new List<Type>() { typeof(AttributeRelationPart), typeof(AttributeRelationPart) }
             )
        { }

        public override string ToString(ConfigurationConstraint constraint)
        {
            List<Interval> intervals = this.GetDifferenceIntervals(constraint);
            return string.Format("A = B  i.e.  B - A in {0}", intervals.Select(x => x.ToString()).Aggregate((a, b) => a + " or " + b));
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
                string dVarA = edge.StartNode.GetDVarName();
                string dVarB = edge.EndNode.GetDVarName();
                TmsConstraint tmsConstraint = new TmsConstraint();

                tmsConstraint.ConstraintType = TmsManager.CTNameEquals;
                tmsConstraint.VariableTuple = new List<string>() { dVarA, dVarB, constraint.GetSatDVarName() };

                result.Add(tmsConstraint);
            }

            return result;
        }

        public override Node GetStartNode(ConfigurationConstraint constraint)
        {
            return constraint.RelationParts[0].ToNode();
        }

        public override Node GetEndNode(ConfigurationConstraint constraint)
        {
            return constraint.RelationParts[1].ToNode();
        }

        /// <summary>
        /// Finds the implied (by a constraint) difference between the value of the two nodes as a union of intervals
        /// </summary>
        /// <param name="constraint">The constraint implying the difference</param>
        /// <returns></returns>
        public List<Interval> GetDifferenceIntervals(ConfigurationConstraint constraint)
        {
            // B-A is in [0, 0]
            Interval interval = new Interval(0, 0);
            List<Interval> intervals = new List<Interval>();
            intervals.Add(interval);

            return intervals;
        }

        public ConfigurationConstraint GetInverseConstraint(ConfigurationConstraint constraint)
        {
            ConfigurationConstraint inverseConstraint = new ConfigurationConstraint(constraint.DomainConstraint);
            List<IRelationPart> relationParts = new List<IRelationPart>();

            inverseConstraint.AllowedRelations = new List<BinaryRelation>(){ 
                StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.Equals)};

            relationParts.Add(constraint.RelationParts[1]);
            relationParts.Add(constraint.RelationParts[0]);

            inverseConstraint.SetRelationParts(relationParts);

            return inverseConstraint;
        }

    }
}
