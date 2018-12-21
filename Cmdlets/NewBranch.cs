using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Cmdlets
{
    [Cmdlet(VerbsCommon.New, "Branch")]
    public class NewBranch : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string ApiUser { get; set; }
        [Parameter(Mandatory = true)]
        public string ApiToken { get; set; }
        [Parameter(Mandatory = true)]
        public string Account { get; set; }
        [Parameter(Mandatory = true)]
        public string Project { get; set; }
        [Parameter(Mandatory = true)]
        public string Repository { get; set; }

        protected override void ProcessRecord()
        {
            var newBranch = CreateBranch();
        }

        public GitRefUpdateResult CreateBranch()
        {
            var credential = new VssBasicCredential(ApiUser, ApiToken);
            var connection = new VssConnection(new Uri($@"https://{ Account }.visualstudio.com"), credential);

            var projectClient = connection.GetClient<ProjectHttpClient>();
            TeamProjectReference project = projectClient.GetProject(Project).Result;

            var gitClient = connection.GetClient<GitHttpClient>();
            GitRepository repository = gitClient.GetRepositoryAsync(project.Name, Repository).Result;
            GitRef sourceRef = gitClient.GetRefsAsync(repository.Id, filter: "refs/heads/master").Result.First();

            return gitClient.UpdateRefsAsync(
                new GitRefUpdate[] { new GitRefUpdate() {
                    OldObjectId = new string('0', 40),
                    NewObjectId = sourceRef.ObjectId,
                    Name = @"refs/heads/cmdlet",
                } },
                repositoryId: repository.Id).Result.First();
        }
    }
}
