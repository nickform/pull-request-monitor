using System;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace PullRequestMonitor.Model
{
    /// <summary>
    /// Represents a Git repository hosted inside a Team Foundation project.
    /// </summary>
    public interface ITfGitRepository
    {
        /// <summary>
        /// The ID of the repository.
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// The name of the repository.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The Team Foundation project which hosts this repository.
        /// </summary>
        ITfProject Project { get; }
    }

    public class TfGitRepository : ITfGitRepository
    {
        private readonly GitRepository _gitRepository;

        public TfGitRepository(GitRepository gitRepository, ITfProject project)
        {
            _gitRepository = gitRepository;
            Project = project;
        }

        public Guid Id => _gitRepository.Id;
        public string Name => _gitRepository.Name;
        public ITfProject Project { get; }
    }
}