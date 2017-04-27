using Gecko.StructuralReasoner.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public interface IDomainRelationPart
    {
        /// <summary>
        /// Generates the possible configuration relation parts for a given list of components based on the current domain relation part
        /// </summary>
        /// <param name="componentsList">The components list</param>
        /// <returns>The list of possible configuration relation parts</returns>
        List<IRelationPart> GeneratePossibleRelationParts(List<GKOComponent> componentsList, Log log);
    }
}
