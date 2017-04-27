using Gecko.StructuralReasoner.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public class AttributeDomainRelationPart : IDomainRelationPart
    {
        public AttributeDomainRelationPart(ComponentFilter filter, GKOAttribute attribute)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("The component filter must be set!");
            }
            if (attribute == null)
            {
                throw new ArgumentNullException("The attribute must be set!");
            }

            this.Filter = filter;
            this.Attribute = attribute;
        }

        public ComponentFilter Filter { get; set; }
        public GKOAttribute Attribute { get; set; }

        public List<IRelationPart> GeneratePossibleRelationParts(List<GKOComponent> componentsList, Log log)
        {
            List<IRelationPart> parts = new List<IRelationPart>();
            List<GKOComponent> matchingComponents = Filter.GetMatchingComponents(componentsList);

            foreach (var component in matchingComponents)
            {
                if (!component.AttributeValues.ContainsKey(Attribute))
                {
                    log.AddItem(LogType.Warning, "A relation part cannot be created. The component '" + component.Name + "' does not have an attribute '" + Attribute.Name + "'");
                }
                else
                {
                    parts.Add(new AttributeRelationPart(component, this.Attribute));
                }
            }

            return parts;
        }
    }
}
