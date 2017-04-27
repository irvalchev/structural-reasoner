using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gecko.StructuralReasoner.Relations
{
    // To make it easier in the moment the class is static
    public static class StructuralRelationsManager
    {
        public const int MaxMetricValue = 100;

        static StructuralRelationsManager()
        {
            RelationFamilies = new List<RelationFamily>();
            Relations = new List<BinaryRelation>();
            CalculiDomains = new List<GKODomain>();

            CreateMetricRelations();
            CreateIntervalAlgebra();
            CreatePointsAlgebra();
            CreateRcc8();
        }

        public static List<RelationFamily> RelationFamilies { get; private set; }
        public static List<BinaryRelation> Relations { get; private set; }

        /// <summary>
        /// The domains used for the defined structural relations
        /// </summary>
        public static List<GKODomain> CalculiDomains { get; private set; }

        /// <summary>
        /// The domain used for the metric relations
        /// </summary>
        public static GKOIntDomain MetricDomain { get; private set; }

        public static RelationFamily Rcc8Calculus
        {
            get
            {
                return StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.Rcc8Name);
            }
        }

        public static RelationFamily IntervalAlgebra
        {
            get
            {
                return StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.IntervalAlgebraName);
            }
        }

        public static RelationFamily PointAlgebra
        {
            get
            {
                return StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.PointsAlgebraName);
            }
        }

        public static RelationFamily MetricRelationsFamily
        {
            get
            {
                return StructuralRelationsManager.GetRelationFamily(RelationFamilyNames.MetricRelationsName);
            }
        }

        /// <summary>
        /// Find a relation family by name
        /// </summary>
        /// <param name="familyName">The name of the family. Use one of the predefined constants for this parameter</param>
        /// <returns>The found family or null if it is not found</returns>
        public static RelationFamily GetRelationFamily(string familyName)
        {
            RelationFamily foundFamily = null;

            foundFamily = RelationFamilies.SingleOrDefault(x => x.Name == familyName);

            return foundFamily;
        }

        /// <summary>
        /// Finds a domain in the list of defined domains by name
        /// </summary>
        /// <param name="domainName">The name of the domain</param>
        /// <returns>The doimain if found, otherwise null</returns>
        public static GKODomain GetDomain(string domainName)
        {
            GKODomain domain = null;

            domain = CalculiDomains.SingleOrDefault(x => x.Name == domainName);

            return domain;
        }

        /// <summary>
        /// Find a relation
        /// </summary>
        /// <param name="familyName">The name of the family of the relation. Use null if the relation is not in a family, otherwise use one of the predefined constants for this parameter</param>
        /// <param name="relationName">The name of the relation. Use one of the predefined constants for this parameter</param>
        /// <returns>The found relation or null if it is not found</returns>
        public static BinaryRelation GetRelation(string familyName, string relationName)
        {
            BinaryRelation foundRelation = null;

            foundRelation = Relations.SingleOrDefault(
                x => x.Name == relationName &&
                ((string.IsNullOrWhiteSpace(familyName) && x.RelationFamily == null) ||
                (x.RelationFamily != null && x.RelationFamily.Name == familyName))
                );

            return foundRelation;
        }

        /// <summary>
        /// Generates the composition constraint types for the included calculi
        /// </summary>
        /// <returns></returns>
        public static List<GKOConstraintType> GenerateCompositionConstraintTypes()
        {
            List<GKOConstraintType> types = new List<GKOConstraintType>();

            types.AddRange(GenerateRcc8ConstrTypes());
            types.AddRange(GeneratePointsAlgebraConstrTypes());
            types.AddRange(GenerateIntervalAlgebraConstrTypes());

            return types;
        }

        private static void CreateRcc8()
        {
            List<BinaryRelation> rccRelations = new List<BinaryRelation>();
            BinaryRelation rel;
            string relName;
            string relMeaning;
            List<Type> relSignature;
            RelationFamily family;
            GKODomain relationsDomain;

            #region Relations
            // Disconnected (DC)
            relName = Rcc8RelationNames.DC;
            relMeaning = "The two relation components are disconnected";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Externally connected (EC)
            relName = Rcc8RelationNames.EC;
            relMeaning = "The two relation components are edge connected";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Equal (EQ)
            relName = Rcc8RelationNames.EQ;
            relMeaning = "The two relation components are equal";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Partially overlapping (PO)
            relName = Rcc8RelationNames.PO;
            relMeaning = "The two relation components are partially overlapping";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Tangential proper part (TPP)
            relName = Rcc8RelationNames.TPP;
            relMeaning = "The first relation component is included in the second and some of the border overlaps";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Tangential proper part inverse (TPPi)
            relName = Rcc8RelationNames.TPPi;
            relMeaning = "The second relation component is included in the first and some of the border overlaps";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Non-tangential proper part (NTPP)
            relName = Rcc8RelationNames.NTPP;
            relMeaning = "The first relation component is included in the second and the borders don't overlap";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);
            // Non-tangential proper part inverse (NTPPi)
            relName = Rcc8RelationNames.NTPPi;
            relMeaning = "The second relation component is included in the first and the borders don't overlap";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            rccRelations.Add(rel);

            // Adding the RCC8 relation family in the list of families
            family = new RelationFamily(RelationFamilyNames.Rcc8Name, rccRelations);
            Relations.AddRange(rccRelations);
            RelationFamilies.Add(family);
            family.EqualsRelation = StructuralRelationsManager.GetRelation(family.Name, Rcc8RelationNames.EQ);
            #endregion

            // Creating the relation family domain
            relationsDomain = new GKODomain()
            {
                Id = family.GetTmsRelationsDomainName(),
                Name = family.GetTmsRelationsDomainName(),
                Values = family.Relations.Select(x => x.Name).ToList()
            };
            CalculiDomains.Add(relationsDomain);
        }

        private static IEnumerable<GKOConstraintType> GenerateRcc8ConstrTypes()
        {
            GKOConstraintType constraintType;
            List<List<string>> tuples;
            List<GKOConstraintType> constraintTypes = new List<GKOConstraintType>();
            RelationFamily family = GetRelationFamily(RelationFamilyNames.Rcc8Name);
            GKODomain relationsDomain = GetDomain(family.GetTmsRelationsDomainName());

            // Creating the constraint type for the relations composition
            constraintType = new GKOConstraintType()
            {
                Id = family.GetTmsCompositionConstraintName(),
                Name = family.GetTmsCompositionConstraintName()
            };
            constraintType.Signature = new List<GKODomainAbstract>() { relationsDomain, relationsDomain, relationsDomain };

            tuples = new List<List<string>>();
            #region tuples
            tuples.Add(new List<string>() { "DC", "DC", "DC" });
            tuples.Add(new List<string>() { "DC", "DC", "EC" });
            tuples.Add(new List<string>() { "DC", "DC", "PO" });
            tuples.Add(new List<string>() { "DC", "DC", "TPP" });
            tuples.Add(new List<string>() { "DC", "DC", "NTPP" });
            tuples.Add(new List<string>() { "DC", "DC", "TPPi" });
            tuples.Add(new List<string>() { "DC", "DC", "NTPPi" });
            tuples.Add(new List<string>() { "DC", "DC", "EQ" });
            tuples.Add(new List<string>() { "DC", "EC", "DC" });
            tuples.Add(new List<string>() { "DC", "EC", "EC" });
            tuples.Add(new List<string>() { "DC", "EC", "PO" });
            tuples.Add(new List<string>() { "DC", "EC", "TPP" });
            tuples.Add(new List<string>() { "DC", "EC", "NTPP" });
            tuples.Add(new List<string>() { "DC", "PO", "DC" });
            tuples.Add(new List<string>() { "DC", "PO", "EC" });
            tuples.Add(new List<string>() { "DC", "PO", "PO" });
            tuples.Add(new List<string>() { "DC", "PO", "TPP" });
            tuples.Add(new List<string>() { "DC", "PO", "NTPP" });
            tuples.Add(new List<string>() { "DC", "TPP", "DC" });
            tuples.Add(new List<string>() { "DC", "TPP", "EC" });
            tuples.Add(new List<string>() { "DC", "TPP", "PO" });
            tuples.Add(new List<string>() { "DC", "TPP", "TPP" });
            tuples.Add(new List<string>() { "DC", "TPP", "NTPP" });
            tuples.Add(new List<string>() { "DC", "NTPP", "DC" });
            tuples.Add(new List<string>() { "DC", "NTPP", "EC" });
            tuples.Add(new List<string>() { "DC", "NTPP", "PO" });
            tuples.Add(new List<string>() { "DC", "NTPP", "TPP" });
            tuples.Add(new List<string>() { "DC", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "DC", "TPPi", "DC" });
            tuples.Add(new List<string>() { "DC", "NTPPi", "DC" });
            tuples.Add(new List<string>() { "DC", "EQ", "DC" });
            tuples.Add(new List<string>() { "EC", "DC", "DC" });
            tuples.Add(new List<string>() { "EC", "DC", "EC" });
            tuples.Add(new List<string>() { "EC", "DC", "PO" });
            tuples.Add(new List<string>() { "EC", "DC", "TPPi" });
            tuples.Add(new List<string>() { "EC", "DC", "NTPPi" });
            tuples.Add(new List<string>() { "EC", "EC", "DC" });
            tuples.Add(new List<string>() { "EC", "EC", "EC" });
            tuples.Add(new List<string>() { "EC", "EC", "PO" });
            tuples.Add(new List<string>() { "EC", "EC", "TPP" });
            tuples.Add(new List<string>() { "EC", "EC", "TPPi" });
            tuples.Add(new List<string>() { "EC", "EC", "EQ" });
            tuples.Add(new List<string>() { "EC", "PO", "DC" });
            tuples.Add(new List<string>() { "EC", "PO", "EC" });
            tuples.Add(new List<string>() { "EC", "PO", "PO" });
            tuples.Add(new List<string>() { "EC", "PO", "TPP" });
            tuples.Add(new List<string>() { "EC", "PO", "NTPP" });
            tuples.Add(new List<string>() { "EC", "TPP", "EC" });
            tuples.Add(new List<string>() { "EC", "TPP", "PO" });
            tuples.Add(new List<string>() { "EC", "TPP", "TPP" });
            tuples.Add(new List<string>() { "EC", "TPP", "NTPP" });
            tuples.Add(new List<string>() { "EC", "NTPP", "PO" });
            tuples.Add(new List<string>() { "EC", "NTPP", "TPP" });
            tuples.Add(new List<string>() { "EC", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "EC", "TPPi", "DC" });
            tuples.Add(new List<string>() { "EC", "TPPi", "EC" });
            tuples.Add(new List<string>() { "EC", "NTPPi", "DC" });
            tuples.Add(new List<string>() { "EC", "EQ", "EC" });
            tuples.Add(new List<string>() { "PO", "DC", "DC" });
            tuples.Add(new List<string>() { "PO", "DC", "EC" });
            tuples.Add(new List<string>() { "PO", "DC", "PO" });
            tuples.Add(new List<string>() { "PO", "DC", "TPPi" });
            tuples.Add(new List<string>() { "PO", "DC", "NTPPi" });
            tuples.Add(new List<string>() { "PO", "EC", "DC" });
            tuples.Add(new List<string>() { "PO", "EC", "EC" });
            tuples.Add(new List<string>() { "PO", "EC", "PO" });
            tuples.Add(new List<string>() { "PO", "EC", "TPPi" });
            tuples.Add(new List<string>() { "PO", "EC", "NTPPi" });
            tuples.Add(new List<string>() { "PO", "PO", "DC" });
            tuples.Add(new List<string>() { "PO", "PO", "EC" });
            tuples.Add(new List<string>() { "PO", "PO", "PO" });
            tuples.Add(new List<string>() { "PO", "PO", "TPP" });
            tuples.Add(new List<string>() { "PO", "PO", "NTPP" });
            tuples.Add(new List<string>() { "PO", "PO", "TPPi" });
            tuples.Add(new List<string>() { "PO", "PO", "NTPPi" });
            tuples.Add(new List<string>() { "PO", "PO", "EQ" });
            tuples.Add(new List<string>() { "PO", "TPP", "PO" });
            tuples.Add(new List<string>() { "PO", "TPP", "TPP" });
            tuples.Add(new List<string>() { "PO", "TPP", "NTPP" });
            tuples.Add(new List<string>() { "PO", "NTPP", "PO" });
            tuples.Add(new List<string>() { "PO", "NTPP", "TPP" });
            tuples.Add(new List<string>() { "PO", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "PO", "TPPi", "DC" });
            tuples.Add(new List<string>() { "PO", "TPPi", "EC" });
            tuples.Add(new List<string>() { "PO", "TPPi", "PO" });
            tuples.Add(new List<string>() { "PO", "TPPi", "TPPi" });
            tuples.Add(new List<string>() { "PO", "TPPi", "NTPPi" });
            tuples.Add(new List<string>() { "PO", "NTPPi", "DC" });
            tuples.Add(new List<string>() { "PO", "NTPPi", "EC" });
            tuples.Add(new List<string>() { "PO", "NTPPi", "PO" });
            tuples.Add(new List<string>() { "PO", "NTPPi", "TPPi" });
            tuples.Add(new List<string>() { "PO", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "PO", "EQ", "PO" });
            tuples.Add(new List<string>() { "TPP", "DC", "DC" });
            tuples.Add(new List<string>() { "TPP", "EC", "DC" });
            tuples.Add(new List<string>() { "TPP", "EC", "EC" });
            tuples.Add(new List<string>() { "TPP", "PO", "DC" });
            tuples.Add(new List<string>() { "TPP", "PO", "EC" });
            tuples.Add(new List<string>() { "TPP", "PO", "PO" });
            tuples.Add(new List<string>() { "TPP", "PO", "TPP" });
            tuples.Add(new List<string>() { "TPP", "PO", "NTPP" });
            tuples.Add(new List<string>() { "TPP", "TPP", "TPP" });
            tuples.Add(new List<string>() { "TPP", "TPP", "NTPP" });
            tuples.Add(new List<string>() { "TPP", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "DC" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "EC" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "PO" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "TPP" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "TPPi" });
            tuples.Add(new List<string>() { "TPP", "TPPi", "EQ" });
            tuples.Add(new List<string>() { "TPP", "NTPPi", "DC" });
            tuples.Add(new List<string>() { "TPP", "NTPPi", "EC" });
            tuples.Add(new List<string>() { "TPP", "NTPPi", "PO" });
            tuples.Add(new List<string>() { "TPP", "NTPPi", "TPPi" });
            tuples.Add(new List<string>() { "TPP", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "TPP", "EQ", "TPP" });
            tuples.Add(new List<string>() { "NTPP", "DC", "DC" });
            tuples.Add(new List<string>() { "NTPP", "EC", "DC" });
            tuples.Add(new List<string>() { "NTPP", "PO", "DC" });
            tuples.Add(new List<string>() { "NTPP", "PO", "EC" });
            tuples.Add(new List<string>() { "NTPP", "PO", "PO" });
            tuples.Add(new List<string>() { "NTPP", "PO", "TPP" });
            tuples.Add(new List<string>() { "NTPP", "PO", "NTPP" });
            tuples.Add(new List<string>() { "NTPP", "TPP", "NTPP" });
            tuples.Add(new List<string>() { "NTPP", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "NTPP", "TPPi", "DC" });
            tuples.Add(new List<string>() { "NTPP", "TPPi", "EC" });
            tuples.Add(new List<string>() { "NTPP", "TPPi", "PO" });
            tuples.Add(new List<string>() { "NTPP", "TPPi", "TPP" });
            tuples.Add(new List<string>() { "NTPP", "TPPi", "NTPP" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "DC" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "EC" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "PO" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "TPP" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "NTPP" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "TPPi" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "NTPP", "NTPPi", "EQ" });
            tuples.Add(new List<string>() { "NTPP", "EQ", "NTPP" });
            tuples.Add(new List<string>() { "TPPi", "DC", "DC" });
            tuples.Add(new List<string>() { "TPPi", "DC", "EC" });
            tuples.Add(new List<string>() { "TPPi", "DC", "PO" });
            tuples.Add(new List<string>() { "TPPi", "DC", "TPPi" });
            tuples.Add(new List<string>() { "TPPi", "DC", "NTPPi" });
            tuples.Add(new List<string>() { "TPPi", "EC", "EC" });
            tuples.Add(new List<string>() { "TPPi", "EC", "PO" });
            tuples.Add(new List<string>() { "TPPi", "EC", "TPPi" });
            tuples.Add(new List<string>() { "TPPi", "EC", "NTPPi" });
            tuples.Add(new List<string>() { "TPPi", "PO", "PO" });
            tuples.Add(new List<string>() { "TPPi", "PO", "TPPi" });
            tuples.Add(new List<string>() { "TPPi", "PO", "NTPPi" });
            tuples.Add(new List<string>() { "TPPi", "TPP", "PO" });
            tuples.Add(new List<string>() { "TPPi", "TPP", "TPP" });
            tuples.Add(new List<string>() { "TPPi", "TPP", "TPPi" });
            tuples.Add(new List<string>() { "TPPi", "TPP", "EQ" });
            tuples.Add(new List<string>() { "TPPi", "NTPP", "PO" });
            tuples.Add(new List<string>() { "TPPi", "NTPP", "TPP" });
            tuples.Add(new List<string>() { "TPPi", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "TPPi", "TPPi", "TPPi" });
            tuples.Add(new List<string>() { "TPPi", "TPPi", "NTPPi" });
            tuples.Add(new List<string>() { "TPPi", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "TPPi", "EQ", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "DC", "DC" });
            tuples.Add(new List<string>() { "NTPPi", "DC", "EC" });
            tuples.Add(new List<string>() { "NTPPi", "DC", "PO" });
            tuples.Add(new List<string>() { "NTPPi", "DC", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "DC", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "EC", "PO" });
            tuples.Add(new List<string>() { "NTPPi", "EC", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "EC", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "PO", "PO" });
            tuples.Add(new List<string>() { "NTPPi", "PO", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "PO", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "TPP", "PO" });
            tuples.Add(new List<string>() { "NTPPi", "TPP", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "TPP", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "PO" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "TPP" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "TPPi" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "NTPP", "EQ" });
            tuples.Add(new List<string>() { "NTPPi", "TPPi", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "NTPPi", "EQ", "NTPPi" });
            tuples.Add(new List<string>() { "EQ", "DC", "DC" });
            tuples.Add(new List<string>() { "EQ", "EC", "EC" });
            tuples.Add(new List<string>() { "EQ", "PO", "PO" });
            tuples.Add(new List<string>() { "EQ", "TPP", "TPP" });
            tuples.Add(new List<string>() { "EQ", "NTPP", "NTPP" });
            tuples.Add(new List<string>() { "EQ", "TPPi", "TPPi" });
            tuples.Add(new List<string>() { "EQ", "NTPPi", "NTPPi" });
            tuples.Add(new List<string>() { "EQ", "EQ", "EQ" });
            #endregion

            constraintType.Tuples = tuples;
            constraintTypes.Add(constraintType);

            return constraintTypes;
        }

        private static void CreatePointsAlgebra()
        {
            List<BinaryRelation> pointsRelations = new List<BinaryRelation>();
            BinaryRelation rel;
            string relName;
            string relMeaning;
            List<Type> relSignature;
            RelationFamily family;
            GKODomain relationsDomain;

            #region Relations
            // Before (<)
            relName = PointsAlgebraRelationNames.Before;
            relMeaning = "A is before B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            pointsRelations.Add(rel);
            // After (>)
            relName = PointsAlgebraRelationNames.After;
            relMeaning = "A is after B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            pointsRelations.Add(rel);
            // Equals (=)
            relName = PointsAlgebraRelationNames.Equals;
            relMeaning = "A and B are equal";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            pointsRelations.Add(rel);

            // Adding the points algebra relation family in the list of families
            family = new RelationFamily(RelationFamilyNames.PointsAlgebraName, pointsRelations);
            Relations.AddRange(pointsRelations);
            RelationFamilies.Add(family);
            family.EqualsRelation = StructuralRelationsManager.GetRelation(family.Name, PointsAlgebraRelationNames.Equals);
            #endregion

            // Creating the relation family domain
            relationsDomain = new GKODomain()
            {
                Id = family.GetTmsRelationsDomainName(),
                Name = family.GetTmsRelationsDomainName(),
                Values = family.Relations.Select(x => x.Name).ToList()
            };
            CalculiDomains.Add(relationsDomain);
        }

        private static IEnumerable<GKOConstraintType> GeneratePointsAlgebraConstrTypes()
        {
            GKOConstraintType constraintType;
            List<List<string>> tuples;
            List<GKOConstraintType> constraintTypes = new List<GKOConstraintType>();
            RelationFamily family = GetRelationFamily(RelationFamilyNames.PointsAlgebraName);
            GKODomain relationsDomain = GetDomain(family.GetTmsRelationsDomainName());

            // Creating the constraint type for the relations composition
            constraintType = new GKOConstraintType()
            {
                Id = family.GetTmsCompositionConstraintName(),
                Name = family.GetTmsCompositionConstraintName()
            };
            constraintType.Signature = new List<GKODomainAbstract>() { relationsDomain, relationsDomain, relationsDomain };

            tuples = new List<List<string>>();
            #region tuples
            tuples.Add(new List<string>() { "After", "After", "After" });
            tuples.Add(new List<string>() { "After", "Before", "After" });
            tuples.Add(new List<string>() { "After", "Before", "Before" });
            tuples.Add(new List<string>() { "After", "Before", "Equals" });
            tuples.Add(new List<string>() { "After", "Equals", "After" });
            tuples.Add(new List<string>() { "Before", "After", "After" });
            tuples.Add(new List<string>() { "Before", "After", "Before" });
            tuples.Add(new List<string>() { "Before", "After", "Equals" });
            tuples.Add(new List<string>() { "Before", "Before", "Before" });
            tuples.Add(new List<string>() { "Before", "Equals", "Before" });
            tuples.Add(new List<string>() { "Equals", "After", "After" });
            tuples.Add(new List<string>() { "Equals", "Before", "Before" });
            tuples.Add(new List<string>() { "Equals", "Equals", "Equals" });
            #endregion

            constraintType.Tuples = tuples;
            constraintTypes.Add(constraintType);

            return constraintTypes;
        }

        private static void CreateIntervalAlgebra()
        {
            List<BinaryRelation> intervalRelations = new List<BinaryRelation>();
            BinaryRelation rel;
            string relName;
            string relMeaning;
            List<Type> relSignature;
            RelationFamily family;
            GKODomain relationsDomain;

            #region Relations
            // Creating the Interval relations
            // Before (b)
            relName = IntervalAlgebraRelationNames.Before;
            relMeaning = "A is before B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // After (b-1)
            relName = IntervalAlgebraRelationNames.After;
            relMeaning = "A is after B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Meets (m)
            relName = IntervalAlgebraRelationNames.Meets;
            relMeaning = "The end of A is the start of B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Met-by (m-1)
            relName = IntervalAlgebraRelationNames.MetBy;
            relMeaning = "The start of A is the end of B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Overlaps (o)
            relName = IntervalAlgebraRelationNames.Overlaps;
            relMeaning = "A starts before B, B ends after A and part of A is in B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Overlapped-by (o-1)
            relName = IntervalAlgebraRelationNames.OverlappedBy;
            relMeaning = "B starts before A, A ends after B and part of B is in A";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // During (d)
            relName = IntervalAlgebraRelationNames.During;
            relMeaning = "A happens during B, but A does not include the start or end of B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Includes (d-1)
            relName = IntervalAlgebraRelationNames.Includes;
            relMeaning = "A includes B, but the start and end of A is not in B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Starts (s)
            relName = IntervalAlgebraRelationNames.Starts;
            relMeaning = "A starts B - the start of A and B are the same and A ends before B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Started-by (s-1)
            relName = IntervalAlgebraRelationNames.StartedBy;
            relMeaning = "B starts A - the start of A and B are the same and B ends before A";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Finishes (f)
            relName = IntervalAlgebraRelationNames.Finishes;
            relMeaning = "A finishes B - the end of A and B are the same and B starts before A";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Finished-by (f-1)
            relName = IntervalAlgebraRelationNames.FinishedBy;
            relMeaning = "B finishes A - the end of A and B are the same and A starts before B";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);
            // Equals (=)
            relName = IntervalAlgebraRelationNames.Equals;
            relMeaning = "A and B are equal";
            relSignature = new List<Type>() { typeof(ComponentRelationPart), typeof(ComponentRelationPart) };
            rel = new QualitativeRelation(relName, relMeaning, relSignature);
            intervalRelations.Add(rel);

            // Adding the interval relation family to the list of families
            family = new RelationFamily(RelationFamilyNames.IntervalAlgebraName, intervalRelations);
            Relations.AddRange(intervalRelations);
            RelationFamilies.Add(family);
            family.EqualsRelation = StructuralRelationsManager.GetRelation(family.Name, IntervalAlgebraRelationNames.Equals);
            #endregion

            // Creating the relation family domain
            relationsDomain = new GKODomain()
            {
                Id = family.GetTmsRelationsDomainName(),
                Name = family.GetTmsRelationsDomainName(),
                Values = family.Relations.Select(x => x.Name).ToList()
            };
            CalculiDomains.Add(relationsDomain);
        }

        private static IEnumerable<GKOConstraintType> GenerateIntervalAlgebraConstrTypes()
        {
            GKOConstraintType constraintType;
            List<List<string>> tuples;
            List<GKOConstraintType> constraintTypes = new List<GKOConstraintType>();
            RelationFamily family = GetRelationFamily(RelationFamilyNames.IntervalAlgebraName);
            GKODomain relationsDomain = GetDomain(family.GetTmsRelationsDomainName());

            // Creating the constraint type for the relations composition
            constraintType = new GKOConstraintType()
            {
                Id = family.GetTmsCompositionConstraintName(),
                Name = family.GetTmsCompositionConstraintName()
            };
            constraintType.Signature = new List<GKODomainAbstract>() { relationsDomain, relationsDomain, relationsDomain };

            tuples = new List<List<string>>();
            #region tuples
            tuples.Add(new List<string>() { "Before", "Before", "Before" });
            tuples.Add(new List<string>() { "Before", "Meets", "Before" });
            tuples.Add(new List<string>() { "Before", "Overlaps", "Before" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "Before", "Includes", "Before" });
            tuples.Add(new List<string>() { "Before", "Starts", "Before" });
            tuples.Add(new List<string>() { "Before", "Equals", "Before" });
            tuples.Add(new List<string>() { "Before", "Started-by", "Before" });
            tuples.Add(new List<string>() { "Before", "Includes", "Before" });
            tuples.Add(new List<string>() { "Before", "Includes", "Meets" });
            tuples.Add(new List<string>() { "Before", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Before", "Includes", "Starts" });
            tuples.Add(new List<string>() { "Before", "Includes", "During" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "Meets" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "Starts" });
            tuples.Add(new List<string>() { "Before", "Finished-by", "During" });
            tuples.Add(new List<string>() { "Before", "Overlapped-by", "Before" });
            tuples.Add(new List<string>() { "Before", "Overlapped-by", "Meets" });
            tuples.Add(new List<string>() { "Before", "Overlapped-by", "Overlaps" });
            tuples.Add(new List<string>() { "Before", "Overlapped-by", "Starts" });
            tuples.Add(new List<string>() { "Before", "Overlapped-by", "During" });
            tuples.Add(new List<string>() { "Before", "Met-by", "Before" });
            tuples.Add(new List<string>() { "Before", "Met-by", "Meets" });
            tuples.Add(new List<string>() { "Before", "Met-by", "Overlaps" });
            tuples.Add(new List<string>() { "Before", "Met-by", "Starts" });
            tuples.Add(new List<string>() { "Before", "Met-by", "During" });
            tuples.Add(new List<string>() { "Before", "After", "Before" });
            tuples.Add(new List<string>() { "Before", "After", "Meets" });
            tuples.Add(new List<string>() { "Before", "After", "Overlaps" });
            tuples.Add(new List<string>() { "Before", "After", "Finished-by" });
            tuples.Add(new List<string>() { "Before", "After", "Includes" });
            tuples.Add(new List<string>() { "Before", "After", "Starts" });
            tuples.Add(new List<string>() { "Before", "After", "Equals" });
            tuples.Add(new List<string>() { "Before", "After", "Started-by" });
            tuples.Add(new List<string>() { "Before", "After", "During" });
            tuples.Add(new List<string>() { "Before", "After", "Finishes" });
            tuples.Add(new List<string>() { "Before", "After", "Overlapped-by" });
            tuples.Add(new List<string>() { "Before", "After", "Met-by" });
            tuples.Add(new List<string>() { "Before", "After", "After" });
            tuples.Add(new List<string>() { "Meets", "Before", "Before" });
            tuples.Add(new List<string>() { "Meets", "Meets", "Before" });
            tuples.Add(new List<string>() { "Meets", "Overlaps", "Before" });
            tuples.Add(new List<string>() { "Meets", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "Meets", "Includes", "Before" });
            tuples.Add(new List<string>() { "Meets", "Starts", "Meets" });
            tuples.Add(new List<string>() { "Meets", "Equals", "Meets" });
            tuples.Add(new List<string>() { "Meets", "Started-by", "Meets" });
            tuples.Add(new List<string>() { "Meets", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Meets", "Includes", "Starts" });
            tuples.Add(new List<string>() { "Meets", "Includes", "During" });
            tuples.Add(new List<string>() { "Meets", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "Meets", "Finished-by", "Starts" });
            tuples.Add(new List<string>() { "Meets", "Finished-by", "During" });
            tuples.Add(new List<string>() { "Meets", "Overlapped-by", "Overlaps" });
            tuples.Add(new List<string>() { "Meets", "Overlapped-by", "Starts" });
            tuples.Add(new List<string>() { "Meets", "Overlapped-by", "During" });
            tuples.Add(new List<string>() { "Meets", "Met-by", "Finished-by" });
            tuples.Add(new List<string>() { "Meets", "Met-by", "Equals" });
            tuples.Add(new List<string>() { "Meets", "Met-by", "Finishes" });
            tuples.Add(new List<string>() { "Meets", "After", "Includes" });
            tuples.Add(new List<string>() { "Meets", "After", "Started-by" });
            tuples.Add(new List<string>() { "Meets", "After", "Overlapped-by" });
            tuples.Add(new List<string>() { "Meets", "After", "Met-by" });
            tuples.Add(new List<string>() { "Meets", "After", "After" });
            tuples.Add(new List<string>() { "Overlaps", "Before", "Before" });
            tuples.Add(new List<string>() { "Overlaps", "Meets", "Before" });
            tuples.Add(new List<string>() { "Overlaps", "Overlaps", "Before" });
            tuples.Add(new List<string>() { "Overlaps", "Overlaps", "Meets" });
            tuples.Add(new List<string>() { "Overlaps", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "Meets" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Before" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Meets" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Finished-by" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Overlaps", "Starts", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Equals", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Started-by", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Started-by", "Finished-by" });
            tuples.Add(new List<string>() { "Overlaps", "Started-by", "Includes" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "Starts" });
            tuples.Add(new List<string>() { "Overlaps", "Includes", "During" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "Starts" });
            tuples.Add(new List<string>() { "Overlaps", "Finished-by", "During" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Overlaps" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Finished-by" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Includes" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Starts" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Equals" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Started-by" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "During" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Finishes" });
            tuples.Add(new List<string>() { "Overlaps", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlaps", "Met-by", "Includes" });
            tuples.Add(new List<string>() { "Overlaps", "Met-by", "Started-by" });
            tuples.Add(new List<string>() { "Overlaps", "Met-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlaps", "After", "Includes" });
            tuples.Add(new List<string>() { "Overlaps", "After", "Started-by" });
            tuples.Add(new List<string>() { "Overlaps", "After", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlaps", "After", "Met-by" });
            tuples.Add(new List<string>() { "Overlaps", "After", "After" });
            tuples.Add(new List<string>() { "Finished-by", "Before", "Before" });
            tuples.Add(new List<string>() { "Finished-by", "Meets", "Meets" });
            tuples.Add(new List<string>() { "Finished-by", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Finished-by", "Finished-by", "Finished-by" });
            tuples.Add(new List<string>() { "Finished-by", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Finished-by", "Starts", "Overlaps" });
            tuples.Add(new List<string>() { "Finished-by", "Equals", "Finished-by" });
            tuples.Add(new List<string>() { "Finished-by", "Started-by", "Includes" });
            tuples.Add(new List<string>() { "Finished-by", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Finished-by", "Includes", "Starts" });
            tuples.Add(new List<string>() { "Finished-by", "Includes", "During" });
            tuples.Add(new List<string>() { "Finished-by", "Finished-by", "Finished-by" });
            tuples.Add(new List<string>() { "Finished-by", "Finished-by", "Equals" });
            tuples.Add(new List<string>() { "Finished-by", "Finished-by", "Finishes" });
            tuples.Add(new List<string>() { "Finished-by", "Overlapped-by", "Includes" });
            tuples.Add(new List<string>() { "Finished-by", "Overlapped-by", "Started-by" });
            tuples.Add(new List<string>() { "Finished-by", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finished-by", "Met-by", "Includes" });
            tuples.Add(new List<string>() { "Finished-by", "Met-by", "Started-by" });
            tuples.Add(new List<string>() { "Finished-by", "Met-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finished-by", "After", "Includes" });
            tuples.Add(new List<string>() { "Finished-by", "After", "Started-by" });
            tuples.Add(new List<string>() { "Finished-by", "After", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finished-by", "After", "Met-by" });
            tuples.Add(new List<string>() { "Finished-by", "After", "After" });
            tuples.Add(new List<string>() { "Includes", "Before", "Before" });
            tuples.Add(new List<string>() { "Includes", "Before", "Meets" });
            tuples.Add(new List<string>() { "Includes", "Before", "Overlaps" });
            tuples.Add(new List<string>() { "Includes", "Before", "Finished-by" });
            tuples.Add(new List<string>() { "Includes", "Before", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Meets", "Overlaps" });
            tuples.Add(new List<string>() { "Includes", "Meets", "Finished-by" });
            tuples.Add(new List<string>() { "Includes", "Meets", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Includes", "Overlaps", "Finished-by" });
            tuples.Add(new List<string>() { "Includes", "Overlaps", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Finished-by", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Starts", "Overlaps" });
            tuples.Add(new List<string>() { "Includes", "Starts", "Finished-by" });
            tuples.Add(new List<string>() { "Includes", "Starts", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Equals", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Started-by", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Finished-by" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Starts" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Equals" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Started-by" });
            tuples.Add(new List<string>() { "Includes", "Includes", "During" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "Includes", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Includes", "Finished-by", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Finished-by", "Started-by" });
            tuples.Add(new List<string>() { "Includes", "Finished-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Includes", "Overlapped-by", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Overlapped-by", "Started-by" });
            tuples.Add(new List<string>() { "Includes", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Includes", "Met-by", "Includes" });
            tuples.Add(new List<string>() { "Includes", "Met-by", "Started-by" });
            tuples.Add(new List<string>() { "Includes", "Met-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Includes", "After", "Includes" });
            tuples.Add(new List<string>() { "Includes", "After", "Started-by" });
            tuples.Add(new List<string>() { "Includes", "After", "Overlapped-by" });
            tuples.Add(new List<string>() { "Includes", "After", "Met-by" });
            tuples.Add(new List<string>() { "Includes", "After", "After" });
            tuples.Add(new List<string>() { "Starts", "Before", "Before" });
            tuples.Add(new List<string>() { "Starts", "Meets", "Before" });
            tuples.Add(new List<string>() { "Starts", "Overlaps", "Before" });
            tuples.Add(new List<string>() { "Starts", "Overlaps", "Meets" });
            tuples.Add(new List<string>() { "Starts", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Starts", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "Starts", "Finished-by", "Meets" });
            tuples.Add(new List<string>() { "Starts", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "Starts", "Includes", "Before" });
            tuples.Add(new List<string>() { "Starts", "Includes", "Meets" });
            tuples.Add(new List<string>() { "Starts", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "Starts", "Includes", "Finished-by" });
            tuples.Add(new List<string>() { "Starts", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Starts", "Starts", "Starts" });
            tuples.Add(new List<string>() { "Starts", "Equals", "Starts" });
            tuples.Add(new List<string>() { "Starts", "Started-by", "Starts" });
            tuples.Add(new List<string>() { "Starts", "Started-by", "Equals" });
            tuples.Add(new List<string>() { "Starts", "Started-by", "Started-by" });
            tuples.Add(new List<string>() { "Starts", "Includes", "During" });
            tuples.Add(new List<string>() { "Starts", "Finished-by", "During" });
            tuples.Add(new List<string>() { "Starts", "Overlapped-by", "During" });
            tuples.Add(new List<string>() { "Starts", "Overlapped-by", "Finishes" });
            tuples.Add(new List<string>() { "Starts", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Starts", "Met-by", "Met-by" });
            tuples.Add(new List<string>() { "Starts", "After", "After" });
            tuples.Add(new List<string>() { "Equals", "Before", "Before" });
            tuples.Add(new List<string>() { "Equals", "Meets", "Meets" });
            tuples.Add(new List<string>() { "Equals", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Equals", "Finished-by", "Finished-by" });
            tuples.Add(new List<string>() { "Equals", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Equals", "Starts", "Starts" });
            tuples.Add(new List<string>() { "Equals", "Equals", "Equals" });
            tuples.Add(new List<string>() { "Equals", "Started-by", "Started-by" });
            tuples.Add(new List<string>() { "Equals", "Includes", "During" });
            tuples.Add(new List<string>() { "Equals", "Finished-by", "Finishes" });
            tuples.Add(new List<string>() { "Equals", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Equals", "Met-by", "Met-by" });
            tuples.Add(new List<string>() { "Equals", "After", "After" });
            tuples.Add(new List<string>() { "Started-by", "Before", "Before" });
            tuples.Add(new List<string>() { "Started-by", "Before", "Meets" });
            tuples.Add(new List<string>() { "Started-by", "Before", "Overlaps" });
            tuples.Add(new List<string>() { "Started-by", "Before", "Finished-by" });
            tuples.Add(new List<string>() { "Started-by", "Before", "Includes" });
            tuples.Add(new List<string>() { "Started-by", "Meets", "Overlaps" });
            tuples.Add(new List<string>() { "Started-by", "Meets", "Finished-by" });
            tuples.Add(new List<string>() { "Started-by", "Meets", "Includes" });
            tuples.Add(new List<string>() { "Started-by", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Started-by", "Overlaps", "Finished-by" });
            tuples.Add(new List<string>() { "Started-by", "Overlaps", "Includes" });
            tuples.Add(new List<string>() { "Started-by", "Finished-by", "Includes" });
            tuples.Add(new List<string>() { "Started-by", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Started-by", "Starts", "Starts" });
            tuples.Add(new List<string>() { "Started-by", "Starts", "Equals" });
            tuples.Add(new List<string>() { "Started-by", "Starts", "Started-by" });
            tuples.Add(new List<string>() { "Started-by", "Equals", "Started-by" });
            tuples.Add(new List<string>() { "Started-by", "Started-by", "Started-by" });
            tuples.Add(new List<string>() { "Started-by", "Includes", "During" });
            tuples.Add(new List<string>() { "Started-by", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "Started-by", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Started-by", "Finished-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Started-by", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Started-by", "Met-by", "Met-by" });
            tuples.Add(new List<string>() { "Started-by", "After", "After" });
            tuples.Add(new List<string>() { "During", "Before", "Before" });
            tuples.Add(new List<string>() { "During", "Meets", "Before" });
            tuples.Add(new List<string>() { "During", "Overlaps", "Before" });
            tuples.Add(new List<string>() { "During", "Overlaps", "Meets" });
            tuples.Add(new List<string>() { "During", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "During", "Overlaps", "Starts" });
            tuples.Add(new List<string>() { "During", "Overlaps", "During" });
            tuples.Add(new List<string>() { "During", "Finished-by", "Before" });
            tuples.Add(new List<string>() { "During", "Finished-by", "Meets" });
            tuples.Add(new List<string>() { "During", "Finished-by", "Overlaps" });
            tuples.Add(new List<string>() { "During", "Finished-by", "Starts" });
            tuples.Add(new List<string>() { "During", "Finished-by", "During" });
            tuples.Add(new List<string>() { "During", "Includes", "Before" });
            tuples.Add(new List<string>() { "During", "Includes", "Meets" });
            tuples.Add(new List<string>() { "During", "Includes", "Overlaps" });
            tuples.Add(new List<string>() { "During", "Includes", "Finished-by" });
            tuples.Add(new List<string>() { "During", "Includes", "Includes" });
            tuples.Add(new List<string>() { "During", "Includes", "Starts" });
            tuples.Add(new List<string>() { "During", "Includes", "Equals" });
            tuples.Add(new List<string>() { "During", "Includes", "Started-by" });
            tuples.Add(new List<string>() { "During", "Includes", "During" });
            tuples.Add(new List<string>() { "During", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "During", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "During", "Includes", "Met-by" });
            tuples.Add(new List<string>() { "During", "Includes", "After" });
            tuples.Add(new List<string>() { "During", "Starts", "During" });
            tuples.Add(new List<string>() { "During", "Equals", "During" });
            tuples.Add(new List<string>() { "During", "Started-by", "During" });
            tuples.Add(new List<string>() { "During", "Started-by", "Finishes" });
            tuples.Add(new List<string>() { "During", "Started-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "During", "Started-by", "Met-by" });
            tuples.Add(new List<string>() { "During", "Started-by", "After" });
            tuples.Add(new List<string>() { "During", "Includes", "During" });
            tuples.Add(new List<string>() { "During", "Finished-by", "During" });
            tuples.Add(new List<string>() { "During", "Overlapped-by", "During" });
            tuples.Add(new List<string>() { "During", "Overlapped-by", "Finishes" });
            tuples.Add(new List<string>() { "During", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "During", "Overlapped-by", "Met-by" });
            tuples.Add(new List<string>() { "During", "Overlapped-by", "After" });
            tuples.Add(new List<string>() { "During", "Met-by", "After" });
            tuples.Add(new List<string>() { "During", "After", "After" });
            tuples.Add(new List<string>() { "Finishes", "Before", "Before" });
            tuples.Add(new List<string>() { "Finishes", "Meets", "Meets" });
            tuples.Add(new List<string>() { "Finishes", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Finishes", "Overlaps", "Starts" });
            tuples.Add(new List<string>() { "Finishes", "Overlaps", "During" });
            tuples.Add(new List<string>() { "Finishes", "Finished-by", "Finished-by" });
            tuples.Add(new List<string>() { "Finishes", "Finished-by", "Equals" });
            tuples.Add(new List<string>() { "Finishes", "Finished-by", "Finishes" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "Started-by" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "Met-by" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "After" });
            tuples.Add(new List<string>() { "Finishes", "Starts", "During" });
            tuples.Add(new List<string>() { "Finishes", "Equals", "Finishes" });
            tuples.Add(new List<string>() { "Finishes", "Started-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finishes", "Started-by", "Met-by" });
            tuples.Add(new List<string>() { "Finishes", "Started-by", "After" });
            tuples.Add(new List<string>() { "Finishes", "Includes", "During" });
            tuples.Add(new List<string>() { "Finishes", "Finished-by", "Finishes" });
            tuples.Add(new List<string>() { "Finishes", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Finishes", "Overlapped-by", "Met-by" });
            tuples.Add(new List<string>() { "Finishes", "Overlapped-by", "After" });
            tuples.Add(new List<string>() { "Finishes", "Met-by", "After" });
            tuples.Add(new List<string>() { "Finishes", "After", "After" });
            tuples.Add(new List<string>() { "Overlapped-by", "Before", "Before" });
            tuples.Add(new List<string>() { "Overlapped-by", "Before", "Meets" });
            tuples.Add(new List<string>() { "Overlapped-by", "Before", "Overlaps" });
            tuples.Add(new List<string>() { "Overlapped-by", "Before", "Finished-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Before", "Includes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Meets", "Overlaps" });
            tuples.Add(new List<string>() { "Overlapped-by", "Meets", "Finished-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Meets", "Includes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Overlaps" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Finished-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Includes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Starts" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Equals" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Started-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "During" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Finishes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlaps", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Finished-by", "Includes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Finished-by", "Started-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Finished-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Includes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Started-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Met-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "After" });
            tuples.Add(new List<string>() { "Overlapped-by", "Starts", "During" });
            tuples.Add(new List<string>() { "Overlapped-by", "Starts", "Finishes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Starts", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Equals", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Started-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Started-by", "Met-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Started-by", "After" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "During" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "Overlapped-by", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Finished-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlapped-by", "Overlapped-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlapped-by", "Met-by" });
            tuples.Add(new List<string>() { "Overlapped-by", "Overlapped-by", "After" });
            tuples.Add(new List<string>() { "Overlapped-by", "Met-by", "After" });
            tuples.Add(new List<string>() { "Overlapped-by", "After", "After" });
            tuples.Add(new List<string>() { "Met-by", "Before", "Before" });
            tuples.Add(new List<string>() { "Met-by", "Before", "Meets" });
            tuples.Add(new List<string>() { "Met-by", "Before", "Overlaps" });
            tuples.Add(new List<string>() { "Met-by", "Before", "Finished-by" });
            tuples.Add(new List<string>() { "Met-by", "Before", "Includes" });
            tuples.Add(new List<string>() { "Met-by", "Meets", "Starts" });
            tuples.Add(new List<string>() { "Met-by", "Meets", "Equals" });
            tuples.Add(new List<string>() { "Met-by", "Meets", "Started-by" });
            tuples.Add(new List<string>() { "Met-by", "Overlaps", "During" });
            tuples.Add(new List<string>() { "Met-by", "Overlaps", "Finishes" });
            tuples.Add(new List<string>() { "Met-by", "Overlaps", "Overlapped-by" });
            tuples.Add(new List<string>() { "Met-by", "Finished-by", "Met-by" });
            tuples.Add(new List<string>() { "Met-by", "Includes", "After" });
            tuples.Add(new List<string>() { "Met-by", "Starts", "During" });
            tuples.Add(new List<string>() { "Met-by", "Starts", "Finishes" });
            tuples.Add(new List<string>() { "Met-by", "Starts", "Overlapped-by" });
            tuples.Add(new List<string>() { "Met-by", "Equals", "Met-by" });
            tuples.Add(new List<string>() { "Met-by", "Started-by", "After" });
            tuples.Add(new List<string>() { "Met-by", "Includes", "During" });
            tuples.Add(new List<string>() { "Met-by", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "Met-by", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "Met-by", "Finished-by", "Met-by" });
            tuples.Add(new List<string>() { "Met-by", "Overlapped-by", "After" });
            tuples.Add(new List<string>() { "Met-by", "Met-by", "After" });
            tuples.Add(new List<string>() { "Met-by", "After", "After" });
            tuples.Add(new List<string>() { "After", "Before", "Before" });
            tuples.Add(new List<string>() { "After", "Before", "Meets" });
            tuples.Add(new List<string>() { "After", "Before", "Overlaps" });
            tuples.Add(new List<string>() { "After", "Before", "Finished-by" });
            tuples.Add(new List<string>() { "After", "Before", "Includes" });
            tuples.Add(new List<string>() { "After", "Before", "Starts" });
            tuples.Add(new List<string>() { "After", "Before", "Equals" });
            tuples.Add(new List<string>() { "After", "Before", "Started-by" });
            tuples.Add(new List<string>() { "After", "Before", "During" });
            tuples.Add(new List<string>() { "After", "Before", "Finishes" });
            tuples.Add(new List<string>() { "After", "Before", "Overlapped-by" });
            tuples.Add(new List<string>() { "After", "Before", "Met-by" });
            tuples.Add(new List<string>() { "After", "Before", "After" });
            tuples.Add(new List<string>() { "After", "Meets", "During" });
            tuples.Add(new List<string>() { "After", "Meets", "Finishes" });
            tuples.Add(new List<string>() { "After", "Meets", "Overlapped-by" });
            tuples.Add(new List<string>() { "After", "Meets", "Met-by" });
            tuples.Add(new List<string>() { "After", "Meets", "After" });
            tuples.Add(new List<string>() { "After", "Overlaps", "During" });
            tuples.Add(new List<string>() { "After", "Overlaps", "Finishes" });
            tuples.Add(new List<string>() { "After", "Overlaps", "Overlapped-by" });
            tuples.Add(new List<string>() { "After", "Overlaps", "Met-by" });
            tuples.Add(new List<string>() { "After", "Overlaps", "After" });
            tuples.Add(new List<string>() { "After", "Finished-by", "After" });
            tuples.Add(new List<string>() { "After", "Includes", "After" });
            tuples.Add(new List<string>() { "After", "Starts", "During" });
            tuples.Add(new List<string>() { "After", "Starts", "Finishes" });
            tuples.Add(new List<string>() { "After", "Starts", "Overlapped-by" });
            tuples.Add(new List<string>() { "After", "Starts", "Met-by" });
            tuples.Add(new List<string>() { "After", "Starts", "After" });
            tuples.Add(new List<string>() { "After", "Equals", "After" });
            tuples.Add(new List<string>() { "After", "Started-by", "After" });
            tuples.Add(new List<string>() { "After", "Includes", "During" });
            tuples.Add(new List<string>() { "After", "Includes", "Finishes" });
            tuples.Add(new List<string>() { "After", "Includes", "Overlapped-by" });
            tuples.Add(new List<string>() { "After", "Includes", "Met-by" });
            tuples.Add(new List<string>() { "After", "Includes", "After" });
            tuples.Add(new List<string>() { "After", "Finished-by", "After" });
            tuples.Add(new List<string>() { "After", "Overlapped-by", "After" });
            tuples.Add(new List<string>() { "After", "Met-by", "After" });
            tuples.Add(new List<string>() { "After", "After", "After" });
            #endregion

            constraintType.Tuples = tuples;
            constraintTypes.Add(constraintType);

            return constraintTypes;
        }

        private static void CreateMetricRelations()
        {
            List<BinaryRelation> metricRelations = new List<BinaryRelation>();
            RelationFamily family;

            #region Relations
            // Adding all metric relations to the family
            metricRelations.Add(new MetricRelations.GreaterThan());
            metricRelations.Add(new MetricRelations.AfterN());
            metricRelations.Add(new MetricRelations.LessThan());
            metricRelations.Add(new MetricRelations.BeforeN());
            metricRelations.Add(new MetricRelations.Equals());
            metricRelations.Add(new MetricRelations.EqualsN());
            metricRelations.Add(new MetricRelations.NotEquals());
            metricRelations.Add(new MetricRelations.NotEqualsN());
            metricRelations.Add(new MetricRelations.LessThanN());
            metricRelations.Add(new MetricRelations.GreaterThanN());
            metricRelations.Add(new MetricRelations.LessOrEqualsN());
            metricRelations.Add(new MetricRelations.GreaterOrEqualsN());

            // Adding the metric relation family in the list of families
            family = new RelationFamily(RelationFamilyNames.MetricRelationsName, metricRelations);
            Relations.AddRange(metricRelations);
            RelationFamilies.Add(family);
            family.EqualsRelation = StructuralRelationsManager.GetRelation(family.Name, MetricRelationNames.Equals);
            #endregion

            // Creating the metric relation domain
            MetricDomain = new GKOIntDomain()
            {
                Id = TmsManager.DomainNameMetric,
                Name = TmsManager.DomainNameMetric,
                MinValue = 0,
                MaxValue = StructuralRelationsManager.MaxMetricValue,
                StepWidth = 1
            };
        }

    }
}
