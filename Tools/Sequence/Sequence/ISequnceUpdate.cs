
namespace Nullspace
{
    internal interface ISequnceUpdate
    {
        void Update(float deltaTime);  // sequence 执行
        void Next();            // 执行下一个
        void Kill();
        bool IsPlaying { get; } // 是否正在运行中
    }
}
