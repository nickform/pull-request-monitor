using PullRequestMonitor.Model;

namespace PullRequestMonitor.ViewModel
{
    internal sealed class SingleProjectViewModel : IUpdateable
    {
        public SingleProjectViewModel(PullRequestListViewModel unapproved, PullRequestListViewModel approved)
        {
            Unapproved = unapproved;
            Approved = approved;
        }

        public ITfProject Model { get; set; }

        public PullRequestListViewModel Unapproved { get; }

        public PullRequestListViewModel Approved { get; }

        public void Update()
        {
            if (Model != null)
            {
                Unapproved.Model = Model.Unapproved;
                Approved.Model = Model.Approved;
            }

            Unapproved.Update();
            Approved.Update();
        }
    }
}