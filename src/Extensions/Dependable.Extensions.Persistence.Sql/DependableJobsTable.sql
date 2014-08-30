BEGIN TRAN

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DependableJobs]') AND type in (N'U'))
BEGIN
	PRINT 'Table does not exist. Creating a new one.'

	CREATE TABLE [dbo].[DependableJobs](
		[Id] [uniqueidentifier] NOT NULL,
		[Type] [nvarchar](max) NOT NULL,
		[Method] [nvarchar](max) NOT NULL,
		[Arguments] [nvarchar](max) NULL,
		[CreatedOn] [datetime] NOT NULL,
		[RootId] [uniqueidentifier] NOT NULL,
		[ParentId] [uniqueidentifier] NULL,
		[CorrelationId] [uniqueidentifier] NOT NULL,		
		[Status] [varchar](255) NOT NULL,
		[DispatchCount] int NOT NULL,
		[RetryOn] [datetime] NULL,
		[Continuation] [varchar](max) NULL,
		[ExceptionFilters] [varchar](max) NULL,
		[Suspended] [bit]  DEFAULT 0 NOT NULL,
		[InstanceName] [varchar](1024) NULL	
	CONSTRAINT [PK_dbo.DependableJobs] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
		WITH (PAD_INDEX  = OFF, 
		STATISTICS_NORECOMPUTE  = OFF, 
		IGNORE_DUP_KEY = OFF, 
		ALLOW_ROW_LOCKS  = ON, 
		ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]		
END
ELSE 
BEGIN
	PRINT 'Updating the existing table.'	
END
COMMIT TRAN