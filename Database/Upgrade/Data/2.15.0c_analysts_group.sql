DECLARE @organizationId INT
SET @organizationId = (SELECT id FROM organization WHERE name = 'LCG')

-- Create the analysts group if it doesn't exist
IF NOT EXISTS (SELECT * FROM [group] WHERE organizationId = @organizationId AND name = 'Internal Analysts')
BEGIN
	INSERT INTO [group] (name, organizationId)
		VALUES ('Internal Analysts', @organizationId)
END

-- Get the analysts group id
DECLARE @groupId INT
SET @groupId = (SELECT id FROM [group] WHERE organizationId = @organizationId AND name = 'Internal Analysts')
DECLARE @userId INT

-- sarah.gaunt
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'sarah.gaunt')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- yan.heras
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'yan.heras')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- chengjian.che
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'chengjian.che')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- george.koromia
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'george.koromia')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- russ.hamm
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'russ.hamm')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- zabrina.gonzaga
SET @userId = (SELECT id FROM [user] WHERE organizationId = @organizationId AND userName = 'zabrina.gonzaga')

IF NOT EXISTS (SELECT * FROM user_group WHERE userId = @userId AND groupId = @groupId)
BEGIN
	INSERT INTO user_group (userId, groupId)
		VALUES (@userId, @groupId)
END

-- Assign analysts group to all implementation guides
INSERT INTO implementationguide_permission (implementationGuideId, permission, [type], groupId)
SELECT ig.id, 'View', 'Group', @groupId FROM implementationguide ig
WHERE NOT EXISTS (
	SELECT * FROM implementationguide_permission 
	WHERE implementationGuideId = ig.id AND permission = 'View' and groupId = @groupId)
	
INSERT INTO implementationguide_permission (implementationGuideId, permission, [type], groupId)
SELECT ig.id, 'Edit', 'Group', @groupId FROM implementationguide ig
WHERE NOT EXISTS (
	SELECT * FROM implementationguide_permission 
	WHERE implementationGuideId = ig.id AND permission = 'Edit' and groupId = @groupId)