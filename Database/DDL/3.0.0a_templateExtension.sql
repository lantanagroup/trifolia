IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'template_extension')
BEGIN
CREATE TABLE [dbo].[template_extension] (
  [id] [int] IDENTITY(1,1) NOT NULL,
  [templateId] [int] NOT NULL,
  [identifier] [nvarchar](255) NOT NULL,
  [type] [nvarchar](55) NOT NULL,
  [value] [nvarchar](255) NOT NULL,
  CONSTRAINT [PK_template_extension] PRIMARY KEY CLUSTERED ([id] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

  ALTER TABLE [dbo].[template_extension]  WITH CHECK ADD  CONSTRAINT [FK_template_extension_template] FOREIGN KEY([templateId]) REFERENCES [dbo].[template] ([id])

  ALTER TABLE [dbo].[template_extension] CHECK CONSTRAINT [FK_template_extension_template]
END