
namespace Nullspace
{
    public interface ISequnceUpdate
    {
        void Update(float deltaTime);  // sequence 执行
        void Next();            // 执行下一个
        void Kill();
        ISequnceUpdate Sibling { get; set; } // for tree
        bool IsPlaying { get; } // 是否正在运行中
    }
}
