        public bool GitHubPayLoadToIntegrationCommit(WebHookPayLoad webHookPayLoad) {
            const string GIT_HUB_COMMITS = "commits";
            const string GIT_HUB_REPO_NAME = "repository.name";
            const string GIT_HUB_COMMIT_URL = "url";
            const string GIT_HUB_COMMIT_ID = "id";
            const string GIT_HUB_COMMIT_MESSAGE = "message";
            const string GIT_HUB_COMMIT_AUTH_NAME = "author.name";
            const string GIT_HUB_COMMIT_AUTH_EMAIL = "author.email";
            const string GIT_HUB_COMMIT_AUTH_USERNAME = "author.username";
            const string GIT_HUB_COMMIT_TIMESTAMP = "timestamp";

            const string GIT_HUB_COMMIT_ADDED_FILES = "added";
            const string GIT_HUB_COMMIT_REMOVED_FILES = "removed";
            const string GIT_HUB_COMMIT_MODIFIED_FILES = "modified";

            try {
                var jsonPayload = (JObject)JsonConvert.DeserializeObject(webHookPayLoad.Payload, new JsonSerializerSettings { DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset });
                var commitToken = jsonPayload.SelectToken(GIT_HUB_COMMITS, false);

                if (commitToken != null && commitToken.Type == JTokenType.Array && commitToken.HasValues) {
                    var commits = jsonPayload.SelectToken(GIT_HUB_COMMITS, false).Children();

                    foreach (var commit in commits) {
                        var integrationCommit = new IntegrationCommit {
                            ProjectIntegration = webHookPayLoad.ProjectIntegration,
                            Repository = jsonPayload.SelectToken(GIT_HUB_REPO_NAME, false)?.Value<string>() ?? "",
                            URL = commit.SelectToken(GIT_HUB_COMMIT_URL, false)?.Value<string>() ?? "",
                            Revision = commit.SelectToken(GIT_HUB_COMMIT_ID, true).Value<string>(),
                            Comment = commit.SelectToken(GIT_HUB_COMMIT_MESSAGE, false)?.Value<string>() ?? "",
                            Author = commit.SelectToken(GIT_HUB_COMMIT_AUTH_NAME, false)?.Value<string>() ?? "",
                            AuthorEmail = commit.SelectToken(GIT_HUB_COMMIT_AUTH_EMAIL, false)?.Value<string>() ?? "",
                            AuthorUserName = commit.SelectToken(GIT_HUB_COMMIT_AUTH_USERNAME, false)?.Value<string>() ?? "",
                            CommitUTCDate = commit.SelectToken(GIT_HUB_COMMIT_TIMESTAMP, true).Value<DateTimeOffset>().UtcDateTime
                        };

                        AppendIntegrationCommitFileList(integrationCommit, commit.SelectToken(GIT_HUB_COMMIT_ADDED_FILES, false), IntegrationCommitAction.Added);
                        AppendIntegrationCommitFileList(integrationCommit, commit.SelectToken(GIT_HUB_COMMIT_REMOVED_FILES, false), IntegrationCommitAction.Removed);
                        AppendIntegrationCommitFileList(integrationCommit, commit.SelectToken(GIT_HUB_COMMIT_MODIFIED_FILES, false), IntegrationCommitAction.Modified);

                        if (!String.IsNullOrWhiteSpace(integrationCommit.Comment)) {
                            var projectBoardCards = FindProjectBoardCards(webHookPayLoad.ProjectIntegration, integrationCommit.Comment);

                            foreach (var projectBoardCard in projectBoardCards) {
                                integrationCommit.IntegrationCommitProjectBoardCards.Add(new IntegrationCommitProjectBoardCard {IntegrationCommit = integrationCommit, ProjectBoardCard = projectBoardCard});

                                // Add an entry in the project board activity.
                                ServiceLayerFactory.ProjectBoardActivityManager.InsertGitHubFileCommitToBoardCardActivity(projectBoardCard, integrationCommit);
                                TransactionActions.AddAfterCommitAction(() => {
                                    SignalREndPointFactory.ProjectBoardCard.ProjectBoardCardIntegrationCommitCreated(projectBoardCard.ProjectBoardList.ProjectBoard.ID, projectBoardCard.ProjectBoardList.ID, projectBoardCard.ID);
                                });
                            }
                        }
                        RepositoryFactory.AddToContext(integrationCommit);
                    }
                }
                return true;
            }
            catch (Exception ex) {
                ErrorSignaler.LogException(ex);
                Messages.Add(new MessageInfo(MessageType.Error, "Unable to translate webhook payload."));
            }

            return false;
        }

        protected virtual ICollection<ProjectBoardCard> FindProjectBoardCards(ProjectIntegration projectIntegration, string searchString) {
            const string STORY_REG_EX = "\\bs[0-9]+\\b";

            var storyIDs = new List<int>();
            var storyMatches = Regex.Matches(searchString, STORY_REG_EX, RegexOptions.IgnoreCase);
            foreach (Match storyMatch in storyMatches) {
                storyIDs.Add(Int32.Parse(storyMatch.Value.Substring(1)));
            }

            return RepositoryFactory.DatabaseContext.ProjectBoardCard.Include(x => x.ProjectBoardList.ProjectBoard)
                .Where(x => x.ProjectBoardList.ProjectBoard.ProjectID == projectIntegration.ProjectID && storyIDs.Contains(x.SequenceNumber))
                .Include(x => x.ProjectBoardList)
                .Include(x => x.ProjectBoardList.ProjectBoard)
                .ToList();
        }

        private void AppendIntegrationCommitFileList(IntegrationCommit integrationCommit, JToken fileListToken, IntegrationCommitAction integrationCommitAction) {
            const string GIT_HUB_FILE_URL_LINK_DELIMITER = "#diff-";

            if (fileListToken != null && fileListToken.Type == JTokenType.Array && fileListToken.HasValues){
                foreach (JToken commitFile in fileListToken.Children()){
                    var fileName = commitFile.Value<string>();

                    if (!String.IsNullOrWhiteSpace(fileName)){
                        var fileURL = "";

                        if (!String.IsNullOrWhiteSpace(integrationCommit.URL)){
                            using (MD5 md5 = MD5.Create()){
                                byte[] hash = md5.ComputeHash(fileName.ToUtf8Bytes());
                                fileURL = integrationCommit.URL + GIT_HUB_FILE_URL_LINK_DELIMITER + ToHex(hash, false);
                            }
                        }

                        integrationCommit.IntegrationCommitFiles.Add(
                            new IntegrationCommitFile{
                                IntegrationCommit = integrationCommit,
                                FileName = fileName,
                                URL = fileURL,
                                Action = integrationCommitAction
                            }
                        );
                    }
                }
            }
        }

        private string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
