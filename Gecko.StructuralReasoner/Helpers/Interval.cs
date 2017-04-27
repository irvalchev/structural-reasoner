using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class Interval
    {
        public Interval(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; private set; }
        public int End { get; private set; }
        public bool IsEmpty
        {
            get
            {
                return this.Start > this.End;
            }
        }

        # region Static

        /// <summary>
        /// Finds the intersection of two intervals
        /// </summary>
        /// <param name="interval1">The first inteval for the intersection</param>
        /// <param name="interval2">The second interval for the intersection</param>
        /// <returns>The intersection. It can be empty</returns>
        public static Interval Intersect(Interval interval1, Interval interval2)
        {
            int start;
            int end;

            if (interval1.IsEmpty || interval2.IsEmpty || !interval1.Overlaps(interval2))
            {
                start = 0;
                end = -1;
            }
            else
            {
                start = (interval1.Start >= interval2.Start) ? interval1.Start : interval2.Start;
                end = (interval1.End <= interval2.End) ? interval1.End : interval2.End;
            }

            return new Interval(start, end);
        }

        /// <summary>
        /// Creates a union of several intervals by doing merging where necessary
        /// </summary>
        /// <param name="intervals">The intervals to merge</param>
        /// <returns>The union list of the intervals. All empty sets will be trimmed</returns>
        public static List<Interval> Union(List<Interval> intervals)
        {
            List<Interval> merged = new List<Interval>();

            for (int i = 0; i < intervals.Count; i++)
            {
                int start = intervals[i].Start;
                int end = intervals[i].End;
                List<Interval> overlappingIntervals = merged.Where(x => x.Overlaps(intervals[i]) || (x.Start - 1) == intervals[i].End || (x.End + 1) == intervals[i].Start).ToList();

                if (overlappingIntervals.Count > 0)
                {
                    start = Math.Min(overlappingIntervals.Min(x => x.Start), start);
                    end = Math.Max(overlappingIntervals.Max(x => x.End), end);
                    // Removing the old intervals
                    overlappingIntervals.ForEach(x => merged.Remove(x));
                }

                // Adding the merged interval
                merged.Add(new Interval(start, end));
            }

            return merged.Where(x => !x.IsEmpty).ToList();
        }

        /// <summary>
        /// Intersects all intervals of a list with a particular interval
        /// </summary>
        /// <param name="mainIntervals">The list of main intervals. They should not be overlapping</param>
        /// <param name="intersectInterval">The intersection interval</param>
        /// <returns>A union of the intersected intervals. The empty sets will be removed.</returns>
        public static List<Interval> Intersect(List<Interval> mainIntervals, Interval intersectInterval)
        {
            List<Interval> intervals = new List<Interval>();

            foreach (var interval in mainIntervals)
            {
                intervals.Add(Interval.Intersect(interval, intersectInterval));
            }

            return Interval.Union(intervals);
        }

        /// <summary>
        /// Intersects all intervals of a list with another list of intervals.
        /// <para>
        /// The value from the main interval must be present in at least one of the intersect intervals to be included in the output
        /// </para>
        /// </summary>
        /// <param name="mainIntervals">The list of main intervals. They should not be overlapping</param>
        /// <param name="intersectIntervals">The intersection interval. They should not be overlapping</param>
        /// <returns>A union of the intersected intervals. The empty sets will be removed.</returns>
        public static List<Interval> Intersect(List<Interval> mainIntervals, List<Interval> intersectIntervals)
        {
            List<Interval> intervals = new List<Interval>();

            foreach (var intersectInterval in intersectIntervals)
            {
                intervals.AddRange(Interval.Intersect(mainIntervals, intersectInterval));
            }

            return Interval.Union(intervals);
        }

        /// <summary>
        /// Finds the difference between two intervals. The list is empty if the result does not contain non-empty intervals.
        /// </summary>
        /// <param name="interval1">The main interval</param>
        /// <param name="interval2">The interval to be removed from the main one</param>
        /// <returns>interval1 - interval2</returns>
        public static List<Interval> Difference(Interval interval1, Interval interval2)
        {
            List<Interval> intervals = new List<Interval>();

            if (!interval1.Overlaps(interval2) || interval2.IsEmpty)
            {
                // If the intervals do not overlap only the main is considered without modifications
                intervals.Add(new Interval(interval1.Start, interval1.End));
            }
            else
            {
                // If the second interval is included in the first we would need a split
                if (interval1.Start < interval2.Start && interval1.End > interval2.End)
                {
                    intervals.Add(new Interval(interval1.Start, interval2.Start - 1));
                    intervals.Add(new Interval(interval2.End + 1, interval1.End));
                }
                else
                {
                    int start;
                    int end;

                    if (interval1.Start >= interval2.Start)
                    {
                        // might result in empty interval
                        start = interval2.End + 1;
                        end = interval1.End;
                    }
                    else
                    {
                        // might result in empty interval
                        start = interval1.Start;
                        end = interval2.Start - 1;
                    }

                    intervals.Add(new Interval(start, end));
                }
            }

            return intervals.Where(x => !x.IsEmpty).ToList();
        }

        /// <summary>
        /// Removes an interval from a list of intervals
        /// </summary>
        /// <param name="mainIntervals">The list of main intervals. They should not be overlapping</param>
        /// <param name="diffInterval">The interval with excluded values</param>
        /// <returns>A Union of the main intervals excluding the diffInterval</returns>
        public static List<Interval> Difference(List<Interval> mainIntervals, Interval diffInterval)
        {
            List<Interval> intervals = new List<Interval>();

            if (intervals.Any(x => intervals.Any(y => x != y && x.Overlaps(y))))
            {
                throw new ArgumentException("The list of intervals should not contains overlapping intervals!");
            }

            foreach (var interval in mainIntervals)
            {
                intervals.AddRange(Interval.Difference(interval, diffInterval));
            }

            return intervals;
        }

        #endregion

        public bool Overlaps(Interval other)
        {
            return other.IsInRange(this.Start) || other.IsInRange(this.End) || this.IsInRange(other.Start) || this.IsInRange(other.End);
        }

        /// <summary>
        /// Checks if a value is in the range
        /// </summary>
        /// <param name="val">The value to check</param>
        /// <returns></returns>
        public bool IsInRange(int val)
        {
            return val >= Start && val <= End;
        }

        public string ToString()
        {

            return string.Format("[{0}, {1}]", (Math.Abs(this.Start) == StructuralRelationsManager.MetricDomain.MaxValue) ? (this.Start < 0 ? "-MAX" : "MAX") : this.Start.ToString(), (Math.Abs(this.End) == StructuralRelationsManager.MetricDomain.MaxValue) ? (this.End < 0 ? "-MAX" : "MAX") : this.End.ToString());
        }

    }
}
