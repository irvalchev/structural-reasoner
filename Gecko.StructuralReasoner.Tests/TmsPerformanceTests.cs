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
        [TestMethod/*, Timeout(5000)*/]
        public void TestPerformance()
        {
            var cdaStarTms = new Workspace();

            const int MetricMinValue = 1;
            const int NumberOfElements = 6;

            // These values are just for convenience
            const string TrueValue = "T";
            const string FalseValue = "F";
            var boolDomainValues = new string[] { TrueValue, FalseValue };
            var metricDomainValues = Enumerable.Range(MetricMinValue, NumberOfElements).Select(x => x.ToString()).ToArray();

            // Domains
            var boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues);
            var metricDomain = cdaStarTms.DefDomain("Metric", metricDomainValues);

            // Constraint types
            var ctSignature = new IDomain[] { metricDomain, metricDomain, boolDomain };
            var tuples = new List<List<string>>();
            // Not equals (!=) constraint
            tuples = new List<List<string>>();
            for (int a = MetricMinValue; a < NumberOfElements + 1; a += 1)
            {
                for (int b = MetricMinValue; b < NumberOfElements + 1; b += 1)
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
            for (int a = MetricMinValue; a < NumberOfElements + 1; a += 1)
            {
                for (int b = MetricMinValue; b < NumberOfElements + 1; b += 1)
                {
                    tuples.Add(new List<string>() { a.ToString(), b.ToString(), a > b ? TrueValue : FalseValue });
                }
            }
            var ctGreaterThan = cdaStarTms.DefConstraintType("GreaterThan", ctSignature,
                tuples.Select(x => x.ToArray()).ToArray()
            );

            // Variables and attributes
            var variables = new List<IVariable>();
            for (int i = 0; i < NumberOfElements; i++)
            {
                variables.Add(cdaStarTms.DefVariable("position_component_" + (i + 1), metricDomain));
            }
            variables.ForEach(v => cdaStarTms.DefAttribute(v.Name));        

            // Conditions and utility variables/attributes 
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
            //while (solution != null)
            {
                Debug.WriteLine("Utility: " + solution.Utility());
                Debug.WriteLine("AsString: " + solution.AsString());
                Debug.WriteLine("");

                solution = solutionIterator.NextSolution();
            }

            Debug.WriteLine("Finished in " + watch.ElapsedMilliseconds + "ms");
        }

    }
}