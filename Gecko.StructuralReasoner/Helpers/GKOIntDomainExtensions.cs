using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public static class GKOIntDomainExtensions
    {
        /// <summary>
        /// Checks if a value is in the integer domain
        /// </summary>
        /// <param name="intDomain"></param>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is in the domain, false otherwise</returns>
        public static bool ValueInDomain(this GKOIntDomain intDomain, int value)
        {
            return value >= intDomain.MinValue && value <= intDomain.MaxValue &&
                (intDomain.MinValue - value) % intDomain.StepWidth == 0; // derived from: start + n*step = value
        }
    }
}
