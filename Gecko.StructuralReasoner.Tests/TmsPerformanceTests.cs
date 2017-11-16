using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Razr.CS3.mqm;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Razr.CS3.mqm.test
{
    [TestClass]
    public class TmsPerformanceTests
    {
        /// <summary>
        /// Specifies whether to use detailed log (output in the console)
        /// </summary>
        private static bool DetailedLog = false;

        [TestMethod/*, Timeout(5000)*/]
        public void Test5Components_1Solution()
        {
            TestPerformance(5, 1, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test5Components_10Solution()
        {
            TestPerformance(5, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test5Components_100Solution()
        {
            TestPerformance(5, 100, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test6Components_1Solution()
        {
            TestPerformance(6, 1, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test6Components_10Solution()
        {
            TestPerformance(6, 10, DetailedLog);
        }

        [TestMethod/*, Timeout(5000)*/]
        public void Test6Components_100Solution()
        {
            TestPerformance(6, 100, DetailedLog);
        }

        private void TestPerformance(int numberOfElements, int maxSolutions, bool detailedLog)
        {
            var cdaStarTms = new Workspace();

            const int MetricMinValue = 1;

            // These values are just for convenience
            const string TrueValue = "T";
            const string FalseValue = "F";
            var boolDomainValues = new string[] { TrueValue, FalseValue };
            var metricDomainValues = Enumerable.Range(MetricMinValue, numberOfElements).Select(x => x.ToString()).ToArray();

            // Domains
            var boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues);
            var metricDomain = cdaStarTms.DefDomain("Metric", metricDomainValues);

            // Constraint types
            var ctSignature = new IDomain[] { metricDomain, metricDomain, boolDomain };
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
            // Greater than (>) constraint
            tuples = new List<List<string>>();
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

            // Variables and attributes
            var variables = new List<IVariable>();
            for (int i = 0; i < numberOfElements; i++)
            {
                variables.Add(cdaStarTms.DefVariable("position_component_" + (i + 1), metricDomain));
            }
            variables.ForEach(v => cdaStarTms.DefAttribute(v.Name));

            // Conditions and utility variables/attributes 
            int constraintsCount = 0;
            // Different positions
            for (int i = 1; i < variables.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    // For each constraint add new variable to check whether it is satisfied or not
                    var softVar = cdaStarTms.DefVariable("posOf_" + (j + 1) + "_notSameAs_posOf" + (i + 1), boolDomain);
                    // softVars.Add(softVar);
                    // Defining the variable utility
                    cdaStarTms.DefAttribute(softVar.Name, 1, 0);

                    // The constraint itself
                    cdaStarTms.DefConstraint(ctNotEquals, new IVariable[] { variables[j], variables[i], softVar });
                    constraintsCount++;
                }
            }

            // Utility function
            cdaStarTms.DefAggregateUtility((double[] utilities) => utilities.Sum());

            // Solving
            ISolutionsIterator solutionIterator;
            ISolution solution;

            var watch = new Stopwatch();
            watch.Start();
            solutionIterator = cdaStarTms.Solutions();
            solution = solutionIterator.FirstSolution();

            Assert.IsNotNull(solution);
            int solutionCount = 0;
            while (solutionCount < maxSolutions)
            {
                if (detailedLog)
                {
                    Debug.WriteLine("Utility: " + solution.Utility());
                    Debug.WriteLine("AsString: " + solution.AsString());
                    Debug.WriteLine("");
                }

                solution = solutionIterator.NextSolution();
                solutionCount++;
            }

            Debug.WriteLine("Constraints count: " + constraintsCount);
            Debug.WriteLine("Enumerated solutions: " + solutionCount);
            Debug.WriteLine("Finished in " + watch.ElapsedMilliseconds + "ms");
        }

    }
}