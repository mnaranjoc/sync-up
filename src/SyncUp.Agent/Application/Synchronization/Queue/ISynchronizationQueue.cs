using SyncUp.Agent.Application.Synchronization.Queue.Operations;

namespace SyncUp.Agent.Application.Synchronization.Queue
{
    public interface ISynchronizationQueue
    {
        void Queue(IOperation operation);

        IList<IOperation> DequeueAll();
    }
}
