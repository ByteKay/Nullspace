using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullMesh
{
    public interface INullStream
    {
        int SaveToStream(NullMemoryStream stream);
        bool LoadFromStream(NullMemoryStream stream);
    }
}
