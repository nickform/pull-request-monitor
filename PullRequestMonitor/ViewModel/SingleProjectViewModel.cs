using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class SingleProjectViewModel : IUpdateable
    {
        public SingleProjectViewModel(ActivePullRequestListViewModel unapproved, ActivePullRequestListViewModel approved, CompletedPullRequestListViewModel completed)
        {
            Unapproved = unapproved;
            Approved = approved;
            Completed = completed;
        }

        public ITfProject Model { get; set; }

        public ActivePullRequestListViewModel Unapproved { get; }

        public ActivePullRequestListViewModel Approved { get; }

        public CompletedPullRequestListViewModel Completed { get; }

        public void Update()
        {
            if (Model != null)
            {
                Unapproved.Model = Model.Unapproved;
                Approved.Model = Model.Approved;
                Completed.Model = Model.Completed;
            }

            Unapproved.Update();
            Approved.Update();
            Completed.Update();
        }
    }
}