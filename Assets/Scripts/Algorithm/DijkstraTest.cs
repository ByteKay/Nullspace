
using System.Collections.Generic;


namespace Nullspace
{
    /// <summary>
    /// http://v.youku.com/v_show/id_XMjQyOTY1NDQw.html 测试用例
    /// 
    /// 另外，周培德 计算几何 多边形内最短 路径算法
    /// </summary>
    public class DijkstraTest
    {
        public static void Test()
        {
            Dijkstra<string> test = new Dijkstra<string>();
            test.AddConnection("A", "B", 20, false);
            test.AddConnection("A", "G", 90, 20);
            test.AddConnection("A", "D", 80, false);

            test.AddConnection("B", "F", 10, false);

            test.AddConnection("C", "D", 10, true);
            test.AddConnection("C", "H", 20, false);
            test.AddConnection("C", "F", 50, 10);

            test.AddConnection("D", "G", 20, false);

            test.AddConnection("E", "B", 50, false);
            test.AddConnection("E", "G", 30, false);

            test.AddConnection("F", "D", 40, false);

            List<string> path = new List<string>();
            test.GetShortestPath(path, "A", "H", float.MaxValue);
            test.PrintCostMatrix();
            DebugUtils.Info("DijkstraTest", string.Concat(path.ToArray()));
        }
    }

}
