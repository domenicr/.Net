DECLARE @BDVersionNumber VARCHAR(50) = (SELECT TOP 1 DBVersion	FROM [NutCache].[Schema_NutCache].[Settings]);
IF @BDVersionNumber = '13.1.13'
BEGIN
    -- Create WebHookPayLoad table
	CREATE TABLE [Schema_NutCache].[WebHookPayLoad](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[ProjectIntegration_ID] INT NOT NULL,
		[Payload] [NVARCHAR] (max) NOT NULL,
		[Status] [INT] NOT NULL,
		[CreationUTCDate] [DATETIME] NOT NULL,
		[Version] [INT] NOT NULL
	CONSTRAINT [PK_WebHookPayLoad] PRIMARY KEY CLUSTERED (ID ASC)
		WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [FileGroup_NutCache]

	ALTER TABLE [Schema_NutCache].[WebHookPayLoad] ADD CONSTRAINT [DF_WebHookPayLoad_CreateDate]  DEFAULT (GETUTCDATE()) FOR [CreationUTCDate]

	ALTER TABLE [Schema_NutCache].[WebHookPayLoad]  WITH CHECK ADD  CONSTRAINT [FK_WebHookPayLoad_ProjectIntegration] FOREIGN KEY([ProjectIntegration_ID])
	REFERENCES [Schema_NutCache].[ProjectIntegration] ([ID])
	ON DELETE CASCADE

	ALTER TABLE [Schema_NutCache].[WebHookPayLoad] CHECK CONSTRAINT [FK_WebHookPayLoad_ProjectIntegration]
	-----------------------------------
	-- Update the DB version
	-----------------------------------
	UPDATE [NutCache].[Schema_NutCache].[Settings]
	SET DBVersion = '13.2.1';
END
GO
DECLARE @BDVersionNumber VARCHAR(50) = (SELECT TOP 1 DBVersion	FROM [NutCache].[Schema_NutCache].[Settings]);

IF @BDVersionNumber = '13.2.1'
BEGIN

    -- Create IntegrationCommit table
	CREATE TABLE [Schema_NutCache].[IntegrationCommit](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[ProjectIntegration_ID] INT NOT NULL,
		[Repository] [NVARCHAR] (max) NOT NULL,
		[URL] [NVARCHAR] (max) NOT NULL,
		[Revision] [NVARCHAR] (400) NOT NULL,
		[Comment] [NVARCHAR] (max) NOT NULL,
		[Author] [NVARCHAR] (max) NOT NULL,
		[AuthorEmail] [NVARCHAR] (max) NOT NULL,
		[AuthorUserName] [NVARCHAR] (max) NOT NULL,
		[CommitUTCDate] [DATETIME] NOT NULL,
		[Version] [INT] NOT NULL
	CONSTRAINT [PK_IntegrationCommit] PRIMARY KEY CLUSTERED (ID ASC)
		WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [FileGroup_NutCache]

	ALTER TABLE [Schema_NutCache].[IntegrationCommit]  WITH CHECK ADD CONSTRAINT [FK_IntegrationCommit_ProjectIntegration] FOREIGN KEY([ProjectIntegration_ID])
	REFERENCES [Schema_NutCache].[ProjectIntegration] ([ID])
	ON DELETE CASCADE

	ALTER TABLE [Schema_NutCache].[IntegrationCommit] CHECK CONSTRAINT [FK_IntegrationCommit_ProjectIntegration]
	
	CREATE UNIQUE INDEX UQ_IntegrationCommit_ProjectIntegrationID_Revision ON [Schema_NutCache].[IntegrationCommit] (ProjectIntegration_ID, Revision)    	
	-----------------------------------
	-- Update the DB version
	-----------------------------------
	UPDATE [NutCache].[Schema_NutCache].[Settings]
	SET DBVersion = '13.2.2';
END
GO

DECLARE @BDVersionNumber VARCHAR(50) = (SELECT TOP 1 DBVersion	FROM [NutCache].[Schema_NutCache].[Settings]);

IF @BDVersionNumber = '13.2.2'
BEGIN
    -- Create IntegrationCommitProjectBoardCard table
	CREATE TABLE [Schema_NutCache].[IntegrationCommitProjectBoardCard](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationCommit_ID] INT NOT NULL,
		[ProjectBoardCard_ID] INT NOT NULL,
		[Version] [INT] NOT NULL
   	CONSTRAINT [PK_IntegrationCommitProjectBoardCard] PRIMARY KEY CLUSTERED (ID ASC)
		WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [FileGroup_NutCache]
	
	ALTER TABLE [Schema_NutCache].[IntegrationCommitProjectBoardCard] WITH CHECK ADD CONSTRAINT [FK_IntegrationCommitProjectBoardCard_IntegrationCommit] FOREIGN KEY([IntegrationCommit_ID])
	REFERENCES [Schema_NutCache].[IntegrationCommit] ([ID])
	ON DELETE CASCADE

	ALTER TABLE [Schema_NutCache].[IntegrationCommitProjectBoardCard] CHECK CONSTRAINT [FK_IntegrationCommitProjectBoardCard_IntegrationCommit]

	ALTER TABLE [Schema_NutCache].[IntegrationCommitProjectBoardCard] WITH CHECK ADD CONSTRAINT [FK_IntegrationCommitProjectBoardCard_ProjectBoardCard] FOREIGN KEY([ProjectBoardCard_ID])
	REFERENCES [Schema_NutCache].[ProjectBoardCard] ([ID])
	ON DELETE CASCADE

	ALTER TABLE [Schema_NutCache].[IntegrationCommitProjectBoardCard] CHECK CONSTRAINT [FK_IntegrationCommitProjectBoardCard_ProjectBoardCard]
	
	CREATE UNIQUE INDEX UQ_IntegrationCommitProjectBoardCard_IntegrationCommit_ProjectBoardCard ON [Schema_NutCache].[IntegrationCommitProjectBoardCard] (IntegrationCommit_ID, ProjectBoardCard_ID)

	-----------------------------------
	-- Update the DB version
	-----------------------------------
	UPDATE [NutCache].[Schema_NutCache].[Settings]
	SET DBVersion = '13.2.3';
END
GO

DECLARE @BDVersionNumber VARCHAR(50) = (SELECT TOP 1 DBVersion	FROM [NutCache].[Schema_NutCache].[Settings]);

IF @BDVersionNumber = '13.2.3'
BEGIN
	-- Create IntegrationCommitFile table
	CREATE TABLE [Schema_NutCache].[IntegrationCommitFile](
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[IntegrationCommit_ID] INT NOT NULL,
		[FileName] [NVARCHAR] (max) NOT NULL,
		[URL] [NVARCHAR] (max) NOT NULL,
		[Action] [INT] NOT NULL,
		[Version] [INT] NOT NULL
	CONSTRAINT [PK_IntegrationCommitFile] PRIMARY KEY CLUSTERED (ID ASC)
		WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [FileGroup_NutCache]

	ALTER TABLE [Schema_NutCache].[IntegrationCommitFile]  WITH CHECK ADD CONSTRAINT [FK_IntegrationCommitFile_IntegrationCommit] FOREIGN KEY([IntegrationCommit_ID])
	REFERENCES [Schema_NutCache].[IntegrationCommit] ([ID])
	ON DELETE CASCADE

	ALTER TABLE [Schema_NutCache].[IntegrationCommitFile] CHECK CONSTRAINT [FK_IntegrationCommitFile_IntegrationCommit]

	-----------------------------------
	-- Update the DB version
	-----------------------------------
	UPDATE [NutCache].[Schema_NutCache].[Settings]
	SET DBVersion = '13.2.4';
END
GO

DECLARE @BDVersionNumber VARCHAR(50) = (SELECT TOP 1 DBVersion	FROM [NutCache].[Schema_NutCache].[Settings]);

IF @BDVersionNumber = '13.2.4'
BEGIN
	-- [Unsubscription]: Make column reason & comments optional
	ALTER TABLE [Schema_NutCache].[Unsubscription] ALTER COLUMN [Reason] INT NULL
	ALTER TABLE [Schema_NutCache].[Unsubscription] ALTER COLUMN [Comment] NVARCHAR(2048) NULL

	-----------------------------------
	-- Update the DB version
	-----------------------------------
	UPDATE [NutCache].[Schema_NutCache].[Settings]
	SET DBVersion = '13.2.5';
END
GO
