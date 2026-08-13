using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;

namespace SyncUp.Agent.Application.Synchronization.Queue
{
    public interface ISynchronizationQueue
    {
        void Queue(IFileEvent fileEvent);

        void EnqueueAll(List<IFileEvent> fileEvents);

        IList<IFileEvent> DequeueAll();

        bool IsQueueEmpty();
    }
}
