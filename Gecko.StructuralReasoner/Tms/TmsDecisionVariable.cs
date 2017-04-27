using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Tms
{
    public class TmsDecisionVariable
    {
       /// <summary>
        /// Finds the distinct decision variables in a list
        /// </summary>
        /// <param name="variables">The list of variables</param>
        /// <param name="calculateUtility">Specifies whether to increase the utility of a variable if it is included several times</param>
        /// <returns></returns>
        internal static List<TmsDecisionVariable> GetDistinctVariables(List<TmsDecisionVariable> variables, bool calculateUtility)
        {
            List<TmsDecisionVariable> variablesFiltered = new List<TmsDecisionVariable>();

            // Removing duplicated variables
            // Duplicates can exist, because the same decision variable can be used for several equal constraints
            variables.ForEach(x =>
            {
                TmsDecisionVariable equalVariable = variablesFiltered.SingleOrDefault(y => y.Name == x.Name && x.Domain.Id == y.Domain.Id);
                if (equalVariable == null)
                {
                    variablesFiltered.Add(x);
                }
                else if (calculateUtility)
                {
                    // If the variable is already present and it is used for the utility function then increase its utility factor, since it is used for several constraints
                    if (equalVariable.UtilityFactor > 0)
                    {
                        equalVariable.UtilityFactor += x.UtilityFactor;
                    }
                }
            });

            return variablesFiltered;
        }

        internal TmsDecisionVariable(GKODomainAbstract domain, string name)
        {
            this.Domain = domain;
            this.Name = name;
            this.UtilityFactor = 0;
        }

        public GKODomainAbstract Domain { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// The importance of the variable for the TMS utility function. 
        /// It is used as a multiplier
        /// </summary>
        public int UtilityFactor { get; set; }
    }
}
