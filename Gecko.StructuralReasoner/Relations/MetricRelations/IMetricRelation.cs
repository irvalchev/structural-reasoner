using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Relations.MetricRelations
{
    interface IMetricRelation
    {
        /// <summary>
        /// Finds the implied (by a constraint) difference between the value of the two nodes as a union of intervals
        /// </summary>
        /// <param name="constraint">The constraint implying the difference</param>
        /// <returns></returns>
        List<Interval> GetDifferenceIntervals(ConfigurationConstraint constraint);

        /// <summary>
        /// Generates the inverse constraint implied by the configuration constraint.
        /// In the end the constraints would have the same meaning but with switched relation elements
        /// </summary>
        /// <param name="constraint">The constraint for which to generate teh inverse</param>
        /// <returns></returns>
        ConfigurationConstraint GetInverseConstraint(ConfigurationConstraint constraint);
    }
}
