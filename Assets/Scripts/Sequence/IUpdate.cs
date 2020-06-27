
namespace Nullspace
{
    public interface IUpdate
    {
        void Update(float deltaTime);
        bool IsPlaying { get; }
    }
}
