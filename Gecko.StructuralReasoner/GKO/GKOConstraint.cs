using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOConstraint
    {
        public String Id { get; set; }
        public String Type { get; set; }
        public List<String> Index { get; set; }
        public List<String> Variables { get; set; }
    }
}
