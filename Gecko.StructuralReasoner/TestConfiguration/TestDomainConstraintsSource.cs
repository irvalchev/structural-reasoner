using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.TestConfiguration
{
    public class TestDomainConstraintsSource : DomainConstraintsSource
    {
        public TestDomainConstraintsSource(TestKnowledgeBase kb)
            : base(kb)
        {

        }

        private void CreateDomainConstraints()
        {
            CreateDifferentPositionsConstr();
            //CreateOddBeforeEven();
            //Create6thBefore10th();
            CreateNoGapsConstr();
            //CreateIntervalStartEndConstraint();

        }

        private void CreateIntervalStartEndConstraint()
        {
            GKOAttribute startAttr = this.KnowledgeBase.Attributes[8];
            GKOAttribute endAttr = this.KnowledgeBase.Attributes[9];
            DomainConstraint domConstraint = DomainConstraint.IntervalStartBeforeEndConstraint(startAttr, endAttr, false);

            this.DomainConstraints.Add(domConstraint);
        }

        private void Create6thBefore10th()
        {
            string domainConstraintName = "Every 6th Before every 10th";
            BinaryRelation relation;
            ComponentFilter comp1Filter;
            ComponentFilter comp2Filter;
            GKOAttribute attribute;
            DomainConstraint domConstraint;

            // For all components / no filter
            comp1Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null,
                AttributeFilters = new List<AttributeFilter>()
                {
                    new AttributeFilter(this.KnowledgeBase.Attributes[3],new List<string>(){true.ToString()})
                }
            };
            comp2Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null,
                AttributeFilters = new List<AttributeFilter>()
                {
                    new AttributeFilter(this.KnowledgeBase.Attributes[0],new List<string>(){true.ToString()}),
                    new AttributeFilter(this.KnowledgeBase.Attributes[2],new List<string>(){true.ToString()}),
                }
            };
            // For the "Position" attribute
            attribute = this.KnowledgeBase.Attributes[7];
            // Not equals relations must apply
            relation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.GreaterThan);

            // Creating the domain constraint
            domConstraint = new DomainConstraint(domainConstraintName, relation, false);
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp1Filter, attribute));
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp2Filter, attribute));
            //domConstraint.DomainRelationParts.Add(new MetricDomainRelationPart(SpecialMetricValue.ComponentsCount));

            this.DomainConstraints.Add(domConstraint);
        }

        private void CreateOddBeforeEven()
        {
            string domainConstraintName = "Every Odd Before every Even";
            BinaryRelation relation;
            ComponentFilter comp1Filter;
            ComponentFilter comp2Filter;
            GKOAttribute attribute;
            DomainConstraint domConstraint;

            // For all components / no filter
            comp1Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null,
                AttributeFilters = new List<AttributeFilter>()
                {
                    new AttributeFilter(this.KnowledgeBase.Attributes[1],new List<string>(){true.ToString()})
                }
            };
            comp2Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null,
                AttributeFilters = new List<AttributeFilter>()
                {
                    new AttributeFilter(this.KnowledgeBase.Attributes[0],new List<string>(){true.ToString()})
                }
            };
            // For the "Position" attribute
            attribute = this.KnowledgeBase.Attributes[7];
            // Not equals relations must apply
            relation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.LessThan);

            // Creating the domain constraint
            domConstraint = new DomainConstraint(domainConstraintName, relation, false);
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp1Filter, attribute));
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp2Filter, attribute));
            //domConstraint.DomainRelationParts.Add(new MetricDomainRelationPart(SpecialMetricValue.ComponentsCount));

            this.DomainConstraints.Add(domConstraint);
        }

        private void CreateDifferentPositionsConstr()
        {
            string domainConstraintName = "All with different positions";
            BinaryRelation relation;
            ComponentFilter comp1Filter;
            ComponentFilter comp2Filter;
            GKOAttribute attribute;
            DomainConstraint domConstraint;

            // For all components / no filter
            comp1Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null
            };
            comp2Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null
            };
            // For the "Position" attribute
            attribute = this.KnowledgeBase.Attributes[7];
            // Not equals relations must apply
            relation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.NotEquals);

            // Creating the domain constraint
            domConstraint = new DomainConstraint(domainConstraintName, relation, false);
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp1Filter, attribute));
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp2Filter, attribute));

            this.DomainConstraints.Add(domConstraint);
        }

        private void CreateNoGapsConstr()
        {
            string domainConstraintName = "No position gaps";
            BinaryRelation relation;
            ComponentFilter comp1Filter;
            GKOAttribute attribute;
            DomainConstraint domConstraint;

            // For all components / no filter
            comp1Filter = new ComponentFilter()
            {
                Filter = null,
                ComponentTypeFilter = null
            };
            // For the "Position" attribute
            attribute = this.KnowledgeBase.Attributes[7];
            // Less than relations must apply
            relation = StructuralRelationsManager.GetRelation(RelationFamilyNames.MetricRelationsName, MetricRelationNames.LessOrEqualsN);

            // Creating the domain constraint
            domConstraint = new DomainConstraint(domainConstraintName, relation, false);
            domConstraint.DomainRelationParts.Add(new AttributeDomainRelationPart(comp1Filter, attribute));
            domConstraint.DomainRelationParts.Add(new MetricDomainRelationPart(SpecialMetricValue.ComponentsCount));

            this.DomainConstraints.Add(domConstraint);
        }

        public override void LoadDomainConstraints()
        {
            CreateDomainConstraints();
        }
    }
}
