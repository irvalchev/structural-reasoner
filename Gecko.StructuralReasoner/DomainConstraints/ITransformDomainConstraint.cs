using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.DomainConstraints
{
    interface ITransformDomainConstraint
    {
        /// <summary>
        /// Transforms the current domain constraint to the needed output format
        /// </summary>
        /// <returns></returns>
        DomainConstraint TransformConstraint();
    }
}
