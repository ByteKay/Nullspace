
namespace Nullspace
{
    public class CollectionLock
    {
        protected bool mUpdateLocker = false;

        protected void LockUpdate()
        {
            mUpdateLocker = true;
        }

        protected void UnLockUpdate()
        {
            mUpdateLocker = false;
        }

        protected bool IsLockUpdate()
        {
            return mUpdateLocker;
        }
    }
}
