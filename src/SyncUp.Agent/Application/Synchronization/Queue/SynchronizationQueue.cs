using SyncUp.Agent.Application.Synchronization.Queue.Operations;

namespace SyncUp.Agent.Application.Synchronization.Queue
{
    public class SynchronizationQueue : ISynchronizationQueue
    {
        private readonly Queue<IOperation> _operations = new Queue<IOperation>();

        public void Queue(IOperation operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            _operations.Enqueue(operation);
        }

        public IList<IOperation> DequeueAll()
        {
            var operations = new List<IOperation>();

            while (_operations.TryDequeue(out var operation))
            {
                operations.Add(operation);
            }

            return operations;
        }
    }
}
