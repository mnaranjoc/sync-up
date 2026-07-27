using SyncUp.Agent.Application.Synchronization.Queue.FileEvent;

namespace SyncUp.Agent.Application.Synchronization.Queue
{
    public class SynchronizationQueue : ISynchronizationQueue
    {
        private readonly Queue<IFileEvent> _fileEvents = new();

        public void Queue(IFileEvent fileEvent)
        {
            ArgumentNullException.ThrowIfNull(fileEvent);

            _fileEvents.Enqueue(fileEvent);
        }

        public IList<IFileEvent> DequeueAll()
        {
            var fileEvents = new List<IFileEvent>();

            while (_fileEvents.TryDequeue(out var fileEvent))
            {
                fileEvents.Add(fileEvent);
            }

            return fileEvents;
        }

        public bool IsQueueEmpty()
        {
            return _fileEvents.Count == 0;
        }
    }
}
