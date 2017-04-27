using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    public abstract class DomainConstraintsSource
    {
        protected DomainConstraintsSource(KnowledgeBase kb)
        {
            if (kb == null)
            {
                throw new ArgumentNullException("The knowledge base must be specified!");
            }

            this.KnowledgeBase = kb;
            this.DomainConstraints = new List<DomainConstraint>();
        }

        public List<DomainConstraint> DomainConstraints { get; set; }
        /// <summary>
        /// The knowledge base used for which the domain constraints are specified
        /// </summary>
        protected KnowledgeBase KnowledgeBase { get; private set; }

        public abstract void LoadDomainConstraints();
    }
}
