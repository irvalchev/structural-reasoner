using Gecko.StructuralReasoner.Logging;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public enum DomainConstraintCombinationType
    {
        And = 1,
        Or = 2,
        ExactlyOne = 3
    }

    public class DomainConstraintCombination : DomainConstraint
    {
        /// <summary>
        /// Creates a new DomainConstraintCombination
        /// </summary>
        /// <param name="name">The unique name of the constraint</param>
        /// <param name="constraints">The domain constraints for which is the combination.</param>
        /// <param name="type">The combination type</param>
        /// <param name="canBeViolated"></param>
        public DomainConstraintCombination(string name, List<DomainConstraint> constraints, DomainConstraintCombinationType type, bool canBeViolated)
            : base(name, canBeViolated)
        {

            if (constraints == null || constraints.Count < 2)
            {
                throw new ArgumentException("Domain constraints combination requires at least two constraints to be set!");
            }

            this.Type = type;
            this.DomainConstraints = new List<DomainConstraint>();

            foreach (var constraint in constraints)
            {
                DomainConstraint currConstraint = null;

                if (constraint is ITransformDomainConstraint)
                {
                    // The ITransformDomainConstraint has to be translated to normal or combination domain constraint
                    currConstraint = (constraint as ITransformDomainConstraint).TransformConstraint();
                }
                else
                {
                    currConstraint = constraint;
                }

                // All of the included constraints should use the same relation family
                if (DomainConstraints.Count > 0 && currConstraint.RelationFamily != RelationFamily)
                {
                    throw new ApplicationException("The combination constraint contains constraints with different relation families.");
                }

                this.DomainConstraints.Add(currConstraint);
            }
        }

        /// <summary>
        /// The domain constraints participating in the combination.
        /// This collection should not include transformation domain constraints (e.g. IA to Metric domain constraints)
        /// <para>Note: Assign these only in the constructor. The constructor allows for transformation constraints</para>
        /// </summary>
        public List<DomainConstraint> DomainConstraints { get; private set; }

        /// <summary>
        /// The combination type. This specifies which of the included domain constraints has to be satisfied, e.g. all of them, only one of them, exactly one...
        /// </summary>
        public DomainConstraintCombinationType Type { get; set; }

        /// <summary>
        /// Should not be used!
        /// </summary>
        /// <param name="componentsList"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal override List<ConfigurationConstraint> GenerateConfigurationConstraints(List<GKOComponent> componentsList, Log log)
        {
            throw new NotImplementedException();
        }

    }
}
