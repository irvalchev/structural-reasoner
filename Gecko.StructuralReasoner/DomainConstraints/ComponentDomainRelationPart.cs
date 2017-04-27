using Gecko.StructuralReasoner.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public class ComponentDomainRelationPart : IDomainRelationPart
    {
        public ComponentDomainRelationPart(ComponentFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("The component filter must be set!");
            }

            this.Filter = filter;
        }

        public ComponentFilter Filter { get; set; }

        public List<IRelationPart> GeneratePossibleRelationParts(List<GKOComponent> componentsList, Log log)
        {
            List<IRelationPart> parts = new List<IRelationPart>();
            List<GKOComponent> matchingComponents = Filter.GetMatchingComponents(componentsList);

            foreach (var component in matchingComponents)
            {
                parts.Add(new ComponentRelationPart(component));
            }

            return parts;
        }
    }
}
