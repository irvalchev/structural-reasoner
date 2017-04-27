using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Relations
{
    class QualitativeRelation : BinaryRelation
    {
        public QualitativeRelation(string name, string meaning, List<Type> signature)
            : base(name, meaning, signature)
        { }

        public override string ToString(ConfigurationConstraint constraint)
        {
            return string.Format("A {0} B", this.Name);
        }

        /// <summary>
        /// ! Use this only once (not for every relation) per edge constraint !
        /// </summary>
        /// <param name="constraint"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        public override List<TmsConstraint> GetTmsConstraints(ConfigurationConstraint constraint, Edge edge)
        {
            ////OLD: Rem: Qualitative relations use composition constraints and constraint limiting the allowed relations between components, hence there are no constraints implied by a single relation
            //throw new NotSupportedException("A single qualitative relation do not imply a constraint!");

            List<TmsConstraint> result = new List<TmsConstraint>();
            RelationFamily relFamily = constraint.AllowedRelations[0].RelationFamily;

            if (!(edge is QualitativeEdge))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("Edge {0} is not a qualitative edge. The requested TMS constraints will not be added.", edge.GetUId()));
            }
            else if (!edge.Constraints.Contains(constraint))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("The constraint {0} is not found in the edge {1}. The requested TMS constraints will not be added.", constraint.DomainConstraint.Name, edge.GetUId()));
            }
            else if (relFamily == StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.MetricRelationsName))
            {
                edge.Network.SturcturalReasonerLog.AddItem(LogType.Warning, string.Format("The constraint {0} is metric while the needed relation for edge {1} is qualitative. The requested TMS constraints will not be added.", constraint.DomainConstraint.Name, edge.GetUId()));
            }
            else
            {
                QualitativeEdge qualEdge = (QualitativeEdge)edge;
                GKODomain calculusDomain = StructuralRelationsManager.GetDomain(constraint.AllowedRelations[0].RelationFamily.GetTmsRelationsDomainName());
                TmsConstraint exactlyOneHoldsConstraint = new TmsConstraint();
                List<string> relDvars = new List<string>(); // holds all relation variables
                GKODomain boolDomain = TmsManager.GenerateBoolDomain();
                string dVarSat = constraint.GetSatDVarName();

                // This will create a constraint sying that the edge dvar is matching the chosen relation and the relation's dvar is TRUE, or if they are not matching and the relation's dvar is FALSE
                foreach (var relation in constraint.AllowedRelations)
                {
                    TmsConstraint tmsConstraint = new TmsConstraint();
                    string edgeDVar = qualEdge.GetDVarName();
                    string edgeRelDVar = qualEdge.GetDVarName((QualitativeRelation)relation);
                    string constraintTypeName = calculusDomain.GetIndividualValueCTName(relation.Name);

                    relDvars.Add(edgeRelDVar);

                    tmsConstraint.ConstraintType = constraintTypeName;
                    tmsConstraint.VariableTuple = new List<string>() { edgeDVar, edgeRelDVar };

                    result.Add(tmsConstraint);
                }

                // This will create the constraint saying that one of the relation dvars has to be set
                // This works because the calculi are JEPD
                exactlyOneHoldsConstraint.ConstraintType = relDvars.Count.GetExactlyOneCTName();
                exactlyOneHoldsConstraint.VariableTuple = new List<string>(relDvars);
                exactlyOneHoldsConstraint.VariableTuple.Add(dVarSat);

                // This prohibits the constraint to be violated
                if (!constraint.CanBeViolated)
                {
                    result.Add(TmsConstraint.GenerateSingleValueConstraint(boolDomain, dVarSat, TmsManager.TrueValue));
                }

                result.Add(exactlyOneHoldsConstraint);
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
    }
}
