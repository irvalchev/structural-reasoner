using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public class ComponentFilter
    {
        public ComponentFilter()
        {
            this.AttributeFilters = new List<AttributeFilter>();
        }

        /// <summary>
        /// The component type of the components that will be included by the filter.
        /// Leave null for no filtering on a component type level
        /// </summary>
        public GKOComponentType ComponentTypeFilter { get; set; }

        /// <summary>
        /// The component that will be included by the filter.
        /// Leave null for no filtering on a component level
        /// </summary>
        public GKOComponent Filter { get; set; }

        /// <summary>
        /// Specifies if the component is matched to the same component as the domain relation part with the specified number
        /// <para>It is a 1-based index</para>
        /// <para>Note: This does not effect the generation of matching components</para>
        /// </summary>
        public int? SameAsDomainRelationPartNr { get; set; }

        /// <summary>
        /// Attribute filters
        /// </summary>
        public List<AttributeFilter> AttributeFilters { get; set; }

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
                        (ComponentTypeFilter == null || x.Type == ComponentTypeFilter) &&
                        (Filter == null || x == Filter)
                    ).ToList();

                // Filter on the attributes after the initial one
                foreach (var filter in AttributeFilters)
                {
                    matchingComponents = filter.GetMatchingComponents(matchingComponents);
                }
            }

            return matchingComponents;
        }
    }
}
