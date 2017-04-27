using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOComponent
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public GKOComponentType Type { get; set; }
        public Dictionary<GKOAttribute, string> AttributeValues { get; set; }

        // ToDo: See if this is the way it is implemented in GECKO
        public bool Active { get; set; }
    }
}
