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
        public const string CTNameSatisfied = "Plus";
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
            GKODomain domain = new GKODomain();

            domain.Id = TmsManager.DomainNameBool;
            domain.Name = TmsManager.DomainNameBool;
            domain.Values = new List<string>() { TmsManager.TrueValue, TmsManager.FalseValue };

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

            //new TestTms2().Solve();
        }

        #region Tests

        private void TestTms()
        {
            ICDAStar cdaStarTms = new Workspace();
            ISolutionsIterator solutionIterator;
            ISolution solution;

            IDomain boolDomain = cdaStarTms.DefDomain("Boolean", new string[] { "T", "F" });
            IVariable testVar = cdaStarTms.DefVariable("test", boolDomain);
            IVariable softVar = cdaStarTms.DefVariable("soft", boolDomain);

            // There is a constraint type that allows only value F to be chosen 
            IConstraintType ct = cdaStarTms.DefConstraintType(
                "Only_False_Allowed",
                new IDomain[1] { boolDomain },
                new string[][] { new string[] { "F" } }
                );
            // For testVar only value F should be allowed
            IConstraint constraint = cdaStarTms.DefConstraint(ct, new IVariable[] { testVar });

            // The function simply returns the sum of utilities
            cdaStarTms.DefAggregateUtility(UtilityFunction);

            // Adding the two variables as attributes
            cdaStarTms.DefAttribute(softVar.Name);
            cdaStarTms.set_Utility(softVar.Name, 0, 0);
            cdaStarTms.set_Utility(softVar.Name, 1, 1);
            // testVar is not relevant to the utility function
            cdaStarTms.DefAttribute(testVar.Name);
            cdaStarTms.set_Utility(testVar.Name, 0, 0);
            cdaStarTms.set_Utility(testVar.Name, 1, 0);

            solutionIterator = cdaStarTms.Solutions();
            solution = solutionIterator.FirstSolution();

            while (solution != null)
            {
                Debug.WriteLine("Utility: " + solution.Utility());
                Debug.WriteLine("Test var: " + solution.Value(testVar.Name));
                Debug.WriteLine("Soft var: " + solution.Value(softVar.Name));
                Debug.WriteLine("AsString: " + solution.AsString());
                Debug.WriteLine("");

                solution = solutionIterator.NextSolution();
            }
        }

        public class TestTms2
        {
            ICDAStar cdaStarTms;

            // Domains
            IDomain boolDomain;
            IDomain metricDomain;
            String[] boolDomainValues;
            String[] metricDomainValues;

            // Variables
            IVariable positionA;
            IVariable positionB;
            IVariable metricVal1;

            // Satisfiability variables used for utility
            IVariable ANotEqBSatisfied;
            IVariable ALessOrEquals1Satisfied;
            IVariable BLessOrEquals1Satisfied;

            // Constraint types
            IConstraintType ctOnly1Allowed;
            IConstraintType ctOnlyTrueAllowed;
            IConstraintType ctNotEquals;
            IConstraintType ctLE;

            public TestTms2()
            {
                cdaStarTms = new Workspace();

                CreateDomains();
                CreateVariables();
                CreateConstraintTypes();
                CreateConstraints();
                CreateUtilityAttributes();

                // This is causing the problem!
                //CreateSolutionAttributes();
            }

            public void Solve()
            {
                ISolutionsIterator solutionIterator;
                ISolution solution;

                //  Setting the utility function
                cdaStarTms.DefAggregateUtility(UtilityFunction);

                solutionIterator = cdaStarTms.Solutions();
                solution = solutionIterator.FirstSolution();

                while (solution != null)
                {
                    Debug.WriteLine("Utility: " + solution.Utility());
                    Debug.WriteLine("A NotEq B Satisfied: " + boolDomainValues[solution.Value(ANotEqBSatisfied.Name)]);
                    Debug.WriteLine("A LessOrEquals 1 Satisfied: " + boolDomainValues[solution.Value(ALessOrEquals1Satisfied.Name)]);
                    Debug.WriteLine("B LessOrEquals 1 Satisfied: " + boolDomainValues[solution.Value(BLessOrEquals1Satisfied.Name)]);
                    Debug.WriteLine("Position A: " + metricDomainValues[solution.Value(positionA.Name)]);
                    Debug.WriteLine("Position B: " + metricDomainValues[solution.Value(positionB.Name)]);
                    Debug.WriteLine("Metric Val 1: " + metricDomainValues[solution.Value(metricVal1.Name)]);
                    Debug.WriteLine("AsString: " + solution.AsString());
                    Debug.WriteLine("");

                    solution = solutionIterator.NextSolution();
                }
            }

            private double UtilityFunction(double[] utilities)
            {
                return utilities.Sum();
            }

            private void CreateDomains()
            {
                // These arrays are just for convenience, for finding the values in the domains later
                boolDomainValues = new string[] { "T", "F" };
                metricDomainValues = new string[] { "1", "2", "3" };

                // Creating the actual domains
                boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues);
                metricDomain = cdaStarTms.DefDomain("Metric", metricDomainValues);
            }

            private void CreateVariables()
            {
                // Creating the metric variables
                positionA = cdaStarTms.DefVariable("position_A", metricDomain);
                positionB = cdaStarTms.DefVariable("position_B", metricDomain);
                metricVal1 = cdaStarTms.DefVariable("metric_val-1", metricDomain);

                // Creating satisfiability variables used for utility
                // They are also used to enable soft constraints
                ANotEqBSatisfied = cdaStarTms.DefVariable("position_A-NotEquals-position_B", boolDomain);
                ALessOrEquals1Satisfied = cdaStarTms.DefVariable("position_A-LessOrEquals-1", boolDomain);
                BLessOrEquals1Satisfied = cdaStarTms.DefVariable("position_B-LessOrEquals-1", boolDomain);
            }

            private void CreateConstraintTypes()
            {
                IDomain[] ctSignature = null;
                string[][] ctTuples = null;
                int tupleCounter = 0;

                // Creating a constraint type allowing a value to be set only to 1 
                ctOnly1Allowed = cdaStarTms.DefConstraintType(
                    "Only_1_Allowed",
                    new IDomain[1] { metricDomain },
                    new string[][] { new string[] { "1" } }
                    );


                // Creating a constraint type allowing a value to be set only to True 
                ctOnlyTrueAllowed = cdaStarTms.DefConstraintType(
                    "Only_True_Allowed",
                    new IDomain[1] { boolDomain },
                    new string[][] { new string[] { "T" } }
                    );


                // Creating the not equals constraint type, i.e. A != B 
                // If a tuple <A, B> satisfies the condition then the third part of the relation is True, otherwise False - this allows using soft constraints and is used for the utility function
                ctSignature = new IDomain[3] { metricDomain, metricDomain, boolDomain };
                ctTuples = new string[3 * 3][];
                tupleCounter = 0;

                for (int i = 1; i < 4; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        string satisfied = i != j ? "T" : "F";

                        ctTuples[tupleCounter] = new string[] { i.ToString(), j.ToString(), satisfied };
                        tupleCounter++;
                    }
                }

                ctNotEquals = cdaStarTms.DefConstraintType("Not_Equals", ctSignature, ctTuples);


                // Creating the less or equals constraint type, i.e. A <= B
                // If a tuple <A, B> satisfies the condition then the third part of the relation is True, otherwise False - this allows using soft constraints and is used for the utility function
                ctSignature = new IDomain[3] { metricDomain, metricDomain, boolDomain };
                ctTuples = new string[3 * 3][];
                tupleCounter = 0;

                for (int i = 1; i < 4; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        string satisfied = i <= j ? "T" : "F";

                        ctTuples[tupleCounter] = new string[] { i.ToString(), j.ToString(), satisfied };
                        tupleCounter++;
                    }
                }

                ctLE = cdaStarTms.DefConstraintType("Less_Or_Equals", ctSignature, ctTuples);
            }

            private void CreateConstraints()
            {
                // A should not have the same position as B
                cdaStarTms.DefConstraint(ctNotEquals, new IVariable[] { positionA, positionB, ANotEqBSatisfied });

                // The positions of A and B should be LE 1
                cdaStarTms.DefConstraint(ctLE, new IVariable[] { positionA, metricVal1, ALessOrEquals1Satisfied });
                cdaStarTms.DefConstraint(ctLE, new IVariable[] { positionB, metricVal1, BLessOrEquals1Satisfied });

                // The metric variable should have value 1
                cdaStarTms.DefConstraint(ctOnly1Allowed, new IVariable[] { metricVal1 });

                // Specifying that the constraints: position A <= 1
                //                                  position B <= 1
                // cannot be violated, i.e. their satisfaction variable should be always set to True
                // This is achieved by using the ctOnlyTrueAllowed constraint for these variables
                cdaStarTms.DefConstraint(ctOnlyTrueAllowed, new IVariable[] { ALessOrEquals1Satisfied });
                cdaStarTms.DefConstraint(ctOnlyTrueAllowed, new IVariable[] { BLessOrEquals1Satisfied });
            }

            /// <summary>
            /// Each constraint in the problem has a satisfaction variable with boolean domain.
            /// If a constraint is satisfied, its satisfaction variable is True, otherwise - False.
            /// If the value is True (i.e. the constraint is satisfied) it has utility - 1, otherwise - 0
            /// </summary>
            private void CreateUtilityAttributes()
            {
                cdaStarTms.DefAttribute(ANotEqBSatisfied.Name);
                cdaStarTms.set_Utility(ANotEqBSatisfied.Name, 0, 1);
                cdaStarTms.set_Utility(ANotEqBSatisfied.Name, 1, 0);

                cdaStarTms.DefAttribute(ALessOrEquals1Satisfied.Name);
                cdaStarTms.set_Utility(ALessOrEquals1Satisfied.Name, 0, 1);
                cdaStarTms.set_Utility(ALessOrEquals1Satisfied.Name, 1, 0);

                cdaStarTms.DefAttribute(BLessOrEquals1Satisfied.Name);
                cdaStarTms.set_Utility(BLessOrEquals1Satisfied.Name, 0, 1);
                cdaStarTms.set_Utility(BLessOrEquals1Satisfied.Name, 1, 0);
            }

            /// <summary>
            /// Setting the non-utility variables as attributes
            /// These variables do not take part in the utility function,
            /// but there assignment is the actual solution which is sought
            /// </summary>
            private void CreateSolutionAttributes()
            {
                cdaStarTms.DefAttribute(positionA.Name, 0, 0, 0);
                cdaStarTms.DefAttribute(positionB.Name, 0, 0, 0);
                cdaStarTms.DefAttribute(metricVal1.Name, 0, 0, 0);
            }
        }

        #endregion

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
            //cdaStarTms.Clear();

            // ToDo: commented for testing purposes. Fix when CS3 works
            //decisionVariables.ForEach(x => Variables.Add(x.Name, x.CreateIVariable(cdaStarTms, Domains)));
            //softDecisionVariables.ForEach(x => Variables.Add(x.Name, x.CreateIVariable(cdaStarTms, Domains)));
            //constraints.ForEach(x => Constraints.Add(x.CreateIConstraint(cdaStarTms, Variables, ConstraintTypes)));

            //cdaStarTms.DefAggregateUtility(UtilityFunction);

            //foreach (var softDVar in softDecisionVariables)
            //{
            //    // Rem: The order of the variables in the boolean domain is important for the utility function
            //    cdaStarTms.DefAttribute(softDVar.Name);
            //    // True value is evaluated to 1
            //    cdaStarTms.set_Utility(softDVar.Name, 0, softDVar.UtilityFactor);
            //    // False value is evaluated to 0
            //    cdaStarTms.set_Utility(softDVar.Name, 1, 0);
            //}
            
            //foreach (var dVar in decisionVariables)
            //{
            //    // To extract the value the normal variables should be specified as attributes for the CDA*
            //    cdaStarTms.DefAttribute(dVar.Name);
            //    for (int i = 0; i < dVar.Domain.GetValues().Count; i++)
            //    {
            //        cdaStarTms.set_Utility(dVar.Name, i, 0);
            //    }
            //}

            //solutionIterator = cdaStarTms.Solutions();
            //solution = solutionIterator.FirstSolution();

            //if (solution != null)
            //{
            //    // ToDo: Assign solution information
            //    solutionInformation.SolutionFound = true;

            //    Debug.WriteLine("");
            //    Debug.WriteLine(solution.AsString());

            //    Debug.WriteLine("");
            //    Debug.WriteLine("Utility: " + solution.Utility());

            //    Debug.WriteLine("");
            //    Debug.WriteLine("-----------DVARS-----------");
            //    foreach (var var in decisionVariables)
            //    {
            //        Debug.WriteLine(string.Format("{0} - {1}", var.Name, solution.Value(var.Name)));
            //    }

            //    Debug.WriteLine("");
            //    Debug.WriteLine("-----------SOFT-DVARS-----------");
            //    foreach (var var in softDecisionVariables)
            //    {
            //        Debug.WriteLine(string.Format("{0} - {1}", var.Name, solution.Value(var.Name)));
            //    }
            //}
            //else
            //{
            //    solutionInformation.SolutionFound = false;
            //}

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

            this.BoolDomain = TmsManager.GenerateBoolDomain();
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
            GKOIntDomain domain = new GKOIntDomain();

            domain.Id = TmsManager.DomainNameMetric;
            domain.Name = TmsManager.DomainNameMetric;
            domain.StepWidth = 1;
            domain.MinValue = StructuralRelationsManager.MetricDomain.MinValue;
            domain.MaxValue = structuralReasonerOptions.MaxMetricValue;

            return domain;
        }

        /// <summary>
        /// Generates the domain used as index for the powerset of the enabled calculi
        /// </summary>
        /// <returns></returns>
        private GKOIntDomain GenerateCalculiPwSetIxDomain()
        {
            GKOIntDomain domain = new GKOIntDomain();

            domain.Id = TmsManager.DomainNameCalculiPwSetIx;
            domain.Name = TmsManager.DomainNameCalculiPwSetIx;
            domain.StepWidth = 1;
            domain.MinValue = 0;
            domain.MaxValue = (int)Math.Pow(2, StructuralRelationsManager.RelationFamilies.Max(x => x.Relations.Count));

            return domain;
        }

        #region Constraint Types

        /// <summary>
        /// Creates constraint types in the CS3, each new CT contains only one value from a domain
        /// </summary>
        /// <param name="domain">The domain to create constraint types for</param>
        /// <param name="addSoft">Specifies whether to add a softness varible at the end</param>
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
                GKOConstraintType ct = new GKOConstraintType();
                ct.Id = domain.GetIndividualValueCTName(value);
                ct.Name = domain.GetIndividualValueCTName(value);
                ct.Signature = new List<GKODomainAbstract>() { domain };
                ct.Tuples = new List<List<string>>() { new List<string>() { value } };

                if (addSoft)
                {
                    ct.Signature.Add(this.BoolDomain);
                    ct.Tuples[0].Add(TmsManager.TrueValue);
                    foreach (var excludedValue in domainValues)
                    {
                        ct.Tuples.Add(new List<string>() { excludedValue, TmsManager.FalseValue });
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

        /// <summary>
        /// Create all constraint types corresponding to the subsets of the relations in a relation family
        /// </summary>
        /// <param name="calculus">The relation family</param>
        private void CreateCTForPowerset(RelationFamily calculus)
        {
            //GKOConstraintType constraintType;
            //GKODomainAbstract relationsDomain;
            //GKODomainAbstract powerSetIxDomain;
            //bool enableSoftConstraints = this.structuralReasonerOptions.SoftConstraintsEnabled;

            //if (calculus.Name == RelationFamilyNames.MetricRelationsName)
            //{
            //    throw new NotSupportedException("The metric relations do not support this operation.");
            //}

            //relationsDomain = StructuralRelationsManager.GetDomain(calculus.GetTmsRelationsDomainName());
            //powerSetIxDomain = this.CalculiPwSetIxDomain;

            //constraintType = new GKOConstraintType();
            //constraintType.Id = calculus.GetPwSetCTName();
            //constraintType.Name = calculus.GetPwSetCTName();
            //constraintType.Signature = new List<GKODomainAbstract>() { powerSetIxDomain, relationsDomain };
            //constraintType.Tuples = new List<List<string>>();
            //if (enableSoftConstraints)
            //{
            //    constraintType.Signature.Add(this.BoolDomain);
            //}

            //// The name of the constraint is labeled by the integer representation of the included relations
            //// e.g. set 17 = 10001, i.e. relations 0 and 3 are included in the constraint with label 17
            //for (int setIx = 1; setIx < Math.Pow(2, calculus.Relations.Count); setIx++)
            //{
            //    List<BinaryRelation> includedRelations = new List<BinaryRelation>();

            //    for (int i = 0; i < calculus.Relations.Count; i++)
            //    {
            //        if ((setIx & (1 << i)) != 0)
            //        {
            //            //n-th bit is set, so we include the n-th relation
            //            includedRelations.Add(calculus.Relations[i]);
            //        }
            //    }

            //    // When soft constraints are enabled the constraint types should be adjusted accordingly
            //    if (enableSoftConstraints)
            //    {
            //        List<BinaryRelation> excludedReltions = calculus.Relations.Except(includedRelations).ToList();

            //        foreach (var rel in includedRelations)
            //        {
            //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name, TmsManager.TrueValue });
            //        }

            //        // Adding the non-satisfied tuples: *, 0
            //        // ct.Tuples.Add(new List<string>() { TmsManager.WildcardValue, TmsManager.FalseValue });
            //        foreach (var rel in excludedReltions)
            //        {
            //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name, TmsManager.FalseValue });
            //        }
            //    }
            //    else
            //    {
            //        foreach (var rel in includedRelations)
            //        {
            //            constraintType.Tuples.Add(new List<string>() { setIx.ToString(), rel.Name });
            //        }
            //    }
            //}

            //this.ConstraintTypes.Add(constraintType.Name, constraintType.CreateIConstraintType(cdaStarTms, this.Domains));
        }

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
                Id = TmsManager.CTNameGreaterThan,
                Name = TmsManager.CTNameGreaterThan
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
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a > b ? TmsManager.TrueValue : TmsManager.FalseValue });
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
                Id = TmsManager.CTNameGreaterOrEquals,
                Name = TmsManager.CTNameGreaterOrEquals
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
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a >= b ? TmsManager.TrueValue : TmsManager.FalseValue });
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
                Id = TmsManager.CTNameEquals,
                Name = TmsManager.CTNameEquals
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
                Id = TmsManager.CTNameNotEquals,
                Name = TmsManager.CTNameNotEquals
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
                        tuples.Add(new List<string>() { a.ToString(), b.ToString(), a != b ? TmsManager.TrueValue : TmsManager.FalseValue });
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
                Id = TmsManager.CTNamePlus,
                Name = TmsManager.CTNamePlus
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
                            tuples.Add(new List<string>() { a.ToString(), b.ToString(), c.ToString(), (a + b) == c ? TmsManager.TrueValue : TmsManager.FalseValue });
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
