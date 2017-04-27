using Gecko.StructuralReasoner.ConstraintRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class ComponentRelationPart : IRelationPart
    {
        public ComponentRelationPart(GKOComponent componentVal)
        {
            this.SetValue(componentVal);
        }
    
        public GKOComponent Component { get; private set; }

        public void SetValue(GKOComponent componentVal)
        {

            if (componentVal == null)
            {
                throw new NullReferenceException("The component must be specified!");
            }

            this.Component = componentVal;
        }

        public string GetValue()
        {
            return this.Component.Active.ToString();
        }

        public Node ToNode()
        {
            return new Node(this.Component);
        }

        public override bool Equals(System.Object obj)
        {
            ComponentRelationPart part;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            part = obj as ComponentRelationPart;
            if ((System.Object)part == null)
            {
                return false;
            }

            return this.Component.Id == part.Component.Id && this.Component.Name == part.Component.Name;
        }

        public bool Equals(ComponentRelationPart part)
        {
            // If parameter is null return false:
            if ((object)part == null)
            {
                return false;
            }

            return this.Component.Id == part.Component.Id && this.Component.Name == part.Component.Name;
        }

        // ToDo: Implement
        //public override int GetHashCode()
        //{
        //    return x ^ y;
        //}
    }
}
