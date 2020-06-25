using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NetworkCommandTypeAttributeAttribute : Attribute
    {
        public NetworkCommandTypeAttributeAttribute(int id, string desc)
        {
            Id = id;
            Description = desc;
        }
        public NetworkCommandTypeAttributeAttribute()
        {

        }
        public int Id { get; set; }

        public string Description { get; set; }

    }
}
