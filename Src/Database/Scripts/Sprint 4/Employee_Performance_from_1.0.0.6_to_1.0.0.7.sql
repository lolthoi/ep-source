
DECLARE @OLD_VERSION VARCHAR(10) = '1.0.0.6'
DECLARE @NEW_VERSION VARCHAR(10) = '1.0.0.7'
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

		IF EXISTS(SELECT 1 From INFORMATION_SCHEMA.COLUMNS Where TABLE_NAME = 'CriteriaTemplate' AND COLUMN_NAME = 'CriteriaTypeStoreId')
		BEGIN
			EXEC sp_rename 'CriteriaTemplate.CriteriaTypeStoreId', 'CriteriaTypeTemplateId' ,'COLUMN'
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