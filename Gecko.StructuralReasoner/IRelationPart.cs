using Gecko.StructuralReasoner.ConstraintRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public interface IRelationPart
    {
        /// <summary>
        /// Gets the value of the relation component
        /// </summary>
        /// <returns>A string representation of the relation component's value</returns>
        string GetValue();

        /// <summary>
        /// Converts the relation part to Node. If this is not possible returns null
        /// </summary>
        /// <returns></returns>
        Node ToNode();
    }
}
