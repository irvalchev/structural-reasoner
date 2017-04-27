using Gecko.StructuralReasoner;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.TestConfiguration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SolveRandomProblem();
            //SolveTestProblem();
        }

        private static void SolveRandomProblem()
        {
            EvaluationManager evaluation = new EvaluationManager(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            evaluation.IterationsPerComponentLevel = 1;
            evaluation.ComponentsMinCount = 15;
            evaluation.ComponentsMaxCount = 20;

            evaluation.PerformTemporalQualitativeToMetricEvaluation(StructuralRelationsManager.IntervalAlgebra, false, false);

            //evaluation.PerformEvaluation(StructuralRelationsManager.PointAlgebra, true, false, 50, 3);
            //evaluation.PerformEvaluation(StructuralRelationsManager.IntervalAlgebra, true, true, 50, 3);
            //evaluation.PerformEvaluation(StructuralRelationsManager.IntervalAlgebra, false, false, 0, 3);
            //evaluation.PerformEvaluation(StructuralRelationsManager.MetricRelationsFamily, false, true, 0, 4);
            //evaluation.PerformEvaluation(StructuralRelationsManager.MetricRelationsFamily, true, false, 70, 5);
        }

        private static void SolveTestProblem()
        {
            TestKnowledgeBase kb = new TestKnowledgeBase(2);
            TestDomainConstraintsSource source = new TestDomainConstraintsSource(kb);
            GKOConfiguration configuration = new GKOConfiguration();
            StructuralReasoner reasoner;

            //List<GKOConstraintType> types = StructuralRelationsManager.GenerateConstraintTypes();
            //int notRight = 0;

            //foreach (var type in types)
            //{
            //    foreach (var tuple in type.Tuples)
            //    {
            //        for (int i = 0; i < tuple.Count; i++)
            //        {
            //            if (type.Signature[i] is GKODomain)
            //            {
            //                GKODomain domain = type.Signature[i] as GKODomain;
            //                if (!domain.Values.Contains(tuple[i]))
            //                {
            //                    notRight++;
            //                }
            //            }
            //        }
            //    }
            //}

            configuration.StructuringContexts = kb.StructuringContexts;
            configuration.Components = kb.Components;
            configuration.Components.ForEach(x => x.Active = true);

            configuration.StructuringContexts[0].Active = true;
            configuration.StructuringContexts[0].IncludedRelationFamilies.Clear();
            configuration.StructuringContexts[0].IncludedRelationFamilies.Add(StructuralRelationsManager.MetricRelationsFamily);

            reasoner = new StructuralReasoner();
            reasoner.Options.MetricReasoningAlgorithm = MetricReasoningAlgorithm.CdaStar;
            reasoner.Options.MaxMetricValue = 35;

            reasoner.ConfigureTms();

            //reasoner.Solve(configuration, source);
        }

    }
}
