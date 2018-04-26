using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface ITfProjectCollectionFactory
    {
        ITfProjectCollection Create(string uri);
        void Release(ITfProjectCollection projectCollection);
    }
}