using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOConstraintType
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public List<GKODomainAbstract> Signature { get; set; }
        public List<List<String>> Tuples { get; set; }
    }
}
