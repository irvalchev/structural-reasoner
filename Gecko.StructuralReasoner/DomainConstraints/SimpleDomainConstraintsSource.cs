using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    /// <summary>
    /// Simple domain constraints source. The constraints has to be added in the collection directly.
    /// </summary>
    public class SimpleDomainConstraintsSource : DomainConstraintsSource
    {
        public SimpleDomainConstraintsSource(KnowledgeBase kb)
            : base(kb)
        {
        }

        /// <summary>
        /// Does nothing. Add the constraints directly in the collection
        /// </summary>
        public override void LoadDomainConstraints()
        {
        }
    }
}
