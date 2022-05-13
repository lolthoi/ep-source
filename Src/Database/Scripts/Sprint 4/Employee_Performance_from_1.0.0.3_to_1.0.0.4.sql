
DECLARE @OLD_VERSION VARCHAR(10) = '1.0.0.3'
DECLARE @NEW_VERSION VARCHAR(10) = '1.0.0.4'
DECLARE @DATABASEVERSION VARCHAR(50) = 'DATABASEVERSION'

IF OBJECT_ID(N'dbo.AppSetting', N'U') IS NULL
BEGIN
	PRINT 'Create new table AppSetting'
	CREATE TABLE [dbo].[AppSetting]
	(
		[Id] [nvarchar](50) NOT NULL,
		[Value] [nvarchar](max) NULL,
		[Status] [bit] NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		CONSTRAINT [PK_AppSetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	INSERT INTO AppSetting
		([Id], Value, Status)
	VALUES
		('DATABASEVERSION', @OLD_VERSION, 1)
END
IF (EXISTS(SELECT *
FROM AppSetting
WHERE [Id] = @DATABASEVERSION AND [Value] = @OLD_VERSION))
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

	IF EXISTS (SELECT *
	FROM tempdb..sysobjects
	WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
	CREATE TABLE #tmpErrors
	(
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
		WHERE TABLE_NAME = 'CriteriaTypeStore' AND TABLE_TYPE = 'BASE TABLE')
			BEGIN
				CREATE TABLE [dbo].[CriteriaTypeStore]
				(
					[Id] [uniqueidentifier] NOT NULL,
					[Name] [nvarchar](255) NOT NULL,
					[Description] [nvarchar] (500) NULL,
					[OrderNo] [int] NOT NULL,
					[Rowversion] [timestamp] NOT NULL,
					[CreatedBy] [int] NOT NULL,
					[CreatedDate] [datetime] NOT NULL,
					[ModifiedBy] [int] NULL,
					[ModifiedDate] [datetime] NULL,
					[DeletedBy] [int] NULL,
					[DeletedDate] [datetime] NULL,
					CONSTRAINT [PK_CriteriaTypeStore] PRIMARY KEY CLUSTERED 
					(
						[Id] ASC
					)
					WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
			END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'CriteriaStore' AND TABLE_TYPE = 'BASE TABLE')
		BEGIN
			CREATE TABLE [dbo].[CriteriaStore]
			(
				[Id] [uniqueidentifier] NOT NULL,
				[CriteriaTypeId] [uniqueidentifier] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Description] [nvarchar] (500) NULL,
				[OrderNo] [int] NOT NULL,
				[Rowversion] [timestamp] NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[CreatedDate] [datetime] NOT NULL,
				[ModifiedBy] [int] NULL,
				[ModifiedDate] [datetime] NULL,
				[DeletedBy] [int] NULL,
				[DeletedDate] [datetime] NULL,
				CONSTRAINT [PK_CriteriaStore] PRIMARY KEY CLUSTERED 
				(
					[Id] ASC
				)
				WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'EvaluationTemplate' AND TABLE_TYPE = 'BASE TABLE')
		BEGIN
			CREATE TABLE [dbo].[EvaluationTemplate]
			(
				[Id] [uniqueidentifier] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[PositionId] [int] NOT NULL,
				[Rowversion] [timestamp] NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[CreatedDate] [datetime] NOT NULL,
				[ModifiedBy] [int] NULL,
				[ModifiedDate] [datetime] NULL,
				[DeletedBy] [int] NULL,
				[DeletedDate] [datetime] NULL,
				CONSTRAINT [PK_EvaluationTemplate] PRIMARY KEY CLUSTERED 
					(
						[Id] ASC
					)
					WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			)  ON [PRIMARY]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'CriteriaTypeTemplate' AND TABLE_TYPE = 'BASE TABLE')
		BEGIN
			CREATE TABLE [dbo].[CriteriaTypeTemplate]
			(
				[Id] [uniqueidentifier] NOT NULL,
				[CriteriaTypeStoreId] [uniqueidentifier] NOT NULL,
				[EvaluationTemplateId] [uniqueidentifier] NOT NULL,
				[OrderNo] [int] NOT NULL,
				[Rowversion] [timestamp] NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[CreatedDate] [datetime] NOT NULL,
				[ModifiedBy] [int] NULL,
				[ModifiedDate] [datetime] NULL,
				[DeletedBy] [int] NULL,
				[DeletedDate] [datetime] NULL,
				CONSTRAINT [PK_CriteriaTypeTemplate] PRIMARY KEY CLUSTERED 
					(
						[Id] ASC
					)
					WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			)  ON [PRIMARY]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'CriteriaTemplate' AND TABLE_TYPE = 'BASE TABLE')
		BEGIN
			CREATE TABLE [dbo].[CriteriaTemplate]
			(
				[Id] [uniqueidentifier] NOT NULL,
				[CriteriaTypeStoreId] [uniqueidentifier] NOT NULL,
				[EvaluationTemplateId] [uniqueidentifier] NOT NULL,
				[CriteriaStoreId] [uniqueidentifier] NOT NULL,
				[OrderNo] [int] NOT NULL,
				[Rowversion] [timestamp] NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[CreatedDate] [datetime] NOT NULL,
				[ModifiedBy] [int] NULL,
				[ModifiedDate] [datetime] NULL,
				[DeletedBy] [int] NULL,
				[DeletedDate] [datetime] NULL,
				CONSTRAINT [PK_CriteriaTemplate] PRIMARY KEY CLUSTERED 
					(
						[Id] ASC
					)
					WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			)  ON [PRIMARY]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_NAME = 'QuarterCriteriaTemplate' AND TABLE_TYPE = 'BASE TABLE')
		BEGIN
			CREATE TABLE [dbo].[QuarterCriteriaTemplate]
			(
				[Id] [uniqueidentifier] NOT NULL,
				[Name] [nvarchar](255) NOT NULL,
				[Year] [int] NOT NULL,
				[Quarter] [int] NOT NULL,
				[PositionId] [int] NOT NULL,
				[Rowversion] [timestamp] NOT NULL,
				[CreatedBy] [int] NOT NULL,
				[CreatedDate] [datetime] NOT NULL,
				[ModifiedBy] [int] NULL,
				[ModifiedDate] [datetime] NULL,
				[DeletedBy] [int] NULL,
				[DeletedDate] [datetime] NULL,
				CONSTRAINT [PK_QuarterCriteriaTemplate] PRIMARY KEY CLUSTERED 
					(
						[Id] ASC
					)
					WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]
		END

		IF NOT EXISTS(SELECT 1 From INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME = 'CriteriaType' AND COLUMN_NAME = 'QuarterCriteriaTemplateId')
		BEGIN
			ALTER TABLE [dbo].[CriteriaType] Add [QuarterCriteriaTemplateId] uniqueidentifier null		
		END

		-- TABLE_CONSTRAINTS

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaStore' AND CONSTRAINT_NAME = 'FK_CriteriaStore_CriteriaTypeStore')
		BEGIN
			ALTER TABLE [dbo].[CriteriaStore] WITH CHECK ADD CONSTRAINT [FK_CriteriaStore_CriteriaTypeStore] FOREIGN KEY ([CriteriaTypeId])
			REFERENCES [dbo].[CriteriaTypeStore] ([Id])
		END
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaStore' AND CONSTRAINT_NAME = 'FK_CriteriaStore_CriteriaTypeStore')
		BEGIN
			ALTER TABLE [dbo].[CriteriaStore] CHECK CONSTRAINT [FK_CriteriaStore_CriteriaTypeStore]
		END
		
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaTypeTemplate' AND CONSTRAINT_NAME = 'FK_CriteriaTypeTemplate_EvaluationTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaTypeTemplate] WITH CHECK ADD CONSTRAINT [FK_CriteriaTypeTemplate_EvaluationTemplate] FOREIGN KEY ([EvaluationTemplateId])
			REFERENCES [dbo].[EvaluationTemplate] ([Id])
		END
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaTypeTemplate' AND CONSTRAINT_NAME = 'FK_CriteriaTypeTemplate_EvaluationTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaTypeTemplate] CHECK CONSTRAINT [FK_CriteriaTypeTemplate_EvaluationTemplate]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaTemplate' AND CONSTRAINT_NAME = 'FK_CriteriaTemplate_CriteriaTypeTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaTemplate] WITH CHECK ADD CONSTRAINT [FK_CriteriaTemplate_CriteriaTypeTemplate] FOREIGN KEY ([CriteriaTypeStoreId])
			REFERENCES [dbo].[CriteriaTypeTemplate] ([Id])
		END
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaTemplate' AND CONSTRAINT_NAME = 'FK_CriteriaTemplate_CriteriaTypeTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaTemplate] CHECK CONSTRAINT [FK_CriteriaTemplate_CriteriaTypeTemplate]
		END

		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaType' AND CONSTRAINT_NAME = 'FK_CriteriaType_QuarterCriteriaTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaType] WITH CHECK ADD CONSTRAINT [FK_CriteriaType_QuarterCriteriaTemplate] FOREIGN KEY ([QuarterCriteriaTemplateId])
			REFERENCES [dbo].[QuarterCriteriaTemplate] ([Id])
		END
		IF NOT EXISTS (SELECT 1
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
		Where TABLE_NAME = 'CriteriaType' AND CONSTRAINT_NAME = 'FK_CriteriaType_QuarterCriteriaTemplate')
		BEGIN
			ALTER TABLE [dbo].[CriteriaType] CHECK CONSTRAINT [FK_CriteriaType_QuarterCriteriaTemplate]
		END

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