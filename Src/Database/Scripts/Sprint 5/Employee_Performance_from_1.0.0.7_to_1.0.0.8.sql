
DECLARE @OLD_VERSION VARCHAR(10) = '1.0.0.7'
DECLARE @NEW_VERSION VARCHAR(10) = '1.0.0.8'
DECLARE @DATABASEVERSION VARCHAR(50) = 'DATABASEVERSION'

IF OBJECT_ID(N'dbo.AppSetting', N'U') IS NULL
BEGIN
	PRINT 'Create new table AppSetting'
	CREATE TABLE [dbo].[AppSetting](
	[Id] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Status] [bit] NOT NULL,
	[Rowversion] [timestamp] NOT NULL,
 CONSTRAINT [PK_AppSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	INSERT INTO AppSetting([Id], Value, Status)
	VALUES ('DATABASEVERSION',@OLD_VERSION, 1)
END
IF (EXISTS(SELECT * FROM AppSetting WHERE [Id] = @DATABASEVERSION AND [Value] = @OLD_VERSION))
BEGIN
	PRINT ''
	PRINT '===================================================================='
	PRINT 'The using database is: ' + DB_NAME()
	PRINT 'Updating From '+@OLD_VERSION +' to '+@NEW_VERSION +' Data Migration'
	PRINT '===================================================================='
		
	SET NUMERIC_ROUNDABORT OFF

	SET ANSI_PADDING ON

	SET ANSI_WARNINGS ON

	SET CONCAT_NULL_YIELDS_NULL ON

	SET ARITHABORT ON

	SET QUOTED_IDENTIFIER ON

	SET ANSI_NULLS ON

	IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
	CREATE TABLE #tmpErrors(
		Error int
	);
	BEGIN TRY
			
		BEGIN TRAN
		
		DECLARE @ErrorMessage VARCHAR(MAX)
		DECLARE @ErrorSeverity VARCHAR(MAX)

		/*
		* UPDATE CODE WRITE HERE
		*/
			
		IF NOT EXISTS  (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'ActivityGroupUser' AND TABLE_TYPE = 'BASE TABLE')
			BEGIN
				CREATE TABLE [dbo].[ActivityGroupUser](
					[Id] [uniqueidentifier] NOT NULL,
					[TSActivityGroupId] [uniqueidentifier] NOT NULL,
					[UserId] [int] NOT NULL,
					[Role] [int] NOT NULL,
					[Rowversion] [timestamp] NOT NULL,
					[CreatedBy] [int] NOT NULL,
					[CreatedDate] [datetime] NOT NULL,
					[ModifiedBy] [int] NULL,
					[ModifiedDate] [datetime] NULL,
					[DeletedBy] [int] NULL,
					[DeletedDate] [datetime] NULL,
				 CONSTRAINT [PK_ActivityGroupUser] PRIMARY KEY CLUSTERED 
				(
					[Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
			END
			
		IF NOT EXISTS  (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'TSActivityGroup' AND TABLE_TYPE = 'BASE TABLE')
			BEGIN
				CREATE TABLE [dbo].[TSActivityGroup](
					[Id] [uniqueidentifier] NOT NULL,
					[ProjectId] [int] NULL,
					[Name] [nvarchar](50) NOT NULL,
					[Description] [nvarchar](200) NULL,
					[Rowversion] [timestamp] NOT NULL,
					[CreatedBy] [int] NOT NULL,
					[CreatedDate] [datetime] NOT NULL,
					[ModifiedBy] [int] NULL,
					[ModifiedDate] [datetime] NULL,
					[DeletedBy] [int] NULL,
					[DeletedDate] [datetime] NULL,
				 CONSTRAINT [PK_TSActivityGroup] PRIMARY KEY CLUSTERED 
				(
					[Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
			END
			
		IF NOT EXISTS  (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'TSActivity' AND TABLE_TYPE = 'BASE TABLE')
			BEGIN
				CREATE TABLE [dbo].[TSActivity](
					[Id] [uniqueidentifier] NOT NULL,
					[TSActivityGroupId] [uniqueidentifier] NOT NULL,
					[Name] [nvarchar](50) NOT NULL,
					[Description] [nchar](200) NULL,
					[CreatedBy] [int] NOT NULL,
					[CreatedDate] [datetime] NOT NULL,
					[Rowversion] [timestamp] NOT NULL,
					[ModifiedBy] [int] NULL,
					[ModifiedDate] [datetime] NULL,
					[DeletedBy] [int] NULL,
					[DeletedDate] [datetime] NULL,
				 CONSTRAINT [PK_TSActivity] PRIMARY KEY CLUSTERED 
				(
					[Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
			END
		
		IF NOT EXISTS  (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'TSRecord' AND TABLE_TYPE = 'BASE TABLE')
			BEGIN
				CREATE TABLE [dbo].[TSRecord](
					[Id] [uniqueidentifier] NOT NULL,
					[TSActivityId] [uniqueidentifier] NOT NULL,
					[UserId] [int] NOT NULL,
					[Name] [nvarchar](50) NULL,
					[BacklogId] [nchar](20) NULL,
					[TaskId] [nchar](20) NULL,
					[StartTime] [datetime] NOT NULL,
					[EndTime] [datetime] NOT NULL,
					[Rowversion] [timestamp] NOT NULL,					
					[CreatedBy] [int] NOT NULL,
					[CreatedDate] [datetime] NOT NULL,
					[ModifiedBy] [int] NULL,
					[ModifiedDate] [datetime] NULL,
					[DeletedBy] [int] NULL,
					[DeletedDate] [datetime] NULL,
				 CONSTRAINT [PK_TSRecord] PRIMARY KEY CLUSTERED 
				(
					[Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
			END


		-- TABLE_CONSTRAINTS
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'ActivityGroupUser' AND CONSTRAINT_NAME = 'FK_ActivityGroup_ActivityGroupUser')
		BEGIN
			ALTER TABLE [dbo].[ActivityGroupUser]  WITH CHECK ADD  CONSTRAINT [FK_ActivityGroup_ActivityGroupUser] FOREIGN KEY([TSActivityGroupId])
			REFERENCES [dbo].[TSActivityGroup] ([Id])
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'ActivityGroupUser' AND CONSTRAINT_NAME = 'FK_ActivityGroup_ActivityGroupUser')
		BEGIN
			ALTER TABLE [dbo].[ActivityGroupUser] CHECK CONSTRAINT [FK_ActivityGroup_ActivityGroupUser]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'ActivityGroupUser' AND CONSTRAINT_NAME = 'FK_User_ActivityGroupUser')
		BEGIN
			ALTER TABLE [dbo].[ActivityGroupUser]  WITH CHECK ADD  CONSTRAINT [FK_User_ActivityGroupUser] FOREIGN KEY([UserId])
			REFERENCES [dbo].[User] ([Id])
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'ActivityGroupUser' AND CONSTRAINT_NAME = 'FK_User_ActivityGroupUser')
		BEGIN
			ALTER TABLE [dbo].[ActivityGroupUser] CHECK CONSTRAINT [FK_User_ActivityGroupUser]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSActivity' AND CONSTRAINT_NAME = 'FK_TSActivityGroup_TSActivity')
		BEGIN
			ALTER TABLE [dbo].[TSActivity]  WITH CHECK ADD  CONSTRAINT [FK_TSActivityGroup_TSActivity] FOREIGN KEY([TSActivityGroupId])
			REFERENCES [dbo].[TSActivityGroup] ([Id])
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSActivity' AND CONSTRAINT_NAME = 'FK_TSActivityGroup_TSActivity')
		BEGIN
			ALTER TABLE [dbo].[TSActivity] CHECK CONSTRAINT [FK_TSActivityGroup_TSActivity]
		END
		
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSRecord' AND CONSTRAINT_NAME = 'FK_TSActivity_TSRecord')
		BEGIN
			ALTER TABLE [dbo].[TSRecord]  WITH CHECK ADD  CONSTRAINT [FK_TSActivity_TSRecord] FOREIGN KEY([TSActivityId])
			REFERENCES [dbo].[TSActivity] ([Id])
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSRecord' AND CONSTRAINT_NAME = 'FK_TSActivity_TSRecord')
		BEGIN
			ALTER TABLE [dbo].[TSRecord] CHECK CONSTRAINT [FK_TSActivity_TSRecord]
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSRecord' AND CONSTRAINT_NAME = 'FK_TSUser_TSRecord')
		BEGIN
			ALTER TABLE [dbo].[TSRecord]  WITH CHECK ADD  CONSTRAINT [FK_TSUser_TSRecord] FOREIGN KEY([UserId])
			REFERENCES [dbo].[User] ([Id])
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'TSRecord' AND CONSTRAINT_NAME = 'FK_TSUser_TSRecord')
		BEGIN
			ALTER TABLE [dbo].[TSRecord] CHECK CONSTRAINT [FK_TSUser_TSRecord]
		END
		
		
		-- TABLE_CONSTRAINTS

		-- INIT APPSETTINGS
		IF NOT EXISTS (SELECT 1 FROM AppSetting Where Id = 'LOCKVALUEBYDATE')
			INSERT [dbo].[AppSetting] ([Id], [Value], [Status]) VALUES (N'LOCKVALUEBYDATE', N'0', 1)
		
		IF NOT EXISTS (SELECT 1 FROM AppSetting Where Id = 'ISLOCKTIMESHEET')
			INSERT [dbo].[AppSetting] ([Id], [Value], [Status]) VALUES (N'ISLOCKTIMESHEET', N'false', 1)
		
		IF NOT EXISTS (SELECT 1 FROM AppSetting Where Id = 'LOCKAFTER')
			INSERT [dbo].[AppSetting] ([Id], [Value], [Status]) VALUES (N'LOCKAFTER', N'0', 1)
		-- INIT APPSETINGS
		
		/*
		* UPDATE CODE END HERE
		*/
		
		--Increase database version
		UPDATE dbo.AppSetting SET [Value] =@NEW_VERSION WHERE [Id] =@DATABASEVERSION
		COMMIT
		PRINT '===================================================================='
		PRINT 'The database update succeeded.'
		PRINT '===================================================================='
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
        ROLLBACK TRAN --RollBack in case of Error
		-- Raise ERROR including the details of the exception
		SET @ErrorMessage = ERROR_MESSAGE();
		SET @errorSeverity = ERROR_SEVERITY();
		RAISERROR(@ErrorMessage, @ErrorSeverity, 1)
	END CATCH
END
ELSE
BEGIN
	PRINT ''
	PRINT 'The database has already been updated to '+@NEW_VERSION
END
