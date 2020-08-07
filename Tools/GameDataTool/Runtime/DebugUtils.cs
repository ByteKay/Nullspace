using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nullspace
{
    public class DebugUtils
    {
        public static void Assert(bool isTrue, string message)
        {
            if (!isTrue)
            {
                GameDataManager.Log(message);
            }
            Debug.Assert(isTrue, message);
        }
    }
}
