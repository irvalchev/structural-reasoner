using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Relations;
using Gecko.StructuralReasoner.Relations.MetricRelations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Tms;

namespace Gecko.StructuralReasoner
{
    public class StructuralReasoner
    {
        private TmsManager tms;

        public StructuralReasoner()
        {
            this.ProcessLog = new Log();
            this.Options = new StructuralReasonerOptions();
        }

        /// <summary>
        /// Process options
        /// </summary>
        public StructuralReasonerOptions Options { get; set; }

        public Log ProcessLog { get; set; }

        /// <summary>
        /// Configures the internal TMS used to solve the structuring problems.
        /// <para>
        /// It is required to call this method once after the relevant options have been set
        /// </para>
        /// <para>Once this operation is complete some of the process options cannot be changed anymore, e.g. 
        /// metric domain max value, soft constraints inclusion
        /// </para>
        /// </summary>
        public void ConfigureTms()
        {
            Stopwatch stopwatch = new Stopwatch();

            ProcessLog.AddItem(LogType.Info, String.Format("Starting TMS configuration"));

            stopwatch.Start();
            this.tms = new TmsManager(this.Options, this.ProcessLog);
            this.Options.TmsConfigured = true;
            stopwatch.Stop();

            ProcessLog.AddItem(LogType.Info, String.Format("TMS configuration finished ({0} ms)", stopwatch.ElapsedMilliseconds));
        }

        public ConfigurationStructure Solve(ProblemDescription problem)
        {
            return Solve(problem.Configuration, problem.ConstraintSource);
        }

        /// <summary>
        /// Solves the current problem given the configuration and the domain constraints
        /// </summary>
        /// <param name="configuration">The configuration to be structured</param>
        /// <param name="constraintsSource">The source for the domain constraints</param>
        /// <returns>The best found configuration structure given the constraints</returns>
        public ConfigurationStructure Solve(GKOConfiguration configuration, DomainConstraintsSource constraintsSource)
        {
            ConfigurationStructure structure = new ConfigurationStructure();
            List<GKOStructuringContext> metricStructuredComponents;
            List<GKOStructuringContext> qualitativeStructuredComponents;
            List<ConstraintNetwork> problemNetworks = new List<ConstraintNetwork>();
            RelationFamily metricRelations = StructuralRelationsManager.MetricRelationsFamily;
            Stopwatch totalTime = new Stopwatch();
            Stopwatch stopwatch = new Stopwatch();

            if (!this.Options.TmsConfigured)
            {
                throw new NotSupportedException("The TMS has to be configured before a structuring operation!");
            }
            if (configuration == null)
            {
                throw new InvalidOperationException("The configuration must be set before the solving process can start!");
            }
            if (constraintsSource == null)
            {
                throw new InvalidOperationException("The domain constraints source must be set before the solving process can start!");
            }

            constraintsSource.LoadDomainConstraints();

            totalTime.Start();

            stopwatch.Start();
            GenerateConfigurationConstraints(configuration, constraintsSource);
            stopwatch.Stop();

            ProcessLog.AddItem(LogType.Info, String.Format("Generating configuration constraints finished ({0} ms)", stopwatch.ElapsedMilliseconds));

            // Finding all structured conmponents which include metric reasoning... 
            metricStructuredComponents = configuration.StructuringContexts.Where(x => x.Active && x.IncludedRelationFamilies.Contains(metricRelations)).ToList();
            // ... and qualitative reasoning 
            qualitativeStructuredComponents = configuration.StructuringContexts.Where(x => x.Active && x.IncludedRelationFamilies.Any(y => y != metricRelations)).ToList();

            // Generating the metric constraint networks
            if (metricStructuredComponents.Count != 0)
            {
                List<ConstraintNetwork> metricNetworks = new List<ConstraintNetwork>();

                ProcessLog.AddItem(LogType.Info, String.Format("Generating metric constraint networks out of composite components starting"));
                stopwatch.Restart();

                metricNetworks = ConstraintNetwork.GenerateMetricConstraintNetworks(metricStructuredComponents, this.tms, this.Options, this.ProcessLog);
                problemNetworks.AddRange(metricNetworks);

                stopwatch.Stop();
                ProcessLog.AddItem(LogType.Info, String.Format("{0} metric constraint networks generated out of {1} composite components ({2} ms)", metricNetworks.Count, metricStructuredComponents.Count, stopwatch.ElapsedMilliseconds));
            }

            // Generating the qualitative constraint networks
            if (qualitativeStructuredComponents.Count != 0)
            {
                ProcessLog.AddItem(LogType.Info, String.Format("Generating qualitative constraint networks for the composite components starting"));
                stopwatch.Restart();
                foreach (var strComponent in qualitativeStructuredComponents)
                {
                    // Generating a new constraint network foreach qualitative calculus in a StructuredComponent
                    foreach (var calculus in strComponent.IncludedRelationFamilies.Where(x => x != metricRelations))
                    {

                        problemNetworks.Add(ConstraintNetwork.GenerateQualitativeConstraintNetwork(strComponent, calculus, this.ProcessLog));

                    }
                }
                stopwatch.Stop();
                ProcessLog.AddItem(LogType.Info, String.Format("Generating qualitative constraint networks for the composite components finished ({0} ms)", stopwatch.ElapsedMilliseconds));
            }

            foreach (var network in problemNetworks)
            {
                ProcessLog.AddItem(LogType.Info, String.Format("Starting structuring of constraint network {0}", network.UId));
                stopwatch.Restart();

                structure.AddSolutionInformation(network.Solve(this.Options, tms));

                stopwatch.Stop();
                ProcessLog.AddItem(LogType.Info, String.Format("Structuring of constraint network {0} finished ({1} ms)", network.UId, stopwatch.ElapsedMilliseconds));
            }

            totalTime.Stop();
            ProcessLog.AddItem(LogType.Info, String.Format("Structuring configuration finished - {0} constraint networks processed ({1} ms)", problemNetworks.Count, totalTime.ElapsedMilliseconds));

            return structure;
        }

        /// <summary>
        /// Generates the internal structural configuration constraints in the structured components in the configuration
        /// </summary>
        private void GenerateConfigurationConstraints(GKOConfiguration configuration, DomainConstraintsSource constraintsSource)
        {
            List<DomainConstraint> domainConstraints = constraintsSource.DomainConstraints;

            foreach (var strComponent in configuration.StructuringContexts.Where(x => x.Active))
            {
                List<GKOComponent> activeComponents = strComponent.Components.Where(x => x.Active).ToList();

                // Construct the configuration constraint trees for all domain constraints which use relation family included in the context 
                foreach (var constraint in domainConstraints.Where(x => strComponent.IncludedRelationFamilies.Contains(x.RelationFamily)))
                {
                    strComponent.StructuralConstraints.Add(new ConfigurationConstraintTree(null, constraint, strComponent.Components.Where(x => x.Active).ToList(), this.ProcessLog));
                }
            }
        }

    }
}
