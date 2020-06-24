using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class ooce_item
    {
        public ooce_item next;
        public ooce_item prev;
        public ooce_object obj;
        public ooce_node node;
        public ooce_item cnext;
        public ooce_item cprev;

        public ooce_item()
        {

        }

        public void Detach()
        {

        }
        public void Attach(ooce_node nd)
        {

        }
        public void Link(ooce_item i2)
        {

        }
        public void Unlink()
        {

        }
        public ooce_item Split()
        {
            return null;
        }
    }
}
