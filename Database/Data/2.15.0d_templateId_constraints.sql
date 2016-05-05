BEGIN TRANSACTION

DECLARE @templateId INT

CREATE TABLE #errors ([type] VARCHAR(255), templateId INT)

DECLARE templateIdentifier CURSOR FOR
SELECT id FROM template

OPEN templateIdentifier

FETCH NEXT FROM templateIdentifier
	INTO @templateId

WHILE @@FETCH_STATUS = 0
BEGIN
	-- Ignore templates that are not in consolidation
	IF NOT EXISTS(SELECT * FROM template t JOIN implementationguide ig ON ig.id = t.owningImplementationGuideId WHERE t.id = @templateId AND ig.name = 'Consolidation')
	BEGIN
		GOTO Cont
	END
	
	-- Ignore templates that have multiple templateId elements
	IF (SELECT COUNT(*) FROM template_constraint WHERE templateId = @templateId AND parentConstraintId IS NULL and context = 'templateId') > 1
	BEGIN
		INSERT INTO #errors ([type], templateId)
			VALUES ('Multiple templateId', @templateId)
		GOTO Cont
	END

	DECLARE @templateConstraintId INT
	SET @templateConstraintId = (SELECT id FROM template_constraint WHERE templateId = @templateId AND parentConstraintId IS NULL and context = 'templateId')

	-- Ignore templates that don't already have a templateId constraint
	IF (@templateConstraintId IS NULL)
	BEGIN
		INSERT INTO #errors ([type], templateId)
			VALUES ('No templateId', @templateId)
		GOTO Cont
	END

	DECLARE @oid VARCHAR(255)
	DECLARE @extension VARCHAR(255)
	SET @oid = (SELECT oid FROM template WHERE id = @templateId)
	
	-- Strip out the OID from the identifier
	IF (CHARINDEX('oid:', @oid) = 1)
	BEGIN
		SET @oid = SUBSTRING(@oid, 5, LEN(@oid))
	END

	-- Strip out the root and extension from the identifier
	IF (CHARINDEX('urn:hl7ii:', @oid) = 1)
	BEGIN
		SET @oid = SUBSTRING(@oid, LEN('urn:hl7ii:') + 1, LEN(@oid))
		SET @extension = SUBSTRING(@oid, CHARINDEX(':', @oid) + 1, LEN(@oid))
		SET @oid = SUBSTRING(@oid, 1, CHARINDEX(':', @oid) - 1)
	END

	-- Skip the template if multiple @root templateId constraints were found
	IF (SELECT COUNT(*) FROM template_constraint WHERE parentConstraintId = @templateConstraintId AND context = '@root') > 1
	BEGIN
		INSERT INTO #errors ([type], templateId)
			VALUES ('Multiple templateId/@root', @templateId)
		GOTO Cont
	END

	DECLARE @rootId INT
	SET @rootId = (SELECT id FROM template_constraint WHERE parentConstraintId = @templateConstraintId AND context = '@root')

	-- Skip this template if no @root constraint was found within the templateId
	IF (@rootId IS NULL)
	BEGIN
		INSERT INTO #errors ([type], templateId)
			VALUES ('No templateId/@root', @templateId)
		GOTO Cont
	END

	-- Skip the template if multiple @extension templateId constraints were found
	IF (SELECT COUNT(*) FROM template_constraint WHERE parentConstraintId = @templateConstraintId AND context = '@extension') > 1
	BEGIN
		INSERT INTO #errors ([type], templateId)
			VALUES ('Multiple templateId/@extension', @templateId)
		GOTO Cont
	END
	
	DECLARE @extensionId INT
	SET @extensionId = (SELECT id FROM template_constraint WHERE parentConstraintId = @templateConstraintId AND context = '@extension')

	-- Update @root constraint
	UPDATE template_constraint SET value = @oid WHERE id = @rootId

	-- Create OR update @extension constraint
	IF (@extension IS NOT NULL)
	BEGIN
		IF (@extensionId IS NULL)
		BEGIN
			INSERT INTO template_constraint (parentConstraintId, templateId, context, conformance, cardinality, [order], value, isBranchIdentifier, isStatic)
			SELECT parentConstraintId, templateId, '@extension', conformance, cardinality, 2, @extension, isBranchIdentifier, isStatic FROM template_constraint WHERE id = @rootId
		END

		IF (@extensionId IS NOT NULL)
		BEGIN
			UPDATE template_constraint SET value = @extension WHERE id = @extensionId
		END
	END

	SET @oid = NULL
	SET @extension = NULL

	Cont:
	FETCH NEXT FROM templateIdentifier
		INTO @templateId
END

CLOSE templateIdentifier
DEALLOCATE templateIdentifier

SELECT #errors.type, t.name, t.oid FROM #errors
  JOIN template t on t.id = #errors.templateId

DROP TABLE #errors

COMMIT TRANSACTION