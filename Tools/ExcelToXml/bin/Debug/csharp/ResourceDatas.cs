using System;
using System.Collections.Generic;
using System.Text;
namespace Nullspace
{
    [XmlData("ResourceArea.xml", "区域")]
    public class ResourceArea : XmlData<ResourceArea>
    {
        public string name {  get; set; }
        public string des {  get; set; }
    }
    [XmlData("ResourceTexture.xml", "纹理")]
    public class ResourceTexture : XmlData<ResourceTexture>
    {
        public string name {  get; set; }
        public string path {  get; set; }
    }
}
