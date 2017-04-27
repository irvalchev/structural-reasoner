using Gecko.StructuralReasoner.ConstraintRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gecko.StructuralReasoner
{
    public class AttributeRelationPart : IRelationPart
    {
        public AttributeRelationPart(GKOComponent componentVal, GKOAttribute attributeVal)
        {
            this.SetValue(componentVal, attributeVal);
        }

        public GKOComponent Component { get; private set; }
        public GKOAttribute Attribute { get; private set; }

        public void SetValue(GKOComponent componentVal, GKOAttribute attributeVal)
        {

            if (componentVal == null)
            {
                throw new NullReferenceException("The component must be specified!");
            }
            else if (attributeVal == null)
            {
                throw new NullReferenceException("The attribute must be specified!");
            }
            else if (!componentVal.AttributeValues.ContainsKey(attributeVal))
            {
                throw new NullReferenceException("This attribute is not available for the component!");
            }

            this.Component = componentVal;
            this.Attribute = attributeVal;
        }

        public string GetValue()
        {
            return this.Component.AttributeValues[Attribute];
        }

        public Node ToNode()
        {
            return new Node(this.Component, this.Attribute);
        }

        public override bool Equals(System.Object obj)
        {
            AttributeRelationPart part;

            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            part = obj as AttributeRelationPart;
            if ((System.Object)part == null)
            {
                return false;
            }

            return this.Component.Id == part.Component.Id && this.Component.Name == part.Component.Name && this.Attribute.Id == part.Attribute.Id && this.Attribute.Name == part.Attribute.Name;
        }

        public bool Equals(AttributeRelationPart part)
        {
            // If parameter is null return false:
            if ((object)part == null)
            {
                return false;
            }

            return this.Component.Id == part.Component.Id && this.Component.Name == part.Component.Name && this.Attribute.Id == part.Attribute.Id && this.Attribute.Name == part.Attribute.Name;
        }

        // ToDo: Implement
        //public override int GetHashCode()
        //{
        //    return x ^ y;
        //}
    }
}
