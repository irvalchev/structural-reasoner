using Gecko.StructuralReasoner.ConstraintRepresentation;
using Gecko.StructuralReasoner.Tms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.Relations
{
    /// <summary>
    /// A class representing a binary relation.
    /// Although it is possible that more or less relation parts are included,
    /// in the constraint graph only two nodes are linked with the relation
    /// </summary>
    public abstract class BinaryRelation
    {
        protected readonly string name = "Not specified!";
        protected readonly string meaning = "Not specified!";
        protected readonly List<Type> signature;

        protected BinaryRelation(string name, string meaning, List<Type> signature)
        {
            this.name = name;
            this.meaning = meaning;
            this.signature = signature;
        }

        /// <summary>
        /// Specifies the unique id of the relation in its calculus.
        /// Do not change this value. It is assigned when the relation family is created!
        /// </summary>
        public int IdInCalculus { get; set; }

        /// <summary>
        /// The human-readable name of the relation
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Human-readable explanation of the relation
        /// </summary>
        public string Meaning
        {
            get
            {
                return meaning;
            }
        }

        /// <summary>
        /// The signature of the relation. It should include only types implementing the interface IRelationPart
        /// </summary>
        public List<Type> Signature
        {
            get
            {
                return signature;
            }
        }

        /// <summary>
        /// The relation family of which this relation is part of
        /// </summary>
        public RelationFamily RelationFamily { get; internal set; }

        /// <summary>
        /// Generates the TMS constraints for a particular configuration constraint in a constraint network edge
        /// <para>Does not handle soft constraints!</para>
        /// </summary>
        /// <param name="constraint">The constraint</param>
        /// <param name="edge">The constraint network edge that holds the configuration constraint</param>
        public abstract List<TmsConstraint> GetTmsConstraints(ConfigurationConstraint constraint, Edge edge);

        /// <summary>
        /// Finds the start node of of a constraint
        /// <para>The start node is X in the relation: XrY</para>
        /// </summary>
        /// <param name="constraint">The constraint specifying the elements of the relation</param>
        /// <returns>The node representing the start element of the relation</returns>
        public abstract Node GetStartNode(ConfigurationConstraint constraint);

        /// <summary>
        /// Finds the end node of of a constraint
        /// <para>The end node is Y in the relation: XrY</para>
        /// </summary>
        /// <param name="constraint">The constraint specifying the elements of the relation</param>
        /// <returns>The node representing the end element of the relation</returns>
        public abstract Node GetEndNode(ConfigurationConstraint constraint);

        /// <summary>
        /// Provides a human readable explanation. A nad B are used as placeholders for the first and second Node relaiton part
        /// </summary>
        /// <param name="constraint">The constraint for which to provide the explanation</param>
        /// <returns></returns>
        public abstract string ToString(ConfigurationConstraint constraint);

    }
}
