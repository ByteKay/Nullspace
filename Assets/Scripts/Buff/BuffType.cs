using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fishing
{
    public static class BuffType
    {
        public const int NONE = 0;
        public const int FREEZE_MOVE = 1;
        public const int PAUSE_MOVE = 1 << 1;
        public const int ACCELERATA_MOVE = 1 << 2;
        public const int DECELERATA_MOVE = 1 << 3;
        public const int UNLOCK_MOVE = 1 << 4;

        public static Dictionary<int, int> BLACK_TYPE = new Dictionary<int, int>();
        static BuffType()
        {
            BLACK_TYPE.Add(UNLOCK_MOVE, FREEZE_MOVE | PAUSE_MOVE | ACCELERATA_MOVE | DECELERATA_MOVE);

        }
    }
}
