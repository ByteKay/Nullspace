using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshFile
{
    public interface IStream
    {
        int SaveToStream(SimpleMemoryStream stream);
        bool LoadFromStream(SimpleMemoryStream stream);
    }
}
