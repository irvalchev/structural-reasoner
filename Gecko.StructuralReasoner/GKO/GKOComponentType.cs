using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOComponentType
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public List<GKOAttribute> Attributes { get; set; }
        // ToDo: See if these are needed
        //public bool IsAggregated { get; set; }
        //public List<GKOStructuralRelation> StructuralRelations { get; set; }
    }
}
