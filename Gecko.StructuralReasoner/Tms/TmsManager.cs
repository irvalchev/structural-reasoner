using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using Razr.CS3.mqm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Tms
{
    internal class TmsManager
    {
        private StructuralReasonerOptions structuralReasonerOptions;
        private ICDAStar cdaStarTms;

        #region Naming conventions

        // CONSTRAINT TYPE NAMES
        //public const string CTNameLessThan = "Less than";
        public const string CTNameGreaterThan = "Greater_than";
        // ToDo this relation is not implemented
        public const string CTNameGreaterOrEquals = "Greater_or_equals";
        public const string CTNameEquals = "Equals";
        public const string CTNameNotEquals = "Not_equals";
        public const string CTNamePlus = "Plus";
        //public const string CTNameMinus = "Minus";
        //public const string CTNameSatisfied = "Satisfied";
        public const string CTNamePostfixCalculusComposition = "Composition";

        // DOMAINS
        public const string DomainNameBool = "Boolean";
        public const string DomainNameMetric = "Metric";
        public const string DomainNameCalculiPwSetIx = "CalculPwSetIx";
        public const string DomainNameCalculusPostfix = "Relations";

        // OTHERS
        public const string TrueValue = "T";
        public const string FalseValue = "F";
        public const string WildcardValue = "*";
        private Logging.Log log;

        #endregion

        /// <summary>
        /// Gets the boolean domain consisting of:
        /// <para> 0/F - meaning False </para>
        /// <para> 1/T - meaning True </para>
        /// </summary>
        /// <returns></returns>
        public static GKODomain GenerateBoolDomain()
        {
            GKODomain domain = new GKODomain()
            {
                Id = DomainNameBool,
                Name = DomainNameBool,
                Values = new List<string>() { TrueValue, FalseValue }
            };
            return domain;
        }

        public GKODomain BoolDomain { get; set; }
        public GKOIntDomain MetricDomain { get; set; }
        // public GKOIntDomain CalculiPwSetIxDomain { get; set; }
        public List<GKODomain> CalculiDomains { get; set; }

        public Dictionary<string, IDomain> Domains { get; set; }
        public Dictionary<string, IConstraintType> ConstraintTypes { get; set; }
        public Dictionary<string, IVariable> Variables { get; set; }
        public List<IConstraint> Constraints { get; set; }

        public TmsManager(StructuralReasonerOptions structuralReasonerOptions, Log log)
        {
            this.structuralReasonerOptions = structuralReasonerOptions;
            this.log = log;

            cdaStarTms = new Workspace();

            CreateDomains();
            CreateConstraintTypes();
        }

        /// <summary>
        /// Finds a solution to the given problem
        /// </summary>
        /// <param name="decisionVariables">A list of the decision variables without the satisfiability ones</param>
        /// <param name="softDecisionVariables">The list of satisfiability decision variables</param>
        /// <param name="constraints">The constraint to be satisfied</param>
        public void Solve(List<TmsDecisionVariable> decisionVariables, List<TmsDecisionVariable> softDecisionVariables, List<TmsConstraint> constraints, SolutionData solutionInformation)
        {
            ISolutionsIterator solutionIterator;
            ISolution solution;

            this.Variables = new Dictionary<string, IVariable>();
            this.Constraints = new List<IConstraint>();

            // ToDo: See how to clear the previous context
            cdaStarTms.Clear();

            decisionVariables.ForEach(x => Variables.Add(x.Name, x.CreateIVariable(cdaStarTms, Domains)));
            softDecisionVariables.ForEach(x => Variables.Add(x.Name, x.CreateIVariable(cdaStarTms, Domains)));
            constraints.ForEach(x => Constraints.Add(x.CreateIConstraint(cdaStarTms, Variables, ConstraintTypes)));

            cdaStarTms.DefAggregateUtility(UtilityFunction);

            foreach (var softDVar in softDecisionVariables)
            {
                // Rem: The order of the variables in the boolean domain is important for the utility function
                cdaStarTms.DefAttribute(softDVar.Name);
                // True value is evaluated to 1
                cdaStarTms.set_Utility(softDVar.Name, 0, softDVar.UtilityFactor);
                // False value is evaluated to 0
                cdaStarTms.set_Utility(softDVar.Name, 1, 0);
            }

            foreach (var dVar in decisionVariables)
            {
                // To extract the value the normal (non-soft) variables should be specified as attributes for the CDA*
                cdaStarTms.DefAttribute(dVar.Name);
                for (int i = 0; i < dVar.Domain.GetValues().Count; i++)
                {
                    cdaStarTms.set_Utility(dVar.Name, i, 0);
                }
            }

            solutionIterator = cdaStarTms.Solutions();
            solution = solutionIterator.FirstSolution();

            if (solution != null)
            {
                TmsResult result = new TmsResult()
                {
                    NormalVariables = new Dictionary<string, string>(),
                    UtilityVariables = new Dictionary<string, string>()
                };

                Debug.WriteLine("");
                Debug.WriteLine("Utility: " + solution.Utility());
                Debug.WriteLine("-----------DVARS-----------");
                foreach (var var in decisionVariables)
                {
                    var value = var.Domain.GetValues()[solution.Value(var.Name)];
                    result.NormalVariables.Add(var.Name, value);
                    Debug.WriteLine(string.Format("{0} - {1}({2})", var.Name, solution.Value(var.Name), value));
                }
                Debug.WriteLine("-----------SOFT-DVARS-----------");
                foreach (var var in softDecisionVariables)
                {
                    var value = var.Domain.GetValues()[solution.Value(var.Name)];
                    result.UtilityVariables.Add(var.Name, value);
                    Debug.WriteLine(string.Format("{0} - {1}({2})", var.Name, solution.Value(var.Name), value));
                    if (value == FalseValue) {
                        solutionInformation.IsRelaxedSolution = true;
                    }
                }
                Debug.WriteLine("SOLUTION: " + solution.AsString());
                
                solutionInformation.SolutionFound = true;
                solutionInformation.TmsSolution = result;
            }
            else
            {
                solutionInformation.SolutionFound = false;
            }
        }

        /// <summary>
        /// The utility function used by the CS3 CDA*. Used as delegate by the CS3
        /// </summary>
        /// <param name="utilities"></param>
        /// <returns></returns>
        private double UtilityFunction(double[] utilities)
        {
            return utilities.Sum();
        }

        /// <summary>
        /// Loads the relevant domains information for the ATMS
        /// </summary>
        private void CreateDomains()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            this.BoolDomain = GenerateBoolDomain();
            this.MetricDomain = GenerateMetricDomain();
            //this.CalculiPwSetIxDomain = GenerateCalculiPwSetIxDomain();
            this.CalculiDomains = StructuralRelationsManager.CalculiDomains;

            // These calls will also create the domains in the CS3 TMS
            this.Domains = new Dictionary<string, IDomain>();
            this.Domains.Add(this.BoolDomain.Name, this.BoolDomain.CreateIDomain(cdaStarTms));
            this.Domains.Add(this.MetricDomain.Name, this.MetricDomain.CreateIDomain(cdaStarTms));
            //this.Domains.Add(this.CalculiPwSetIxDomain, this.CalculiPwSetIxDomain.CreateIDomain(cdaStarTms));
            this.CalculiDomains.ForEach(x => this.Domains.Add(x.Name, x.CreateIDomain(cdaStarTms)));

            stopwatch.Stop();
            log.AddItem(LogType.Info, String.Format("Generating domains for the TMS finished ({0} ms)", stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// Loads the relevant constraint types information
        /// </summary>
        private void CreateConstraintTypes()
        {
            Stopwatch stopwatch = new Stopwatch();
            int maxRelationsCount = StructuralRelationsManager.RelationFamilies.Where(x => x != StructuralRelationsManager.MetricRelationsFamily).Max(x => x.Relations.Count);

            this.ConstraintTypes = new Dictionary<string, IConstraintType>();

            stopwatch.Start();
            CreateMetricRelConstrTypes();
            stopwatch.Stop();

            log.AddItem(LogType.Info, String.Format("Generating metric constraint types for the TMS finished ({0} ms)", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            CreateIndividualCTForDomain(this.BoolDomain, false);
            //CreateIndividualCTForDomain(this.CalculiPwSetIxDomain);
            CreateIndividualCTForDomain(this.MetricDomain, false);
            StructuralRelationsManager.CalculiDomains.ForEach(x => CreateIndividualCTForDomain(x, true));
            for (int i = 0; i < Math.Max(maxRelationsCount, StructuralReasonerOptions.DomainConstraintMaximumBranches); i++)
            {
                // Exactly-one-of constraint is used for the calculi and domain constraint combinations
                CreateExactlyOneCT(i + 1);
                // And and Or constraints are used for domain constraint combinations
                if (i < StructuralReasonerOptions.DomainConstraintMaximumBranches)
                {
                    CreateAndCT(i + 1);
                    CreateOrCT(i + 1);
                }
            }
            stopwatch.Stop();

            log.AddItem(LogType.Info, String.Format("Generating 'domains single value', 'and', 'or' and 'exactly one holds' constraint types for the TMS finished ({0} ms)", stopwatch.ElapsedMilliseconds));

            stopwatch.Restart();
            StructuralRelationsManager.GenerateCompositionConstraintTypes().ForEach(x => this.ConstraintTypes.Add(x.Name, x.CreateIConstraintType(cdaStarTms, this.Domains)));
            stopwatch.Stop();

            log.AddItem(LogType.Info, String.Format("Generating qualitative calculi composition constraint types for the TMS finished ({0} ms)", stopwatch.ElapsedMilliseconds));

            //stopwatch.Restart();
            //foreach (var calculus in StructuralRelationsManager.RelationFamilies.Where(x => x != StructuralRelationsManager.MetricRelationsFamily ))
            //{
            //    CreateCTForPowerset(calculus);
            //}

            //stopwatch.Stop();
            //log.AddItem(LogType.Info, String.Format("Generating powerset qualitative calculi constraint types for the TMS finished ({0} ms)", stopwatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// Generates the metric domain for the TMS
        /// </summary>
        /// <returns></returns>
        private GKOIntDomain GenerateMetricDomain()
        {
            GKOIntDomain domain = new GKOIntDomain()
            {
                Id = DomainNameMetric,
                Name = DomainNameMetric,
                StepWidth = 1,
                MinValue = StructuralRelationsManager.MetricDomain.MinValue,
                MaxValue = structuralReasonerOptions.MaxMetricValue
            };

            return domain;
        }

        ///// <summary>
        ///// Generates the domain used as index for the powerset of the enabled calculi
        ///// </summary>
        ///// <returns></returns>
        //private GKOIntDomain GenerateCalculiPwSetIxDomain()
        //{
        //    GKOIntDomain domain = new GKOIntDomain()
        //    {
        //        Id = DomainNameCalculiPwSetIx,
        //        Name = DomainNameCalculiPwSetIx,
        //        StepWidth = 1,
        //        MinValue = 0,
        //        MaxValue = (int)Math.Pow(2, StructuralRelationsManager.RelationFamilies.Max(x => x.Relations.Count))
        //    };

        //    return domain;
        //}

        #region Constraint Types

        /// <summary>
        /// Creates constraint types in the CS3, each new CT contains only one value from a domain.
        /// NOTE: These constraint types are used to allow only one value to be selected for a variable.
        /// </summary>
        /// <param name="domain">The domain to create constraint types for</param>
        /// <param name="addSoft">Specifies whether to add a softness variable at the end</param>
        private void CreateIndividualCTForDomain(GKODomainAbstract domain, bool addSoft)
        {
            List<GKOConstraintType> constraintTypes = new List<GKOConstraintType>();
            List<string> domainValues = new List<string>();

            if (domain is GKOIntDomain)
            {
                GKOIntDomain domainTemp = domain as GKOIntDomain;

                for (int i = domainTemp.MinValue; i < domainTemp.MaxValue + 1; i += domainTemp.StepWidth)
                {
                    domainValues.Add(i.ToString());
                }
            }
            else if (domain is GKODomain)
            {
                GKODomain domainTemp = domain as GKODomain;

                foreach (var value in domainTemp.Values)
                {
                    domainValues.Add(value);
                }
            }

            foreach (var value in domainValues)
            {
                GKOConstraintType ct = new GKOConstraintType()
                {
                    Id = domain.GetIndividualValueCTName(value),
                    Name = domain.GetIndividualValueCTName(value),
                    Signature = new List<GKODomainAbstract>() { domain },
                    Tuples = new List<List<string>>() { new List<string>() { value } }
                };

                if (addSoft)
                {
                    ct.Signature.Add(this.BoolDomain);
                    ct.Tuples[0].Add(TrueValue);
                    foreach (var excludedValue in domainValues)
                    {
                        ct.Tuples.Add(new List<string>() { excludedValue, FalseValue });
                    }
                }

                constraintTypes.Add(ct);
            }

            constraintTypes.ForEach(x => this.ConstraintTypes.Add(x.Name, x.CreateIConstraintType(cdaStarTms, this.Domains)));
        }

        /// <summary>
        /// Creates constraint type in the CS3, which allow only one out of several bool values to be selected at a time.
        /// Note: The procedure does not generate all tuples when the variable count is more than the allowed branches count for a domain constraint combination. When the variable count is greater than this value, the constraint is still usable for JEPD relations constraints
        /// </summary>
        /// <param name="variableCount">The number of variables of the CT</param>
        private void CreateExactlyOneCT(int variableCount)
        {
            GKOConstraintType ct = new GKOConstraintType();
            ct.Id = variableCount.GetExactlyOneCTName();
            ct.Name = variableCount.GetExactlyOneCTName();
            ct.Signature = new List<GKODomainAbstract>();
            ct.Tuples = new List<List<string>>();

            // Creating the signature
            // There is one extra variable for the satisfaction of the constraint
            for (int i = 0; i < variableCount + 1; i++)
            {
                ct.Signature.Add(this.BoolDomain);
            }

            for (int tupleIx = 0; tupleIx < Math.Pow(2, variableCount); tupleIx++)
            {
                List<string> tuple = new List<string>();
                bool relSatisfied = false;

                for (int i = 0; i < variableCount; i++)
                {
                    // If the i-th bit is set then set True, otherwise False
                    string var = (tupleIx & (1 << i)) == 0 ? TmsManager.FalseValue : TmsManager.TrueValue;
                    tuple.Add(var);
                }

                // The relation is satisfied if only one of the elements in the tuple is True
                relSatisfied = tuple.Where(x => x == TmsManager.TrueValue).Count() == 1;
                tuple.Add(relSatisfied ? TmsManager.TrueValue : TmsManager.FalseValue);

                ct.Tuples.Add(tuple);
            }

            // Remove some of the variables to increase the performance, if they are not used for Domain constraint combinations
            if (variableCount > StructuralReasonerOptions.DomainConstraintMaximumBranches)
            {
                List<string> notSatisfiedTuple = new List<string>();
                // Remove all tuples which do not match the condition
                ct.Tuples = ct.Tuples.Where(x => x.Last() == TmsManager.TrueValue).ToList();

                // Generating and adding the only not-satisfied tuple = (N+1)*False
                for (int i = 0; i < variableCount + 1; i++)
                {
                    notSatisfiedTuple.Add(TmsManager.FalseValue);
                }
                ct.Tuples.Add(notSatisfiedTuple);
            }

            this.ConstraintTypes.Add(ct.Name, ct.CreateIConstraintType(cdaStarTms, this.Domains));
        }

        /// <summary>
        /// Creates constraint type in the CS3, which requires that all values are set.
        /// </summary>
        /// <param name="variableCount">The number of variables of the CT</param>
        private void CreateAndCT(int variableCount)
        {
            GKOConstraintType ct = new GKOConstraintType();
            ct.Id = variableCount.GetAndCTName();
            ct.Name = variableCount.GetAndCTName();
            ct.Signature = new List<GKODomainAbstract>();
            ct.Tuples = new List<List<string>>();

            if (variableCount > StructuralReasonerOptions.DomainConstraintMaximumBranches)
            {
                throw new InvalidOperationException(string.Format("Cannot create AND TMS Constraint Type with more elements ({0}) than the allowed number of branches in a Domain constraint ({1})!", variableCount, StructuralReasonerOptions.DomainConstraintMaximumBranches));
            }

            // Creating the signature
            // There is one extra variable for the satisfaction of the constraint
            for (int i = 0; i < variableCount + 1; i++)
            {
                ct.Signature.Add(this.BoolDomain);
            }

            for (int tupleIx = 0; tupleIx < Math.Pow(2, variableCount); tupleIx++)
            {
                List<string> tuple = new List<string>();
                bool relSatisfied = false;

                for (int i = 0; i < variableCount; i++)
                {
                    // If the i-th bit is set then set True, otherwise False
                    string var = (tupleIx & (1 << i)) == 0 ? TmsManager.FalseValue : TmsManager.TrueValue;
                    tuple.Add(var);
                }

                // The relation is satisfied if all of the elements in the tuple are True
                relSatisfied = tuple.All(x => x == TmsManager.TrueValue);
                tuple.Add(relSatisfied ? TmsManager.TrueValue : TmsManager.FalseValue);

                ct.Tuples.Add(tuple);
            }

            this.ConstraintTypes.Add(ct.Name, ct.CreateIConstraintType(cdaStarTms, this.Domains));
        }

        /// <summary>
        /// Creates constraint type in the CS3, which requires that at least one element is set to True.
        /// </summary>
        /// <param name="variableCount">The number of variables of the CT</param>
        private void CreateOrCT(int variableCount)
        {
            GKOConstraintType ct = new GKOConstraintType();
            ct.Id = variableCount.GetOrCTName();
            ct.Name = variableCount.GetOrCTName();
            ct.Signature = new List<GKODomainAbstract>();
            ct.Tuples = new List<List<string>>();

            if (variableCount > StructuralReasonerOptions.DomainConstraintMaximumBranches)
            {
                throw new InvalidOperationException(string.Format("Cannot create OR TMS Constraint Type with more elements ({0}) than the allowed number of branches in a Domain constraint ({1})!", variableCount, StructuralReasonerOptions.DomainConstraintMaximumBranches));
            }

            // Creating the signature
            // There is one extra variable for the satisfaction of the constraint
            for (int i = 0; i < variableCount + 1; i++)
            {
                ct.Signature.Add(this.BoolDomain);
            }

            for (int tupleIx = 0; tupleIx < Math.Pow(2, variableCount); tupleIx++)
            {
                List<string> tuple = new List<string>();
                bool relSatisfied = false;

                for (int i = 0; i < variableCount; i++)
                {
                    // If the i-th bit is set then set True, otherwise False
                    string var = (tupleIx & (1 << i)) == 0 ? TmsManager.FalseValue : TmsManager.TrueValue;
                    tuple.Add(var);
                }

                // The relation is satisfied if at least one of the elements in the tuple is True
                relSatisfied = tuple.Any(x => x == TmsManager.TrueValue);
                tuple.Add(relSatisfied ? TmsManager.TrueValue : TmsManager.FalseValue);

                ct.Tuples.Add(tuple);
            }

            this.ConstraintTypes.Add(ct.Name, ct.CreateIConstraintType(cdaStarTms, this.Domains));
        }

        ///// <summary>
        ///// Create all constraint types corresponding to the subsets of the relations in a relation family
        ///// </summary>
        ///// <param name="calculus">The relation family</param>
        //private void CreateCTForPowerset(RelationFamily calculus)
        //{
        //    //GKOConstraintType constraintType;
        //    //GKODomainAbstract relationsDomain;
        //    //GKODomainAbstract powerSetIxDomain;
        //    //bool enableSoftConstraints = this.structuralReasonerOptions.SoftConstraintsEnabled;

        //    //if (calculus.Name == RelationFamilyNames.MetricRelationsName)
        //    //{
        //    //    throw new NotSupportedException("The metric relations do not support this operation.");
        //    //}

        //    //relationsDomain = StructuralRelationsManager.GetDomain(calculus.GetTmsRelationsDomainName());
        //    //powerSetIxDomain = this.CalculiPwSetIxDomain;

        //    //constraintType = new GKOConstraintType();
        //    //constraintType.Id = calculus.GetPwSetCTName();
        //    //constraintType.Name = calculus.GetPwSetCTName();
        //    //constraintType.Signature = new List<GKODomainAbstract>() { powerSetIxDomain, relationsDomain };
        //    //constraintType.Tuples = new List<List<string>>();
        //    //if (enableSoftConstraints)
        //    //{
        //    //    constraintType.Signature.Add(this.BoolDomain);
        //    //}

        //    //// The name of the constraint is labeled by the integer representation of the included relations
        //    //// e.g. set 17 = 10001, i.e. relations 0 and 3 are included in the constraint with label 17
        //    //for (int setIx = 1; setIx < Math.Pow(2, calculus.Relations.Count); setIx++)
        //    //{
        //    //    List<BinaryRelation> includedRelations = new List<BinaryRelation>();

        //    //    for (int i = 0; i < calculus.Relations.Count; i++)
        //    //    {
        //    //        if ((setIx & (1 << i)) != 0)
        //    //        {
        //    //            //n-th bit is set, so we include the n-th relation
        //    //            includedRelations.Add(calculus.Relations[i]);
        //    //        }
        //    //    }

        //    //    // When soft constraints are enabled the constraint types should be adjusted accordingly
        //    //    if (enableSoftConstraints)
        //    //    {
        //    //        List<BinaryRelation> excludedReltions = calculus.Relations.Except(includedRelations).ToList();

        //    //        foreach (var rel in includedRelations)
        //    //        {
        //    //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name, TmsManager.TrueValue });
        //    //        }

        //    //        // Adding the non-satisfied tuples: *, 0
        //    //        // ct.Tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.FalseValue });
        //    //        foreach (var rel in excludedReltions)
        //    //        {
        //    //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name, TmsManager.FalseValue });
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        foreach (var rel in includedRelations)
        //    //        {
        //    //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name });
        //    //        }
        //    //    }
        //    //}

        //    //this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));
        //}

        /// <summary>
        /// Creates the metric constraint types
        /// </summary>
        /// <returns></returns>
        private void CreateMetricRelConstrTypes()
        {
            GKOIntDomain metricDomain = this.MetricDomain;
            GKODomainAbstract satisfiedDomain = this.BoolDomain;
            GKOConstraintType constraintType;
            List<List<string>> tuples;
            // Legacy variable. Now it should be always true
            bool enableSoftConstraints = true;

            // Creating the "Greater than" (a>b) constraint type
            constraintType = new GKOConstraintType()
            {
                Id = CTNameGreaterThan,
                Name = CTNameGreaterThan
            };
            constraintType.Signature = new List<GKODomainAbstract>() { metricDomain, metricDomain };
            if (enableSoftConstraints)
            {
                constraintType.Signature.Add(satisfiedDomain);
            }

            tuples = new List<List<string>>();
            for (int a = metricDomain.MinValue; a < metricDomain.MaxValue + 1; a += metricDomain.StepWidth)
            {
                if (enableSoftConstraints)
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        // in some cases the non-satisfied tuples are added: *, *, 0
                        // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a > b ? TrueValue : FalseValue });
                    }
                }
                else
                {
                    for (int b = metricDomain.MinValue; b < a; b += metricDomain.StepWidth)
                    {
                        tuples.Add(new List<string>() { a.ToString(), b.ToString() });
                    }
                }
            }

            constraintType.Tuples = tuples;
            // Adds the constraint type to the CS3
            this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));

            // Creating the "Greater or equals" (a>=b) constraint type
            constraintType = new GKOConstraintType()
            {
                Id = CTNameGreaterOrEquals,
                Name = CTNameGreaterOrEquals
            };
            constraintType.Signature = new List<GKODomainAbstract>() { metricDomain, metricDomain };
            if (enableSoftConstraints)
            {
                constraintType.Signature.Add(satisfiedDomain);
            }

            tuples = new List<List<string>>();
            for (int a = metricDomain.MinValue; a < metricDomain.MaxValue + 1; a += metricDomain.StepWidth)
            {
                if (enableSoftConstraints)
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        // in some cases the non-satisfied tuples are added: *, *, 0
                        // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a >= b ? TrueValue : FalseValue });
                    }
                }
                else
                {
                    for (int b = metricDomain.MinValue; b <= a; b += metricDomain.StepWidth)
                    {
                        tuples.Add(new List<string>() { a.ToString(), b.ToString() });
                    }
                }
            }

            constraintType.Tuples = tuples;
            // Adds the constraint type to the CS3
            this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));

            // Creating the "Equals" (a=b) constraint type
            constraintType = new GKOConstraintType()
            {
                Id = CTNameEquals,
                Name = CTNameEquals
            };
            constraintType.Signature = new List<GKODomainAbstract>() { metricDomain, metricDomain };
            if (enableSoftConstraints)
            {
                constraintType.Signature.Add(satisfiedDomain);
            }

            tuples = new List<List<string>>();
            for (int a = metricDomain.MinValue; a < metricDomain.MaxValue + 1; a += metricDomain.StepWidth)
            {
                if (enableSoftConstraints)
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        // in some cases the non-satisfied tuples are added: *, *, 0
                        // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a == b ? TmsManager.TrueValue : TmsManager.FalseValue });
                    }
                }
                else
                {
                    tuples.Add(new List<string>() { a.ToString(), a.ToString() });
                }
            }

            constraintType.Tuples = tuples;
            // Adds the constraint type to the CS3
            this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));

            // Creating the "Not equals" (a!=b) constraint type
            constraintType = new GKOConstraintType()
            {
                Id = CTNameNotEquals,
                Name = CTNameNotEquals
            };
            constraintType.Signature = new List<GKODomainAbstract>() { metricDomain, metricDomain };
            if (enableSoftConstraints)
            {
                constraintType.Signature.Add(satisfiedDomain);
            }

            tuples = new List<List<string>>();
            for (int a = metricDomain.MinValue; a < metricDomain.MaxValue + 1; a += metricDomain.StepWidth)
            {
                if (enableSoftConstraints)
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        // in some cases the non-satisfied tuples are added: *, *, 0
                        // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a != b ? TrueValue : FalseValue });
                    }
                }
                else
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        if (a != b)
                        {
                            tuples.Add(new List<string>() { a.ToString(), b.ToString() });
                        }
                    }
                }
            }

            constraintType.Tuples = tuples;
            // Adds the constraint type to the CS3
            this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));

            // Creating the "Plus" (a + b = c) constraint type
            constraintType = new GKOConstraintType()
            {
                Id = CTNamePlus,
                Name = CTNamePlus
            };
            constraintType.Signature = new List<GKODomainAbstract>() { metricDomain, metricDomain, metricDomain };
            if (enableSoftConstraints)
            {
                constraintType.Signature.Add(satisfiedDomain);
            }

            tuples = new List<List<string>>();
            for (int a = metricDomain.MinValue; a < metricDomain.MaxValue + 1; a += metricDomain.StepWidth)
            {
                if (enableSoftConstraints)
                {
                    for (int b = metricDomain.MinValue; b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        for (int c = metricDomain.MinValue; c < metricDomain.MaxValue + 1; c += metricDomain.StepWidth)
                        {
                            // in some cases the non-satisfied tuples are added: *, *, *, 0
                            // tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.WildcardValue, TmsManager.FalseValue });
                            tuples.Add(new List<string>() { a.ToString(), b.ToString(), c.ToString(), (a + b) == c ? TrueValue : FalseValue });
                        }
                    }
                }
                else
                {
                    for (int b = metricDomain.MinValue; a + b < metricDomain.MaxValue + 1; b += metricDomain.StepWidth)
                    {
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), (a + b).ToString() });
                    }
                }
            }

            constraintType.Tuples = tuples;
            // Adds the constraint type to the CS3
            this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));
        }

        #endregion

    }
}
