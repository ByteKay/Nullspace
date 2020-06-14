

namespace Nullspace
{
    /// <summary>
    /// 定义触发器抽象基类
    /// </summary>
    public abstract class PathTrigger
    {
        /// <summary>
        /// 触发长度
        /// </summary>
        public float mTriggerLength;

        /// <summary>
        /// 触发响应
        /// </summary>
        /// <param name="handler"></param>

        public abstract void OnTrigger(IPathTrigger handler);

        public abstract void OnDrawGizmos(UnityEngine.Vector3 pos);

    }
}
