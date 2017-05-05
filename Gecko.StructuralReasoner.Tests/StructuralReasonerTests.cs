using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gecko.StructuralReasoner.TestConfiguration;
using Gecko.StructuralReasoner.Relations;

namespace Gecko.StructuralReasoner.Tests
{
    [TestClass]
    public class StructuralReasonerTests
    {
        [TestMethod]
        public void ReasonerGeneralTest1()
        {
            TestKnowledgeBase kb = new TestKnowledgeBase(6);
            TestDomainConstraintsSource source = new TestDomainConstraintsSource(kb);
            GKOConfiguration configuration = new GKOConfiguration();
            StructuralReasoner reasoner;

            configuration.StructuringContexts = kb.StructuringContexts;
            configuration.Components = kb.Components;
            configuration.Components.ForEach(x => x.Active = true);

            configuration.StructuringContexts[0].Active = true;
            configuration.StructuringContexts[0].IncludedRelationFamilies.Clear();
            configuration.StructuringContexts[0].IncludedRelationFamilies.Add(StructuralRelationsManager.MetricRelationsFamily);
            //configuration.StructuringContexts[0].IncludedRelationFamilies.Add(StructuralRelationsManager.ExtendedPointAlgebra);

            reasoner = new StructuralReasoner();
            reasoner.Options.MetricReasoningAlgorithm = MetricReasoningAlgorithm.CdaStar;
            reasoner.Options.MaxMetricValue = 6;

            reasoner.ConfigureTms();
            var result = reasoner.Solve(configuration, source);
        }
    }
}
