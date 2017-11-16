using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Razr.CS3.mqm;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Razr.CS3.mqm.test
{
    [TestClass]
    public class TxValchev_170726_TmsPerformanceTests
    {
        #region "Local variables"
        /// <summary>
        /// Specifies whether to use detailed log (output in the console)
        /// </summary>
        private static bool DetailedLog = true;
        private const int MetricMinValue = 1;

        // These values are just for convenience
        private static string TrueValue = "T";
        private static string FalseValue = "F";
        #endregion

        #region "Tests"

        [TestMethod/*, Timeout(5000)*/]
        public void Test_4_Components_100Solution()
        {
            TestPerformance(4, 100, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_5_Components_1Solution()
        {
            TestPerformance(5, 1, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_5_Components_10Solution()
        {
            TestPerformance(5, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_5_Components_100Solution()
        {
            TestPerformance(5, 100, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_6_Components_1Solution()
        {
            TestPerformance(6, 1, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_6_Components_10Solution()
        {
            TestPerformance(6, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_6_Components_100Solution()
        {
            TestPerformance(6, 100, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_7_Components_1Solution()
        {
            TestPerformance(7, 1, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_7_Components_10Solution()
        {
            TestPerformance(7, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_8_Components_10Solution()
        {
            TestPerformance(8, 10, DetailedLog);
        }
        [TestMethod/*, Timeout(5000)*/]
        public void Test_10_Components_10Solution()
        {
            TestPerformance(10, 10, DetailedLog);
        }
        [TestMethod/*, Timeout(5000)*/]
        public void Test_20_Components_10Solution()
        {
            TestPerformance(20, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_30_Components_10Solution()
        {
            TestPerformance(30, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test_40_Components_10Solution()
        {
            TestPerformance(40, 10, DetailedLog);
        }

        [TestMethod]
        public void Test_Setup_10()
        {
            int ccount = 0;
            SetupTest(10, false, out ccount);
        }

        [TestMethod]
        public void Test_Setup_20()
        {
            int ccount = 0;
            SetupTest(20, false, out ccount);
        }

        #endregion

        #region "Implementation"

        private Workspace SetupTest(int numberOfElements, bool detailedLog, out int constraintsCount)
        {
            var cdaStarTms = new Workspace();

            IDomain boolDomain, metricDomain;
            DefDomains(cdaStarTms, MetricMinValue, numberOfElements, out boolDomain, out metricDomain);

            IConstraintType ctNotEquals, ctGreaterThan;
            DefConstraintTypes(cdaStarTms, metricDomain, boolDomain, numberOfElements, out ctNotEquals, out ctGreaterThan);

            DefProblem(cdaStarTms, numberOfElements, detailedLog, ctNotEquals,
                       metricDomain, boolDomain, out constraintsCount);

            // Utility function

            cdaStarTms.DefAggregateUtility((double[] utilities) => utilfunction(utilities));

            return cdaStarTms;
        }

        double utilfunction(double[] utilities)
        {
            double threshold = utilities.Length * (utilities.Length + 1) / 2;
            double sum = utilities.Sum();
            if (sum > threshold)
            {
                return 0;
            }
            else
            {
                return 1 / (threshold - sum + 1);
            }
        }

        private void DefProblem(Workspace cdaStarTms, int numberOfElements, bool detailedLog, IConstraintType ctNotEquals,
                                IDomain metricDomain, IDomain boolDomain, out int constraintsCount)
        {
            // Variables and attributes
            var variables = new List<IVariable>();
            var utilities = new List<double>();
            //System.Array utilities = Array.CreateInstance(typeof(double), numberOfElements);
            for (int i = 0; i < numberOfElements; i++)
            {
                variables.Add(cdaStarTms.DefVariable("position_component_" + (i + 1), metricDomain));
                utilities.Add(i + 1);
            }
            variables.ForEach(v => cdaStarTms.DefAttribute(v.Name, utilities.ToArray()));

            // Conditions and utility variables/attributes 
            constraintsCount = 0;
            // Different positions
            for (int i = 1; i < variables.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    // For each constraint add new variable to check whether it is satisfied or not
                    var softVar = cdaStarTms.DefVariable("posOf_" + (j + 1) + "_notSameAs_posOf" + (i + 1), boolDomain);
                    // softVars.Add(softVar);
                    // Defining the variable utility
                    // cdaStarTms.DefAttribute(softVar.Name, 1, 0);

                    // The constraint itself
                    cdaStarTms.DefConstraint(ctNotEquals, new IVariable[] { variables[j], variables[i], softVar });
                    constraintsCount++;

                }
            }
        }

        private void DefDomains(Workspace cdaStarTms, int MetricMinValue, int numberOfElements, out IDomain boolDomain, out IDomain metricDomain)
        {
            var boolDomainValues = new string[] { TrueValue, FalseValue };
            var metricDomainValues = Enumerable.Range(MetricMinValue, numberOfElements).Select(x => x.ToString()).ToArray();
            boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues);
            metricDomain = cdaStarTms.DefDomain("Metric", metricDomainValues);
        }

        private void DefConstraintTypes(Workspace cdaStarTms, IDomain metricDomain, IDomain boolDomain, int numberOfElements,
                                        out IConstraintType ctNotEquals, out IConstraintType ctGreaterThan)
        {
            var ctSignature = constraintSignature(metricDomain, boolDomain);
            ctNotEquals = DefNotEquals(cdaStarTms, ctSignature, numberOfElements);
            ctGreaterThan = DefGreaterThan(cdaStarTms, ctSignature, numberOfElements);
        }

        private IDomain[] constraintSignature(IDomain metricDomain, IDomain boolDomain)
        {
            return new IDomain[] { metricDomain, metricDomain, boolDomain };
        }

        private IConstraintType DefNotEquals(Workspace cdaStarTms, IDomain[] ctSignature, int numberOfElements)
        {
            var tuples = new List<List<string>>();
            // Not equals (!=) constraint
            tuples = new List<List<string>>();
            for (int a = MetricMinValue; a < numberOfElements + 1; a += 1)
            {
                for (int b = MetricMinValue; b < numberOfElements + 1; b += 1)
                {
                    // in some cases the non-satisfied tuples are added: *, *, 0
                    // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                    tuples.Add(new List<string>() { a.ToString(), b.ToString(), a != b ? TrueValue : FalseValue });
                }
            }
            var ctNotEquals = cdaStarTms.DefConstraintType("NotEquals", ctSignature,
                tuples.Select(x => x.ToArray()).ToArray()
            );
            return ctNotEquals;
        }

        private IConstraintType DefGreaterThan(Workspace cdaStarTms, IDomain[] ctSignature, int numberOfElements)
        {
            var tuples = new List<List<string>>();
            for (int a = MetricMinValue; a < numberOfElements + 1; a += 1)
            {
                for (int b = MetricMinValue; b < numberOfElements + 1; b += 1)
                {
                    tuples.Add(new List<string>() { a.ToString(), b.ToString(), a > b ? TrueValue : FalseValue });
                }
            }
            var ctGreaterThan = cdaStarTms.DefConstraintType("GreaterThan", ctSignature,
                tuples.Select(x => x.ToArray()).ToArray()
            );
            return ctGreaterThan;
        }

        private void TestPerformance(int numberOfElements, int maxSolutions, bool detailedLog)
        {
            int constraintsCount = 0;
            Workspace cdaStarTms = SetupTest(numberOfElements, detailedLog, out constraintsCount);

            // Solving
            ISolutionsIterator solutionIterator;
            ISolution solution;

            var watch = new Stopwatch();
            watch.Start();
            solutionIterator = cdaStarTms.Solutions();
            solution = solutionIterator.FirstSolution();
            int solutionCount = 0;
            Assert.IsNotNull(solution);
            if (solution != null)
            {
                solutionCount++;
            }
            Trace.WriteLine(solutionCount + " : " + watch.ElapsedMilliseconds + "ms");
            while (solutionCount < maxSolutions && solution != null)
            {
                if (detailedLog)
                {
                    Debug.WriteLine("Utility: " + solution.Utility());
                    Debug.WriteLine("AsString: " + solution.AsString());
                    Debug.WriteLine("");
                }
                var usedPositions = new List<int>();

                for (int i = 1; i < numberOfElements + 1; i++)
                {
                    int value = solution.Value("position_component_" + i);
                    // No other element should have the same position
                    Assert.IsFalse(usedPositions.Any(x => x == value));
                    usedPositions.Add(value);
                }


                solution = solutionIterator.NextSolution();
                solutionCount++;
                Trace.WriteLine(solutionCount + " : " + watch.ElapsedMilliseconds + "ms");
            }

            if (detailedLog)
            {
                Debug.WriteLine("Constraints count: " + constraintsCount);
                Debug.WriteLine("Enumerated solutions: " + solutionCount);
                Debug.WriteLine("Finished in " + watch.ElapsedMilliseconds + "ms");
            }
        }

        #endregion

    }

}