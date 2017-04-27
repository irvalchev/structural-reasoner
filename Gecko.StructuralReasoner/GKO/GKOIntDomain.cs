using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOIntDomain : GKODomainAbstract
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int StepWidth { get; set; }

        public override List<string> GetValues()
        {
            List<string> values = new List<string>();

            for (int i = this.MinValue; i < this.MaxValue + 1; i += this.StepWidth)
            {
                values.Add(i.ToString());
            }

            return values;
        }
    }
}
