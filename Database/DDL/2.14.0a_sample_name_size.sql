UPDATE template_sample SET name = 'Unknown Sample Name' WHERE name IS NULL
GO
ALTER TABLE template_sample ALTER COLUMN name NVARCHAR(255) NOT NULL
GO

UPDATE template_constraint_sample SET name = 'Unknown Sample Name' WHERE name IS NULL
GO
ALTER TABLE template_constraint_sample ALTER COLUMN name NVARCHAR(255) NOT NULL
GO