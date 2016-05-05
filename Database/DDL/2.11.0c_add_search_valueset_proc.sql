SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME = 'SearchValueSet')
BEGIN
	DROP PROCEDURE SearchValueSet
END
GO

CREATE PROCEDURE SearchValueSet
	@searchText VARCHAR(255)
AS
BEGIN
	DECLARE @searchTextAny VARCHAR(512)
	SET @searchTextAny = CONCAT('%', @searchText, '%')

	SELECT distinct vs.* FROM valueset vs
		LEFT JOIN valueset_member vsm ON vsm.valueSetId = vs.Id
		LEFT JOIN codesystem cs on vsm.codeSystemId = cs.id
	WHERE
		vs.code LIKE @searchTextAny
		OR vs.name LIKE @searchTextAny
		OR vs.oid LIKE @searchTextAny
		OR ISNULL(vs.description, '') LIKE @searchTextAny
		OR ISNULL(vs.source, '') LIKE @searchTextAny
		OR ISNULL(vsm.code, '') LIKE @searchTextAny
		OR ISNULL(vsm.displayName, '') LIKE @searchTextAny
		OR ISNULL(cs.name, '') LIKE @searchTextAny
		OR ISNULL(cs.oid, '') LIKE @searchTextAny
END
GO
