using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Razr.CS3.mqm;
using System.Diagnostics;
using System.Linq;

namespace Gecko.StructuralReasoner.Tests
{
    [TestClass]
    public class Cs3Tests
    {
        [TestMethod]
        public void ClearWorkspaceTest1()
        {
            ICDAStar cdaStarTms = new Workspace();
            String[] boolDomainValues = new string[] { "T", "F" };
            IDomain boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues);
            IConstraintType ctT = cdaStarTms.DefConstraintType(
                    "Only_True_Allowed",
                    new IDomain[1] { boolDomain },
                    new string[][] { new string[] { "T" } }
            );

            cdaStarTms.DefAggregateUtility((double[] utilities) => { return utilities.Sum() / 2; });

            IVariable valA = cdaStarTms.DefVariable("val_A", boolDomain);
            IVariable valB = cdaStarTms.DefVariable("val_B", boolDomain);

            cdaStarTms.DefConstraint(ctT, new IVariable[] { valA });
            cdaStarTms.DefConstraint(ctT, new IVariable[] { valA });

            cdaStarTms.DefAttribute(valA.Name);
            cdaStarTms.set_Utility(valA.Name, 0, 2);
            cdaStarTms.set_Utility(valA.Name, 1, 0);

            cdaStarTms.DefAttribute(valB.Name);
            cdaStarTms.set_Utility(valB.Name, 0, 2);
            cdaStarTms.set_Utility(valB.Name, 1, 0);

            ISolutionsIterator solutionIterator = cdaStarTms.Solutions();
            ISolution solution = solutionIterator.FirstSolution();

            while (solution != null)
            {
                Debug.WriteLine("Utility: " + solution.Utility());
                Debug.WriteLine("AsString: " + solution.AsString());
                Debug.WriteLine("");
                solution = solutionIterator.NextSolution();
            }

            // --------------------------------------------------------------------------//

            cdaStarTms.Clear();
            IVariable testVar = cdaStarTms.DefVariable("test", boolDomain);
            IVariable softVar = cdaStarTms.DefVariable("soft", boolDomain);

            cdaStarTms.DefConstraint(ctT, new IVariable[] { testVar });

            cdaStarTms.DefAggregateUtility((double[] utilities) => { return utilities.Sum() * 10; });

            cdaStarTms.DefAttribute(softVar.Name);
            cdaStarTms.set_Utility(softVar.Name, 0, 0);
            cdaStarTms.set_Utility(softVar.Name, 1, 1);
            cdaStarTms.DefAttribute(testVar.Name);

            solutionIterator = cdaStarTms.Solutions();
            solution = solutionIterator.FirstSolution();

            while (solution != null)
            {
                Debug.WriteLine("Utility: " + solution.Utility());
                Debug.WriteLine("AsString: " + solution.AsString());
                Debug.WriteLine("");

                // For some reasone the solution only returns F and enters an infinite loop, 
                // so it loops through the while indefinite number of times
                Assert.IsFalse(solution.AsString() == "F");

                solution = solutionIterator.NextSolution();
            }
        }

        [TestMethod]
        public void ClearWorkspaceTest2()
        {
            ICDAStar cdaStarTms = new Workspace();
            // Domains
            String[] boolDomainValues = new string[] { "T", "F" };
            String[] metricDomainValues = new string[] { "1", "2", "3" };
            IDomain boolDomain = cdaStarTms.DefDomain("Boolean", boolDomainValues); ;
            IDomain metricDomain = cdaStarTms.DefDomain("Metric", metricDomainValues);

            // Constraint types
            IConstraintType ctOnly1Allowed = cdaStarTms.DefConstraintType(
                    "Only_1_Allowed",
                    new IDomain[1] { metricDomain },
                    new string[][] { new string[] { "1" } }
           );
            IConstraintType ctOnlyTrueAllowed = cdaStarTms.DefConstraintType(
                    "Only_True_Allowed",
                    new IDomain[1] { boolDomain },
                    new string[][] { new string[] { "T" } }
            );
            IConstraintType ctF = cdaStarTms.DefConstraintType(
                "Only_False_Allowed",
                new IDomain[1] { boolDomain },
                new string[][] { new string[] { "F" } }
                );
            IConstraintType ctNotEquals;
            IConstraintType ctLE;
            // --------------------------------------------------------------------------//
            // Creating additional constraint types
            // Creating the not equals constraint type, i.e. A != B
            // If a tuple <A, B> satisfies the condition then the third part of the relation is True, otherwise False - this allows using soft constraints and is used for the utility function
            var ctSignature = new IDomain[3] { metricDomain, metricDomain, boolDomain };
            var ctTuples = new string[3 * 3][];
            var tupleCounter = 0;
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
            // --------------------------------------------------------------------------//
            //  Setting the utility function
            cdaStarTms.DefAggregateUtility((double[] utilities) => { return utilities.Sum() / 2; });

            // Variables
            IVariable positionA = cdaStarTms.DefVariable("position_A", metricDomain);
            IVariable positionB = cdaStarTms.DefVariable("position_B", metricDomain);
            IVariable metricVal1 = cdaStarTms.DefVariable("metric_val-1", metricDomain);
            // Satisfiability variables used for utility
            IVariable ANotEqBSatisfied = cdaStarTms.DefVariable("position_A-NotEquals-position_B", boolDomain);
            IVariable ALessOrEquals1Satisfied = cdaStarTms.DefVariable("position_A-LessOrEquals-1", boolDomain);
            IVariable BLessOrEquals1Satisfied = cdaStarTms.DefVariable("position_B-LessOrEquals-1", boolDomain);
            //// --------------------------------------------------------------------------//
            //// Attributes and utilities
            //// Setting the non-utility variables as attributes
            //// These variables do not take part in the utility function,
            //// but there assignment is the actual solution which is sought
            cdaStarTms.DefAttribute(positionA.Name);
            cdaStarTms.DefAttribute(positionB.Name);
            cdaStarTms.DefAttribute(metricVal1.Name);
            // Each constraint in the problem has a satisfaction variable with boolean domain.
            // If a constraint is satisfied, its satisfaction variable is True, otherwise - False.
            // If the value is True (i.e. the constraint is satisfied) it has utility - 1, otherwise - 0
            cdaStarTms.DefAttribute(ANotEqBSatisfied.Name);
            cdaStarTms.set_Utility(ANotEqBSatisfied.Name, 0, 3);
            cdaStarTms.set_Utility(ANotEqBSatisfied.Name, 1, 0);
            cdaStarTms.DefAttribute(ALessOrEquals1Satisfied.Name);
            cdaStarTms.set_Utility(ALessOrEquals1Satisfied.Name, 0, 2);
            cdaStarTms.set_Utility(ALessOrEquals1Satisfied.Name, 1, 0);
            cdaStarTms.DefAttribute(BLessOrEquals1Satisfied.Name);
            cdaStarTms.set_Utility(BLessOrEquals1Satisfied.Name, 0, 2);
            cdaStarTms.set_Utility(BLessOrEquals1Satisfied.Name, 1, 0);
            ////// --------------------------------------------------------------------------//

            // --------------------------------------------------------------------------//
            // Constraints
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
            // --------------------------------------------------------------------------//

            // --------------------------------------------------------------------------//
            // Solving
            ISolutionsIterator solutionIterator;
            ISolution solution;
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
            // --------------------------------------------------------------------------//

            // PROBLEM 2 - works only if the first problem has no CDA* attributes defined
            cdaStarTms.Clear();
            IVariable testVar = cdaStarTms.DefVariable("test", boolDomain);
            IVariable softVar = cdaStarTms.DefVariable("soft", boolDomain);
            // Trying clearing
            IConstraint constraint = cdaStarTms.DefConstraint(ctOnlyTrueAllowed, new IVariable[] { testVar });
            //IConstraint constraintContradicting = cdaStarTms.DefConstraint(ctF, new IVariable[] { testVar });
            // The function simply returns the sum of utilities
            cdaStarTms.DefAggregateUtility((double[] utilities) => { return utilities.Sum() * 10; });
            // Adding the two variables as attributes
            cdaStarTms.DefAttribute(softVar.Name);
            cdaStarTms.set_Utility(softVar.Name, 0, 0);
            cdaStarTms.set_Utility(softVar.Name, 1, 1);
            // testVar is not relevant to the utility function
            cdaStarTms.DefAttribute(testVar.Name);
            //cdaStarTms.set_Utility(testVar.Name, 0, 0);
            //cdaStarTms.set_Utility(testVar.Name, 1, 0);
            solutionIterator = cdaStarTms.Solutions();
            try
            {
                // The exception is currently thrown here
                solution = solutionIterator.FirstSolution();
                while (solution != null)
                {
                    Debug.WriteLine("Utility: " + solution.Utility());
                    Debug.WriteLine("Test var: " + boolDomainValues[solution.Value(testVar.Name)]);
                    Debug.WriteLine("Soft var: " + boolDomainValues[solution.Value(softVar.Name)]);
                    Debug.WriteLine("AsString: " + solution.AsString());
                    Debug.WriteLine("");
                    solution = solutionIterator.NextSolution();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }

    }
}