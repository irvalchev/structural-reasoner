using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOConfiguration
    {
        public GKOConfiguration()
        {
            Components = new List<GKOComponent>();
            StructuringContexts = new List<GKOStructuringContext>();
        }

        public List<GKOComponent> Components { get; set; }
        public List<GKOStructuringContext> StructuringContexts { get; set; }
    }
}
