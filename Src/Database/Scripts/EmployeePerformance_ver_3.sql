Use [master]
GO

/****** Object:  Database [EmployeePerformance] Script Date: 09/05/2021 3:19 PM ******/

IF NOT EXISTS (Select 1
From sys.databases
WHERE name = N'EmployeePerformance')
CREATE DATABASE [EmployeePerformance]
GO

USE [EmployeePerformance]
GO

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Position' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[Position]
	(
		[Id] [int] IDENTITY(1, 1) NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_Position] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'User' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[User]
	(
		[Id] [int] IDENTITY(1, 1) NOT NULL,
		[Email] [nvarchar](75) NOT NULL,
		[FirstName] [nvarchar](50) NOT NULL,
		[LastName] [nvarchar](50) NOT NULL,
		[PositionId] [int] NOT NULL,
		[Sex] [int] NULL,
		[DoB] [datetime] NULL,
		[PhoneNo] [nvarchar](10) NULL,
		[RoleId] [int] NOT NULL,
		[PasswordHash] [nvarchar](500) NOT NULL,
		[PasswordSalt] [nvarchar](50) NOT NULL,
		[PasswordResetCode] [nvarchar] (500) NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'ProjectUser' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[ProjectUser]
	(
		[Id] [uniqueidentifier] NOT NULL,
		[ProjectId] [int] NOT NULL,
		[UserId] [int] NOT NULL,
		[ProjectRoleId] [int] NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_ProjectUser] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Project' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[Project]
	(
		[Id] [int] IDENTITY(1, 1) NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[Description] [nvarchar] (500) NULL,
		[Status] [int] NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'CriteriaType' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[CriteriaType]
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
		CONSTRAINT [PK_CriteriaType] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Criteria' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[Criteria]
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
		CONSTRAINT [PK_Criteria] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'CriteriaQuarterEvaluation' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[CriteriaQuarterEvaluation]
	(
		[Id] [uniqueidentifier] NOT NULL,
		[QuarterEvaluationId] [uniqueidentifier] NOT NULL,
		[CriteriaId] [uniqueidentifier] NOT NULL,
		[Point] [bigint] NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		CONSTRAINT [PK_CriteriaQuarterEvaluation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'CriteriaTypeQuarterEvaluation' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[CriteriaTypeQuarterEvaluation]
	(
		[Id] [uniqueidentifier] NOT NULL,
		[QuarterEvaluationId] [uniqueidentifier] NOT NULL,
		[CriteriaTypeId] [uniqueidentifier] NOT NULL,
		[PointAverage] [float] NOT NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		CONSTRAINT [PK_CriteriaTypeQuarterEvaluation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'QuarterEvaluation' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[QuarterEvaluation]
	(
		[Id] [uniqueidentifier] NOT NULL,
		[Year] [int] NOT NULL,
		[Quarter] [int] NOT NULL,
		[UserId] [int] NOT NULL,
		[PositionId] [int] NOT NULL,
		[ProjectId] [int] NOT NULL,
		ProjectLeaderId [int] NOT NULL,
		[PointAverage] [float] NOT NULL,
		[NoteByLeader] [nvarchar] (500) NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_QuarterEvaluation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'UserQuarterEvaluation' AND TABLE_TYPE = 'BASE TABLE')
BEGIN
	CREATE TABLE [dbo].[UserQuarterEvaluation]
	(
		[Id] [uniqueidentifier] NOT NULL,
		[QuarterEvaluationId] [uniqueidentifier] NOT NULL UNIQUE FOREIGN KEY REFERENCES [dbo].[QuarterEvaluation](Id),
		[NoteGoodThing] [nvarchar] (500) NULL,
		[NoteBadThing] [nvarchar] (500) NULL,
		[NoteOther] [nvarchar] (500) NULL,
		[Rowversion] [timestamp] NOT NULL,
		[CreatedBy] [int] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ModifiedBy] [int] NULL,
		[ModifiedDate] [datetime] NULL,
		[DeletedBy] [int] NULL,
		[DeletedDate] [datetime] NULL,
		CONSTRAINT [PK_UserQuarterEvaluation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

-- TABLE_CONSTRAINTS

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'User' AND CONSTRAINT_NAME = 'FK_User_Position')
BEGIN
	ALTER TABLE [dbo].[User] WITH CHECK ADD CONSTRAINT [FK_User_Position] FOREIGN KEY ([PositionId])
    REFERENCES [dbo].[Position] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'User' AND CONSTRAINT_NAME = 'FK_User_Position')
BEGIN
	ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Position]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'ProjectUser' AND CONSTRAINT_NAME = 'FK_ProjectUser_User')
BEGIN
	ALTER TABLE [dbo].[ProjectUser] WITH CHECK ADD CONSTRAINT [FK_ProjectUser_User] FOREIGN KEY ([UserId])
    REFERENCES [dbo].[User] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'ProjectUser' AND CONSTRAINT_NAME = 'FK_ProjectUser_User')
BEGIN
	ALTER TABLE [dbo].[ProjectUser] CHECK CONSTRAINT [FK_ProjectUser_User]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'ProjectUser' AND CONSTRAINT_NAME = 'FK_ProjectUser_Project')
BEGIN
	ALTER TABLE [dbo].[ProjectUser] WITH CHECK ADD CONSTRAINT [FK_ProjectUser_Project] FOREIGN KEY ([ProjectId])
    REFERENCES [dbo].[Project] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'ProjectUser' AND CONSTRAINT_NAME = 'FK_ProjectUser_Project')
BEGIN
	ALTER TABLE [dbo].[ProjectUser] CHECK CONSTRAINT [FK_ProjectUser_Project]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'Criteria' AND CONSTRAINT_NAME = 'FK_Criteria_CriteriaType')
BEGIN
	ALTER TABLE [dbo].[Criteria] WITH CHECK ADD CONSTRAINT [FK_Criteria_CriteriaType] FOREIGN KEY ([CriteriaTypeId])
    REFERENCES [dbo].[CriteriaType] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'Criteria' AND CONSTRAINT_NAME = 'FK_Criteria_CriteriaType')
BEGIN
	ALTER TABLE [dbo].[Criteria] CHECK CONSTRAINT [FK_Criteria_CriteriaType]
END


IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaQuarterEvaluation_Criteria')
BEGIN
	ALTER TABLE [dbo].[CriteriaQuarterEvaluation] WITH CHECK ADD CONSTRAINT [FK_CriteriaQuarterEvaluation_Criteria] FOREIGN KEY ([CriteriaId])
    REFERENCES [dbo].[Criteria] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaQuarterEvaluation_Criteria')
BEGIN
	ALTER TABLE [dbo].[CriteriaQuarterEvaluation] CHECK CONSTRAINT [FK_CriteriaQuarterEvaluation_Criteria]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaQuarterEvaluation_QuarterEvaluation')
BEGIN
	ALTER TABLE [dbo].[CriteriaQuarterEvaluation] WITH CHECK ADD CONSTRAINT [FK_CriteriaQuarterEvaluation_QuarterEvaluation] FOREIGN KEY ([QuarterEvaluationId])
    REFERENCES [dbo].[QuarterEvaluation] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaQuarterEvaluation_QuarterEvaluation')
BEGIN
	ALTER TABLE [dbo].[CriteriaQuarterEvaluation] CHECK CONSTRAINT [FK_CriteriaQuarterEvaluation_QuarterEvaluation]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaTypeQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaTypeQuarterEvaluation_CriteriaType')
BEGIN
	ALTER TABLE [dbo].[CriteriaTypeQuarterEvaluation] WITH CHECK ADD CONSTRAINT [FK_CriteriaTypeQuarterEvaluation_CriteriaType] FOREIGN KEY ([CriteriaTypeId])
    REFERENCES [dbo].[CriteriaType] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaTypeQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaTypeQuarterEvaluation_CriteriaType')
BEGIN
	ALTER TABLE [dbo].[CriteriaTypeQuarterEvaluation] CHECK CONSTRAINT [FK_CriteriaTypeQuarterEvaluation_CriteriaType]
END

IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaTypeQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaTypeQuarterEvaluation_QuarterEvaluation')
BEGIN
	ALTER TABLE [dbo].[CriteriaTypeQuarterEvaluation] WITH CHECK ADD CONSTRAINT [FK_CriteriaTypeQuarterEvaluation_QuarterEvaluation] FOREIGN KEY ([QuarterEvaluationId])
    REFERENCES [dbo].[QuarterEvaluation] ([Id])
END
IF NOT EXISTS (SELECT 1
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
Where TABLE_NAME = 'CriteriaTypeQuarterEvaluation' AND CONSTRAINT_NAME = 'FK_CriteriaTypeQuarterEvaluation_QuarterEvaluation')
BEGIN
	ALTER TABLE [dbo].[CriteriaTypeQuarterEvaluation] CHECK CONSTRAINT [FK_CriteriaTypeQuarterEvaluation_QuarterEvaluation]
END
