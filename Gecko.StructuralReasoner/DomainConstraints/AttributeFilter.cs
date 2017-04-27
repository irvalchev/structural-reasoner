using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public class AttributeFilter
    {
        public AttributeFilter(GKOAttribute attribute, List<string> allowedValues)
        {
            this.Attribute = attribute;
            this.AllowedValues = allowedValues;
        }

        /// <summary>
        /// The attribute to filter on
        /// </summary>
        public GKOAttribute Attribute { get; set; }

        /// <summary>
        /// The values allowed for the attribute
        /// </summary>
        public List<string> AllowedValues { get; set; }

        /// <summary>
        /// Finds the items matching the filter from a list of possible components
        /// </summary>
        /// <param name="componentsList">The components to filter</param>
        /// <returns>The list of components matching the filter</returns>
        public List<GKOComponent> GetMatchingComponents(List<GKOComponent> componentsList)
        {
            List<GKOComponent> matchingComponents = new List<GKOComponent>();

            if (componentsList != null)
            {
                matchingComponents = componentsList.Where(x =>
                    x.AttributeValues.ContainsKey(Attribute) &&
                    AllowedValues.Contains(x.AttributeValues[Attribute])
                    ).ToList();
            }

            return matchingComponents;
        }

    }
}
