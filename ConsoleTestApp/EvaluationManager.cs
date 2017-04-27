using Gecko.StructuralReasoner;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class EvaluationManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputFolderLocation">The folder where the output of the evaluation will be saved</param>
        public EvaluationManager(string outputFolderLocation)
        {
            this.OutputFolder = outputFolderLocation;

            this.ComponentsMinCount = 2;
            this.ComponentsMaxCount = 2;
            this.ConstraintsToComponentsRatio = 1;
            this.IterationsPerComponentLevel = 1;
        }

        /// <summary>
        /// The output folder where the evaluation information will be saved
        /// </summary>
        public string OutputFolder { get; set; }

        /// <summary>
        /// The min number of components for which an evaluation will be performed
        /// </summary>
        public int ComponentsMinCount { get; set; }

        /// <summary>
        /// The max number of components for which an evaluation will be performed
        /// </summary>
        public int ComponentsMaxCount { get; set; }

        /// <summary>
        /// It is used to specify the number of constraints that will be generated based on the used components count.
        /// Number of constraints = Number of Components * <see cref="ConstraintsToComponentsRatio"/>>
        /// </summary>
        public double ConstraintsToComponentsRatio { get; set; }

        /// <summary>
        /// The number of problems that will be solved for each possible components count ([ComponentsMinCount, ComponentsMaxCount])
        /// </summary>
        public int IterationsPerComponentLevel { get; set; }

        public string PerformTemporalQualitativeToMetricEvaluation(RelationFamily temporalFamily, bool includeSoftConstraints, bool includeTCSP)
        {
            RandomProblemGenerator problemGenerator = new RandomProblemGenerator();
            StructuralReasoner reasoner = new StructuralReasoner();
            string scenario;
            string result;
            long configuringTmsTime;
            long solvingTime;
            Stopwatch stopwatch = new Stopwatch();
            List<List<string>> reportData = new List<List<string>>();
            List<string> reportDataRow;

            if (temporalFamily != StructuralRelationsManager.IntervalAlgebra && temporalFamily != StructuralRelationsManager.PointAlgebra)
            {
                throw new ArgumentException("The relation family can be only IA or PA");
            }

            // TCSP can be solved only for hard constraints
            if (includeSoftConstraints)
            {
                includeTCSP = false;
            }

            // The IA intervals have non-zero length
            problemGenerator.AllowZeroIntervals = false;
            // No combination constraints are generated
            problemGenerator.CombinationConstraintsPercentage = 0;
            // The number of constraints is based on the ratio specified by the property of this instance
            problemGenerator.ConstraintsToComponentsRatio = ConstraintsToComponentsRatio;
            // Not really relevant
            problemGenerator.MaxCombinationParts = 2;
            // Soft constraints are enabled/disabled based on the used argument
            problemGenerator.SoftConstraintsEnabled = includeSoftConstraints;

            // Setting the scenario text
            scenario = string.Format("Scenarion: {0} to metric evaluation | Allow zero-length intervals: {1} | Min number of components: {2} | Maximum number of components: {3} | Constraints to components ratio: {4} | Iterations per component level: {5} | Soft constraints enabled: {6} | Solve as TCSP: {7}", problemGenerator.RelationFamily.Name, problemGenerator.AllowZeroIntervals, ComponentsMinCount, ComponentsMaxCount, problemGenerator.ConstraintsToComponentsRatio, IterationsPerComponentLevel, problemGenerator.SoftConstraintsEnabled, includeTCSP);

            stopwatch.Start();
            // The maximum maetric value that will be reached is equal to the maximum number of components that will be generated
            reasoner.Options.MaxMetricValue = ComponentsMaxCount;
            // The TMS has to be initially configured 
            reasoner.ConfigureTms();
            stopwatch.Stop();
            configuringTmsTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();

            // Setting the headers row in the report data
            reportDataRow = new List<string>() { "Number of components", "(Qual) All constraints count", "(Qual) Combination constraints count", "(Qual) Number of networks", "(Qual) Error occurred", "(Qual) Solution found", "(Qual) Solving time in ms", "(Metric) All constraints count", "(Metric) Combination constraints count", "(CDA*) Number of networks", "(CDA*) Error occurred", "(CDA*) Solution found", "(CDA*) Solving time in ms" };
            if (includeTCSP)
            {
                reportDataRow.AddRange(new List<string>() { "(TCSP) Number of networks", "(TCSP) Error occurred", "(TCSP) Solution found", "(TCSP) Solving time in ms" });
            }

            reportData.Add(reportDataRow);

            //try
            //{
            // For each component level...
            for (int i = ComponentsMinCount; i < ComponentsMaxCount + 1; i++)
            {
                problemGenerator.NumberOfComponents = i;

                // Solve the specified number of problems
                for (int j = 1; j < IterationsPerComponentLevel + 1; j++)
                {
                    ConfigurationStructure solution;
                    reportDataRow = new List<string>();

                    // Generate and solve the IA problem
                    // No combination constraints should be generated
                    problemGenerator.CombinationConstraintsPercentage = 0;
                    // The problem family is set
                    problemGenerator.RelationFamily = temporalFamily;
                    problemGenerator.GenerateProblem();
                    solution = reasoner.Solve(problemGenerator.GeneratedProblem);

                    // Log the qualitative case
                    reportDataRow.Add(problemGenerator.NumberOfComponents.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfActualConstraints.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfActualCombinationConstraints.ToString());
                    reportDataRow.Add(solution.QualitativeStructure.Count.ToString());
                    reportDataRow.Add(solution.QualitativeStructure.Any(x => x.SolutionData.ErrorOccurred).ToString());
                    reportDataRow.Add(solution.QualitativeStructure.All(x => x.SolutionData.SolutionFound).ToString());
                    reportDataRow.Add(solution.QualitativeStructure.Sum(x => (x.SolutionData.EndTime - x.SolutionData.StartTime).Milliseconds).ToString());

                    // Generate and solve the equal metric problem with CDA*
                    problemGenerator.TransformToMetric();
                    reasoner.Options.MetricReasoningAlgorithm = MetricReasoningAlgorithm.CdaStar;
                    solution = reasoner.Solve(problemGenerator.GeneratedProblem);

                    // Lof the metric case
                    reportDataRow.Add(problemGenerator.NumberOfActualConstraints.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfActualCombinationConstraints.ToString());
                    reportDataRow.Add(solution.MetricStructure.SolutionsData.Count.ToString());
                    reportDataRow.Add(solution.MetricStructure.SolutionsData.Any(x => x.ErrorOccurred).ToString());
                    reportDataRow.Add(solution.MetricStructure.SolutionsData.All(x => x.SolutionFound).ToString());
                    reportDataRow.Add(solution.MetricStructure.SolutionsData.Sum(x => (x.EndTime - x.StartTime).Milliseconds).ToString());

                    if (includeTCSP)
                    {
                        // Solve the metric problem as TCSP
                        reasoner.Options.MetricReasoningAlgorithm = MetricReasoningAlgorithm.Tcsp;
                        solution = reasoner.Solve(problemGenerator.GeneratedProblem);

                        // Log the TCSP case
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Count.ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Any(x => x.ErrorOccurred).ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.All(x => x.SolutionFound).ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Sum(x => (x.EndTime - x.StartTime).Milliseconds).ToString());
                    }

                    reportData.Add(reportDataRow);
                }
            }

            result = "No exceptions";
            //}
            //catch (Exception ex)
            //{
            //    result = "Exception: " + ex.ToString();
            //}

            stopwatch.Stop();
            solvingTime = stopwatch.ElapsedMilliseconds;

            return WriteToFile(scenario, result, configuringTmsTime, solvingTime, reportData);
        }

        public string PerformEvaluation(RelationFamily relFamily, bool includeSoftConstraints, bool allowZeroIntervals, int combinationConstraintsPercentage, int maxCombinationParts)
        {
            RandomProblemGenerator problemGenerator = new RandomProblemGenerator();
            StructuralReasoner reasoner = new StructuralReasoner();
            string scenario;
            string result;
            long configuringTmsTime;
            long solvingTime;
            Stopwatch stopwatch = new Stopwatch();
            List<List<string>> reportData = new List<List<string>>();
            List<string> reportDataRow;

            problemGenerator.AllowZeroIntervals = allowZeroIntervals;
            problemGenerator.CombinationConstraintsPercentage = combinationConstraintsPercentage;
            problemGenerator.ConstraintsToComponentsRatio = ConstraintsToComponentsRatio;
            problemGenerator.MaxCombinationParts = maxCombinationParts;
            problemGenerator.SoftConstraintsEnabled = includeSoftConstraints;
            problemGenerator.RelationFamily = relFamily;

            // Setting the scenario text
            scenario = string.Format("Scenarion: {0} relation family | Allow zero-length intervals: {1} | Min number of components: {2} | Maximum number of components: {3} | Constraints to components ratio: {4} | Iterations per component level: {5} | Soft constraints enabled: {6} | Maximum branches in combination constraint: {7} | Combination constraint percentage: {8} ", problemGenerator.RelationFamily.Name, problemGenerator.AllowZeroIntervals, ComponentsMinCount, ComponentsMaxCount, problemGenerator.ConstraintsToComponentsRatio, IterationsPerComponentLevel, problemGenerator.SoftConstraintsEnabled, problemGenerator.MaxCombinationParts, problemGenerator.CombinationConstraintsPercentage);

            stopwatch.Start();
            // The maximum maetric value that will be reached is equal to the maximum number of components that will be generated
            reasoner.Options.MaxMetricValue = ComponentsMaxCount;
            // The TMS has to be initially configured 
            reasoner.ConfigureTms();
            stopwatch.Stop();
            configuringTmsTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();

            // Setting the headers row in the report data
            reportDataRow = new List<string>() { "Number of components", "(Expected) All constraints count", "(Actual) All constraints count", "(Expected) Combination constraints count", "(Actual) Combination constraints count", "Number of networks", "Error occurred", "Solution found", "Solving time in ms" };

            reportData.Add(reportDataRow);

            //try
            //{
            // For each component level...
            for (int i = ComponentsMinCount; i < ComponentsMaxCount + 1; i++)
            {
                problemGenerator.NumberOfComponents = i;

                // Solve the specified number of problems
                for (int j = 1; j < IterationsPerComponentLevel + 1; j++)
                {
                    ConfigurationStructure solution;
                    reportDataRow = new List<string>();

                    problemGenerator.GenerateProblem();
                    solution = reasoner.Solve(problemGenerator.GeneratedProblem);

                    // Log the qualitative case
                    reportDataRow.Add(problemGenerator.NumberOfComponents.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfConstraints.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfActualConstraints.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfCombinationConstraints.ToString());
                    reportDataRow.Add(problemGenerator.NumberOfActualCombinationConstraints.ToString());

                    if (problemGenerator.RelationFamily != StructuralRelationsManager.MetricRelationsFamily)
                    {
                        reportDataRow.Add(solution.QualitativeStructure.Count.ToString());
                        reportDataRow.Add(solution.QualitativeStructure.Any(x => x.SolutionData.ErrorOccurred).ToString());
                        reportDataRow.Add(solution.QualitativeStructure.All(x => x.SolutionData.SolutionFound).ToString());
                        reportDataRow.Add(solution.QualitativeStructure.Sum(x => (x.SolutionData.EndTime - x.SolutionData.StartTime).Milliseconds).ToString());
                    }
                    else
                    {
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Count.ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Any(x => x.ErrorOccurred).ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.All(x => x.SolutionFound).ToString());
                        reportDataRow.Add(solution.MetricStructure.SolutionsData.Sum(x => (x.EndTime - x.StartTime).Milliseconds).ToString());
                    }

                    reportData.Add(reportDataRow);
                }
            }

            result = "No exceptions";
            //}
            //catch (Exception ex)
            //{
            //    result = "Exception: " + ex.ToString();
            //}

            stopwatch.Stop();
            solvingTime = stopwatch.ElapsedMilliseconds;

            return WriteToFile(scenario, result, configuringTmsTime, solvingTime, reportData);
        }

        private string WriteToFile(string scenario, string result, long tmsConfigurationTime, long processingTime, List<List<string>> reportData)
        {
            string filePath = string.Format("{0}\\Evaluation_{1}.csv", OutputFolder, DateTime.UtcNow.ToFileTimeUtc());

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                file.WriteLine(string.Format("{0};{1};TMS configuration time: {2} ms;Solving time: {3} ms", scenario, result, tmsConfigurationTime, processingTime));

                foreach (List<string> line in reportData)
                {
                    file.WriteLine(line.Aggregate((x, y) => x + ";" + y));
                }
            }

            return filePath;
        }

    }
}
