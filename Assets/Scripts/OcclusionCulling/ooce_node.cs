using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class ooce_node
    {
        public int item_count;
        public int level;
        public ooce_item head, tail;
        public bbox b;
        public ooce_node left;
        public ooce_node right;
        public ooce_node parent;
        public char split_axis;
        public char visible;
        private float split_value;
        private static int double_counter;


        public ooce_node()
        {

        }

        public void AddObject(ooce_object obj)
        {

        }
        public void Distribute(int max_level, int max_items)
        {

        }
        public void FullDistribute(int max_level, int max_items)
        {

        }
        public void DeleteItems()
        {

        }

        private void Merge(int max_items)
        {

        }

        private void Split()
        {

        }
    }
}
