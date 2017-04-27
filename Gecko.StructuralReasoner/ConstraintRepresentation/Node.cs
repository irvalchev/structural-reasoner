using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner.ConstraintRepresentation
{
    public class Node : IComparable
    {
        public Node(GKOComponent component)
            : this(component, null)
        {
        }

        public Node(GKOComponent component, GKOAttribute attribute)
        {
            if (component == null)
            {
                throw new ArgumentNullException("The component must be set!");
            }
            if (attribute != null && !component.AttributeValues.ContainsKey(attribute))
            {
                throw new InvalidOperationException("The component does not have this attribute!");
            }

            this.Component = component;
            this.Attribute = attribute;
        }

        public GKOComponent Component { get; private set; }
        public GKOAttribute Attribute { get; private set; }

        /// <summary>
        /// Gets the component's attribute
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            if (this.Attribute == null)
            {
                throw new InvalidOperationException("There is no attribute associated with the node!");
            }

            return this.Component.AttributeValues[Attribute];
        }

        /// <summary>
        /// Sets the component's attribute
        /// </summary>
        public void SetValue(string newValue)
        {
            if (this.Attribute == null)
            {
                throw new InvalidOperationException("There is no attribute associated with the node!");
            }

            this.Component.AttributeValues[Attribute] = newValue;
        }

        public override bool Equals(System.Object obj)
        {
            Node compared;

            if (obj == null)
            {
                return false;
            }

            compared = obj as Node;
            if ((System.Object)compared == null)
            {
                return false;
            }

            return this.Component == compared.Component && this.Attribute == compared.Attribute;
        }

        public bool Equals(Node compared)
        {
            if (compared == null)
            {
                return false;
            }

            return this.Component == compared.Component && this.Attribute == compared.Attribute;
        }

        public override int GetHashCode()
        {
            // Overflow is allowed
            unchecked
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + this.Component.GetHashCode();
                if (this.Attribute != null)
                {
                    hash = hash * 23 + this.Attribute.GetHashCode();
                }
                return hash;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            return -1;
        }
    }
}
