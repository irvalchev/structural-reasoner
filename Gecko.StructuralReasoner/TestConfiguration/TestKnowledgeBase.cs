using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.TestConfiguration
{
    /// <summary>
    /// <para> ----------------------------------DOMAINS: </para>
    /// <para> 0: Bool - True, False </para>
    /// <para> 1..n: StructuralRelationsManager.GKODomains </para>  
    /// <para> ----------------------------------INT DOMAINS: </para>
    /// <para> 0: StructuralRelationsManager.MetricDomain </para>
    /// <para> ----------------------------------ATTRIBUTES: </para>
    /// <para> 0: Even (Bool) - every even element has the property  </para>
    /// <para> 1: Odd (Bool) - every odd element has the property  </para>
    /// <para> 2: OneIn3 (Bool) - every 3rd element has the property  </para>
    /// <para> 3: OneIn10 (Bool) - every 10th element has the property  </para>
    /// <para> 4: OneIn50 (Bool) - every 50th element has the property  </para>
    /// <para> 5: OneIn100 (Bool) - every 100th element has the property  </para>
    /// <para> 6: OneIn300 (Bool) - every 300th element has the property  </para>
    /// <para> 7: Position (Int) - metric attribute  </para>
    /// <para> 8: Start (Int) - metric attribute  </para>
    /// <para> 9: End (Int) - metric attribute  </para>
    /// <para> ----------------------------------COMPONENT TYPES: </para>
    /// <para> 0: CT1 - all attributes  </para>
    /// <para> 1: CT2 - all attributes  </para>
    /// <para> ----------------------------------COMPONENTS: </para>
    /// <para> n - defined in the constructor </para>
    /// <para> n Components of CT1 </para>
    /// <para> n Components of CT2 </para>
    /// <para> ----------------------------------STRUCTURED COMPONENTS: </para>
    /// <para> 0: the first n CT1 components </para>
    /// <para> 1: the first n CT2 components </para>
    /// <para> 2: the first n CT1 and n CT2 components </para>
    /// <para> ----------------------------------CONSTRAINT TYPES: </para>
    /// <para> Not set </para>
    /// </summary>
    public class TestKnowledgeBase : KnowledgeBase
    {
        public TestKnowledgeBase(int componentsOfEachType)
        {
            CreateTestKb(componentsOfEachType);
        }

        private void CreateTestKb(int componentsOfEachType)
        {
            CreateDomains();
            CreateIntDomains();
            CreateAttributes();
            CreateComponentTypes();
            CreateComponents(componentsOfEachType);
        }

        private void CreateDomains()
        {
            this.Domains.Add(new GKODomain()
            {
                Id = "Bool",
                Name = "Bool",
                Values = new List<string>() { "True", "False" }
            });

            this.Domains.AddRange(StructuralRelationsManager.CalculiDomains);
        }

        private void CreateIntDomains()
        {
            this.IntDomains.Add(StructuralRelationsManager.MetricDomain);
        }

        private void CreateAttributes()
        {
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "Even",
                Name = "Even",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "Odd",
                Name = "Odd",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "OneIn3",
                Name = "OneIn3",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "OneIn10",
                Name = "OneIn10",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "OneIn50",
                Name = "OneIn50",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "OneIn100",
                Name = "OneIn100",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "OneIn300",
                Name = "OneIn300",
                Type = "",
                Domain = this.Domains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "Position",
                Name = "Position",
                Type = "",
                Domain = this.IntDomains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "Start",
                Name = "Start",
                Type = "",
                Domain = this.IntDomains[0].Name
            });
            this.Attributes.Add(new GKOAttribute()
            {
                Id = "End",
                Name = "End",
                Type = "",
                Domain = this.IntDomains[0].Name
            });
        }

        private void CreateComponentTypes()
        {
            this.ComponentTypes.Add(new GKOComponentType()
            {
                Id = "CT1",
                Name = "CT1",
                Attributes = this.Attributes
            });

            this.ComponentTypes.Add(new GKOComponentType()
            {
                Id = "CT2",
                Name = "CT2",
                Attributes = this.Attributes
            });
        }

        private void CreateComponents(int componentsOfEachType)
        {
            for (int i = 0; i < componentsOfEachType; i++)
            {
                Dictionary<GKOAttribute, string> attributeValues = new Dictionary<GKOAttribute, string>();
                attributeValues.Add(this.Attributes[0], ((i + 1) % 2 == 0).ToString());
                attributeValues.Add(this.Attributes[1], ((i + 1) % 2 == 1).ToString());
                attributeValues.Add(this.Attributes[2], ((i + 1) % 3 == 0).ToString());
                attributeValues.Add(this.Attributes[3], ((i + 1) % 10 == 0).ToString());
                attributeValues.Add(this.Attributes[4], ((i + 1) % 50 == 0).ToString());
                attributeValues.Add(this.Attributes[5], ((i + 1) % 100 == 0).ToString());
                attributeValues.Add(this.Attributes[6], ((i + 1) % 300 == 0).ToString());
                attributeValues.Add(this.Attributes[7], "0");
                attributeValues.Add(this.Attributes[8], "0");
                attributeValues.Add(this.Attributes[9], "0");

                this.Components.Add(new GKOComponent()
                {
                    Active = false,
                    Id = "Comp1_" + (i + 1),
                    Name = "Comp1_" + (i + 1),
                    Type = this.ComponentTypes[0],
                    AttributeValues = attributeValues
                });
                this.Components.Add(new GKOComponent()
                {
                    Active = false,
                    Id = "Comp2_" + (i + 1),
                    Name = "Comp2_" + (i + 1),
                    Type = this.ComponentTypes[1],
                    AttributeValues = attributeValues
                });
            }

            this.StructuringContexts.Add(new GKOStructuringContext("StrComp1", new List<RelationFamily>() { StructuralRelationsManager.MetricRelationsFamily, StructuralRelationsManager.PointAlgebra })
            {
                Active = false,
                Components = this.Components.Where(x => x.Type == this.ComponentTypes[0]).ToList()
            });
            this.StructuringContexts.Add(new GKOStructuringContext("StrComp2", new List<RelationFamily>() { StructuralRelationsManager.MetricRelationsFamily, StructuralRelationsManager.IntervalAlgebra })
            {
                Active = false,
                Components = this.Components.Where(x => x.Type == this.ComponentTypes[1]).ToList()
            });
            this.StructuringContexts.Add(new GKOStructuringContext("StrComp3", new List<RelationFamily>() { StructuralRelationsManager.Rcc8Calculus })
            {
                Active = false,
                Components = this.Components
            });
        }

    }
}
