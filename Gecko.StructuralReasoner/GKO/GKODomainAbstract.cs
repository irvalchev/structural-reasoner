using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public abstract class GKODomainAbstract
    {
        public String Id { get; set; }
        public String Name { get; set; }

        public abstract List<string> GetValues();
    }
}
