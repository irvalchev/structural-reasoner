using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public abstract class SolutionData
    {
        internal SolutionData(ConstraintNetwork network)
        {
            this.Stopwatch = new Stopwatch();
            this.Log = new Log();
            this.SolvedNetwork = network;
        }

        public ConstraintNetwork SolvedNetwork { get; private set; }

        public Log Log { get; private set; }

        /// <summary>
        /// The time when the solving process has started
        /// </summary>
        public DateTime StartTime { get; internal set; }

        /// <summary>
        /// The end time of the solving process
        /// </summary>
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Specifies whether a solution to the problem is found
        /// </summary>
        public bool SolutionFound { get; internal set; }

        /// <summary>
        /// Specifies whether the found solution is relaxation (i.e. some soft constraints were violated)
        /// </summary>
        public bool IsRelaxedSolution { get; internal set; }

        /// <summary>
        /// A helper stopwatch to be used when needed
        /// </summary>
        internal Stopwatch Stopwatch { get; set; }

        /// <summary>
        /// The solution found by the TMS
        /// </summary>
        internal TmsResult TmsSolution { get; set; }

        /// <summary>
        /// ToDo: Holds information about the reason for the inconsistency
        /// </summary>
        public Object InconsistencyExplanation { get; set; }

        //private Dictionary<ConfigurationConstraint, bool> softConstraints;
        ///// <summary>
        ///// Holds information about the soft constraints and whether they are satisfied
        ///// </summary>
        //public Dictionary<ConfigurationConstraint, bool> SoftConstraints
        //{
        //    get
        //    {
        //        if (softConstraints == null && TmsSolution != null)
        //        {
        //            PopulateSoftConstraintsSolution();
        //        }
        //        return softConstraints;
        //    }
        //    set { softConstraints = value; }
        //}

        /// <summary>
        /// Specifies whether an error has occurred during the process
        /// </summary>
        public bool ErrorOccurred
        {
            get
            {
                return this.GetErrors().Count > 0;
            }
        }

        public List<LogItem> GetErrors()
        {
            return this.Log.Items.Where(x => x.Type == LogType.Error).ToList();
        }

        /// <summary>
        /// Call this when the solution process has finished (successfully or not)
        /// </summary>
        internal void SolutionFinished()
        {
            this.EndTime = DateTime.Now;
            this.Log.AddItem(LogType.Info, string.Format("Solving network {0} complete ({1} ms)", SolvedNetwork.UId, (this.EndTime - this.StartTime).TotalMilliseconds));
            this.Log.AddItem(LogType.Info, string.Format("Solution found: {0}", this.SolutionFound));
            this.Log.AddItem(LogType.Info, string.Format("Solution solved successfully: {0}", this.ErrorOccurred ? "No (see error log)" : "Yes"));
            if (this.SolutionFound)
            {
                this.Log.AddItem(LogType.Info, string.Format("All constraints satisfied: {0}", !this.IsRelaxedSolution));
            }
        }

        //private void PopulateSoftConstraintsSolution()
        //{
        //    this.SoftConstraints = new Dictionary<ConfigurationConstraint, bool>();

        //    foreach (var edge in this.SolvedNetwork.Edges.Values)
        //    {
        //        foreach (var constraint in edge.Constraints.Where(x => x.CanBeViolated))
        //        {
        //            string satisfiedValue = TmsSolution.SoftVariables[edge.GetSoftDVarName(constraint)];

        //            this.SoftConstraints.Add(constraint, satisfiedValue == TmsManager.TrueValue);
        //        }
        //    }
        //}

    }
}
