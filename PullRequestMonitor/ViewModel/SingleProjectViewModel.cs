using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    public sealed class SingleProjectViewModel : IUpdateable
    {
        public SingleProjectViewModel(PullRequestListViewModel unapproved, PullRequestListViewModel approved, PullRequestDescendingListViewModel completed)
        {
            Unapproved = unapproved;
            Approved = approved;
            Completed = completed;
        }

        public ITfProject Model { get; set; }

        public PullRequestListViewModel Unapproved { get; }

        public PullRequestListViewModel Approved { get; }

        public PullRequestDescendingListViewModel Completed { get; }

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