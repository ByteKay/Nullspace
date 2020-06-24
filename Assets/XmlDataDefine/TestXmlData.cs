
namespace Nullspace
{
    [XmlData(XmlFileNameDefine.TestPerson)]
    public class TestXmlData : XmlData<TestXmlData>
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
