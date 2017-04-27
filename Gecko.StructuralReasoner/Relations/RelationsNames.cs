using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Relations
{
    public struct RelationFamilyNames
    {
        public const string MetricRelationsName = "Metric relations";
        public const string IntervalAlgebraName = "Interval algebra";
        public const string PointsAlgebraName = "Points algebra";
        public const string Rcc8Name = "RCC8";
    }

    public struct MetricRelationNames
    {
        public const string AfterN = "After n";
        public const string BeforeN = "Before n";
        public const string GreaterThan = "Greater than";
        public const string GreaterThanN = "Greater than n";
        public const string GreaterOrEquals = "Greater or equals";
        public const string GreaterOrEqualsN = "Greater or equals n";
        public const string LessThan = "Less than";
        public const string LessThanN = "Less than n";
        public const string LessOrEquals = "Less or equals";
        public const string LessOrEqualsN = "Less or equals n";
        public const string Equals = "Equals";
        public const string EqualsN = "Equals n";
        public const string NotEquals = "Not equals";
        public const string NotEqualsN = "Not equals n";
    }

    public struct Rcc8RelationNames
    {
        public const string DC = "DC";
        public const string EC = "EC";
        public const string EQ = "EQ";
        public const string PO = "PO";
        public const string TPP = "TPP";
        public const string TPPi = "TPPi";
        public const string NTPP = "NTPP";
        public const string NTPPi = "NTPPi";
    }

    public struct PointsAlgebraRelationNames
    {
        public const string After = "After";
        public const string Before = "Before";
        public const string Equals = "Equals";
    }

    public struct IntervalAlgebraRelationNames
    {
        public const string Before = "Before";
        public const string After = "After";
        public const string Meets = "Meets";
        public const string MetBy = "Met-by";
        public const string Overlaps = "Overlaps";
        public const string OverlappedBy = "Overlapped-by";
        public const string During = "During";
        public const string Includes = "Includes";
        public const string Starts = "Starts";
        public const string StartedBy = "Started-by";
        public const string Finishes = "Finishes";
        public const string FinishedBy = "Finished-by";
        public const string Equals = "Equals";
    }

}
