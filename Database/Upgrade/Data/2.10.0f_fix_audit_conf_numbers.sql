DECLARE @currentAuditRecordId INT
DECLARE @currentConstraintNumber INT
DECLARE @currentConstraintId INT
DECLARE @currentAuditRecordNote VARCHAR(5000)
DECLARE auditRecords CURSOR FOR
  SELECT a.id, tc.number, tc.id, a.note FROM audit a
    LEFT JOIN template_constraint tc ON tc.id = a.templateConstraintId
  WHERE (a.note like 'Deleted%' and a.note like '%Number=%') or tc.id != tc.number

OPEN auditRecords

FETCH NEXT FROM auditRecords 
INTO @currentAuditRecordId, @currentConstraintNumber, @currentConstraintId, @currentAuditRecordNote

CREATE TABLE #newAuditRecords (id INT, note TEXT);

WHILE @@FETCH_STATUS = 0
BEGIN
	-- Added: N/A
	IF (SUBSTRING(@currentAuditRecordNote, 0, 11) = 'Added: N/A')
	BEGIN
		SET @currentAuditRecordNote = CONCAT('Added: ', @currentConstraintNumber, SUBSTRING(@currentAuditRecordNote, 11, LEN(@currentAuditRecordNote)-11))
	END

	-- Modified
	IF (SUBSTRING(@currentAuditRecordNote, 0, 9) = 'Modified')
	BEGIN
		DECLARE @firstLength INT
		SET @firstLength = 14+LEN(CAST(@currentConstraintId AS VARCHAR))
		SET @currentAuditRecordNote = CONCAT('Modified: ', @currentConstraintNumber,
		  SUBSTRING(@currentAuditRecordNote, @firstLength, LEN(@currentAuditRecordNote)-@firstLength+1))
	END

	-- Deleted
	IF (SUBSTRING (@currentAuditRecordNote, 0, 8) = 'Deleted')
	BEGIN
		DECLARE @noteNumber VARCHAR(2000)
		DECLARE @noteNumberBegin INT
		DECLARE @noteNumberEnd INT
		DECLARE @fieldChangesBegin INT
		SET @noteNumberBegin = CHARINDEX('Number="', @currentAuditRecordNote) + 8
		SET @noteNumber = SUBSTRING(@currentAuditRecordNote, @noteNumberBegin, LEN(@currentAuditRecordNote)-@noteNumberBegin+1)
		SET @noteNumberEnd = CHARINDEX('"', @noteNumber)
		SET @noteNumber = SUBSTRING(@noteNumber, 0, @noteNumberEnd)
		SET @fieldChangesBegin = CHARINDEX('(', @currentAuditRecordNote)-1

		SET @currentAuditRecordNote = CONCAT('Deleted: ', @noteNumber, SUBSTRING(@currentAuditRecordNote, @fieldChangesBegin, LEN(@currentAuditRecordNote)-@fieldChangesBegin+1))
	END

	INSERT INTO #newAuditRecords (id, note)
		VALUES (@currentAuditRecordId, @currentAuditRecordNote)
	  
	FETCH NEXT FROM auditRecords 
	INTO @currentAuditRecordId, @currentConstraintNumber, @currentConstraintId, @currentAuditRecordNote
END

CLOSE auditRecords
DEALLOCATE auditRecords

UPDATE audit
SET audit.note = #newAuditRecords.note
FROM #newAuditRecords
WHERE audit.id = #newAuditRecords.id

DROP TABLE #newAuditRecords