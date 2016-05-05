IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE SPECIFIC_NAME = 'GetConstraintCategory')
BEGIN
	DROP FUNCTION GetConstraintCategory
END
GO

CREATE FUNCTION GetConstraintCategory (@templateConstraintId INT)
RETURNS NVARCHAR(255)
WITH EXECUTE AS CALLER
AS
BEGIN
	DECLARE @constraintId INT
	DECLARE @category NVARCHAR(255)

	SET @constraintId = @templateConstraintId

	WHILE (@constraintId IS NOT NULL)
	BEGIN
		SET @category = (SELECT category FROM template_constraint WHERE id = @constraintId)

		IF (@category IS NOT NULL AND @category != '')
		BEGIN
			RETURN @category
		END

		SET @constraintId = (SELECT parentConstraintId FROM template_constraint WHERE id = @constraintId)	
	END

	RETURN ''
END;