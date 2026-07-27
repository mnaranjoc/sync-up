using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;

namespace SyncUp.Agent.Application.Synchronization.Queue
{
    public interface ISynchronizationQueue
    {
        void Queue(IFileEvent fileEvent);

        IList<IFileEvent> DequeueAll();

        bool IsQueueEmpty();
    }
}
