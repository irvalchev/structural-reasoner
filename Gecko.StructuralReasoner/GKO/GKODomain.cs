using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKODomain : GKODomainAbstract
    {
        public List<String> Values { get; set; }

        public override List<string> GetValues()
        {
            return this.Values;
        }
    }
}
