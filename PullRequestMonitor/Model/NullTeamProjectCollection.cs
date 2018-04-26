using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PullRequestMonitor.Model
{
    public class NullTeamProjectCollection : ITfProjectCollection
    {
        public RetrievalStatus ProjectRetrievalStatus => RetrievalStatus.Unstarted;

        public Task RetrieveProjects()
        {
            return Task.Run(() =>
            {
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProjectRetrievalCompleted?.Invoke(this, EventArgs.Empty);
                    });
                }
            });
        }

        public IEnumerable<ITfProject> Projects => Enumerable.Empty<ITfProject>();
        public event EventHandler ProjectRetrievalCompleted;
        public ITfProject GetProject(Guid projectId)
        {
            return null;
        }
    }
}