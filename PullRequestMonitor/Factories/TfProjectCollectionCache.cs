using System.Collections.Generic;
using System.Threading;
using PullRequestMonitor.Model;

namespace PullRequestMonitor.Factories
{
    public interface ITfProjectCollectionCache
    {
        ITfProjectCollection GetProjectCollection(string account);
    }

    public class TfProjectCollectionCache : ITfProjectCollectionCache
    {
        private readonly ITfProjectCollectionFactory _projectCollectionFactory;
        private readonly Dictionary<string, ITfProjectCollection> _servers;
        private readonly Mutex _mutex;

        public TfProjectCollectionCache(ITfProjectCollectionFactory projectCollectionFactory)
        {
            _projectCollectionFactory = projectCollectionFactory;
            _servers = new Dictionary<string, ITfProjectCollection>();
            _mutex = new Mutex();
        }

        public ITfProjectCollection GetProjectCollection(string account)
        {
            if (account == "") return NullTeamProjectCollection;

            _mutex.WaitOne();
            if (!_servers.ContainsKey(account))
            {
                var serverUrl = ServerUrl.GetServerURL(account);
                _servers[account] = _projectCollectionFactory.Create(serverUrl);
            }
            var tfsServer = _servers[account];
            _mutex.ReleaseMutex();

            return tfsServer;
        }

        private static NullTeamProjectCollection NullTeamProjectCollection = new NullTeamProjectCollection();
    }
}
