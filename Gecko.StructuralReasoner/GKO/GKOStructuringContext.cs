using Gecko.StructuralReasoner.DomainConstraints;
using Gecko.StructuralReasoner.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class GKOStructuringContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">The unique id of the structuring context</param>
        /// <param name="relationFamilies">The list of relations based on which the components will be structured</param>
        public GKOStructuringContext(string id, List<RelationFamily> relationFamilies)
        {
            if (relationFamilies == null || relationFamilies.Count == 0)
            {
                throw new ArgumentException("The StructuringContext needs at least one structuring relation family!");
            }

            Components = new List<GKOComponent>();
            StructuralConstraints = new List<ConfigurationConstraintTree>();
            IncludedRelationFamilies = relationFamilies;
            this.Id = id;
        }

        public bool Active { get; set; }
        public string Id { get; set; }

        public List<GKOComponent> Components { get; set; }
        public List<RelationFamily> IncludedRelationFamilies { get; private set; }
        public List<ConfigurationConstraintTree> StructuralConstraints { get; private set; }
    }
}
