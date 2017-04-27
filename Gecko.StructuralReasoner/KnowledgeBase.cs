using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public abstract class KnowledgeBase
    {
        protected KnowledgeBase()
        {
            ComponentTypes = new List<GKOComponentType>();
            Components = new List<GKOComponent>();
            Attributes = new List<GKOAttribute>();
            Domains = new List<GKODomain>();
            IntDomains = new List<GKOIntDomain>();
            ConstraintTypes = new List<GKOConstraintType>();
            StructuringContexts = new List<GKOStructuringContext>();
        }

        public List<GKOComponentType> ComponentTypes { get; set; }
        public List<GKOComponent> Components { get; set; }
        public List<GKOAttribute> Attributes { get; set; }
        public List<GKODomain> Domains { get; set; }
        public List<GKOIntDomain> IntDomains { get; set; }
        public List<GKOConstraintType> ConstraintTypes { get; set; }

        public List<GKOStructuringContext> StructuringContexts { get; set; }
    }
}
