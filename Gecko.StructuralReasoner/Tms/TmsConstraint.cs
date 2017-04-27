using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Tms
{
    public class TmsConstraint
    {
        #region Static

        /// <summary>
        /// Generates a TMS constraint restricting the value of a decision variable to a predefined one
        /// </summary>
        /// <param name="domain">The domain of the decision variable</param>
        /// <param name="dVarName">The decision variable</param>
        /// <param name="value">The value that the decision value is restricted to have</param>
        /// <returns>The TMS constraint</returns>
        internal static TmsConstraint GenerateSingleValueConstraint(GKODomainAbstract domain, string dVarName, string value)
        {
            TmsConstraint tmsConstraint = new TmsConstraint();
            tmsConstraint.ConstraintType = domain.GetIndividualValueCTName(value);
            tmsConstraint.VariableTuple = new List<string>() { dVarName };

            return tmsConstraint;
        }

        #endregion

        internal TmsConstraint()
        { }

        public string ConstraintType { get; set; }
        public List<String> VariableTuple { get; set; }
    }
}
