BEGIN TRANSACTION

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'implementationguide_section')
BEGIN
	CREATE TABLE dbo.implementationguide_section
		(
		id int NOT NULL IDENTITY (1, 1),
		implementationGuideId int NOT NULL,
		heading nvarchar(255) NOT NULL,
		[content] ntext NULL
		)  ON [PRIMARY]
		 TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE dbo.implementationguide_section ADD CONSTRAINT
		PK_implementationguide_section_1 PRIMARY KEY CLUSTERED 
		(
		id
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	ALTER TABLE dbo.implementationguide_section ADD CONSTRAINT
		FK_implementationguide_section_implementationguide FOREIGN KEY
		(
		implementationGuideId
		) REFERENCES dbo.implementationguide
		(
		id
		) ON UPDATE  NO ACTION 
		 ON DELETE  NO ACTION 
END

COMMIT TRANSACTION