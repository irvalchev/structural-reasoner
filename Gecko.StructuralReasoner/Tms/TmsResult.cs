using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Tms
{
    internal class TmsResult
    {
        /// <summary>
        /// The value assignment of all decision variables. The name of the variable is used as a key
        /// </summary>
        public Dictionary<string, string> AllVariables
        {
            get
            {
                var result = new Dictionary<string, string>();
                NormalVariables.ToList().ForEach(x => result.Add(x.Key, x.Value));
                UtilityVariables.ToList().ForEach(x => result.Add(x.Key, x.Value));
                return result;
            }
        }

        /// <summary>
        /// The value assignment of normal decision variables (ie without soft ones). The name of the variable is used as a key
        /// </summary>
        public Dictionary<string, string> NormalVariables { get; set; }

        /// <summary>
        /// The value assignment of utility decision variables. The name of the variable is used as a key
        /// </summary>
        public Dictionary<string, string> UtilityVariables { get; set; }

        /// <summary>
        /// Gets the name of the variables leading to inconsistency
        /// </summary>
        public List<string> Inconsistency { get; set; }
    }
}
