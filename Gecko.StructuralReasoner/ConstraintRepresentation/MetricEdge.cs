using Gecko.StructuralReasoner.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    class MetricEdge : Edge
    {
        public MetricEdge(ConstraintNetwork network, Node startNode, Node endNode)
            : base(network, startNode, endNode)
        {
            this.ConstraintIntervals = new Dictionary<ConfigurationConstraint, List<Interval>>();
        }

        /// <summary>
        /// Specifies whether the edge is inconsistent when TCSP reasoning is useds
        /// </summary>
        public bool IsInconsistent
        {
            get
            {
                return AllowedIntervals == null || AllowedIntervals.Count == 0 || AllowedIntervals.All(x => x.IsEmpty);
            }
        }

        /// <summary>
        /// List of allowed intervals for TCSP reasoning
        /// </summary>
        public List<Interval> AllowedIntervals { get; private set; }

        /// <summary>
        /// The dictionary contains information about the intervals which are implied by the configuration constraint used as key.
        /// <para> DO NOT add directly to the list. Use the appropriate method instead</para>
        /// </summary>
        public Dictionary<ConfigurationConstraint, List<Interval>> ConstraintIntervals { get; private set; }

        public List<List<Interval>> AllIntervals
        {
            get
            {
                return ConstraintIntervals.Select(x => x.Value).ToList();
            }
        }

        /// <summary>
        /// Assigns a list of intervals to a configuration constraint. Use this instead of directly adding new items to the property.
        /// </summary>
        /// <param name="constraint">The constraint</param>
        /// <param name="intervals">The list of intervals allowed by the constraint</param>
        public void AddConstraintIntervals(ConfigurationConstraint constraint, List<Interval> intervals, Log log)
        {
            if (!Constraints.Contains(constraint))
            {
                this.Constraints.Add(constraint);
            }

            if (!ConstraintIntervals.ContainsKey(constraint))
            {
                ConstraintIntervals.Add(constraint, intervals);
            }
            else
            {
                log.AddItem(LogType.Warning, String.Format("A constraint {0}-{1} is assigned intervals more than once", this.GetUId(), constraint.DomainConstraint.Name));
            }
        }

        /// <summary>
        /// Generates the list of allowed intervals for TCSP reasoning based on the constraints intervals
        /// </summary>
        public void GenerateAllowedIntervals()
        {
            List<List<Interval>> allIntersections = AllIntervals;
            List<Interval> allowedIntervals = new List<Interval>();

            for (int i = 0; i < allIntersections.Count; i++)
            {
                if (i == 0)
                {
                    allowedIntervals.AddRange(allIntersections[i]);
                }
                else
                {
                    allowedIntervals = Interval.Intersect(allowedIntervals, allIntersections[i]);
                }
            }

            // Some duplicates might exist, so union will remove them
            this.AllowedIntervals = Interval.Union(allowedIntervals);
        }
    }
}
