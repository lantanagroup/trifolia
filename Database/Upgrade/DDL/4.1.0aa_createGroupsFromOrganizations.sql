DECLARE @lcgOrgId INT
DECLARE @hl7OrgId INT
DECLARE @lcgGroupId INT
DECLARE @hl7GroupId INT

SET @lcgOrgId = (SELECT TOP 1 id FROM organization WHERE name = 'LCG')
SET @hl7OrgId = (SELECT TOP 1 id FROM organization WHERE name = 'HL7')
SET @lcgGroupId = (SELECT TOP 1 id FROM [group] WHERE name = 'LCG')
SET @hl7GroupId = (SELECT TOP 1 id FROM [group] WHERE name = 'HL7')

IF (@lcgOrgId IS NOT NULL AND @lcgGroupId IS NULL)
BEGIN
	INSERT INTO [group] (name, organizationId) VALUES ('LCG', @lcgOrgId)
	SET @lcgGroupId = @@IDENTITY

	INSERT INTO user_group (userId, groupId)
	SELECT id, @lcgGroupId FROM [user] WHERE email = 'sean.mcilvenna@lantanagroup.com' OR email = 'sarah.gaunt@lantanagroup.com'
END

IF (@hl7OrgId IS NOT NULL AND @hl7GroupId IS NULL)
BEGIN
	INSERT INTO [group] (name, organizationId) VALUES ('HL7', @hl7OrgId)
	SET @hl7GroupId = @@IDENTITY

	INSERT INTO user_group (userId, groupId)
	SELECT id, @lcgGroupId FROM [user] WHERE email = 'sean.mcilvenna@lantanagroup.com' OR email = 'sarah.gaunt@lantanagroup.com'
END

IF (@lcgOrgId IS NOT NULL AND @lcgGroupId IS NOT NULL)
BEGIN
	INSERT INTO user_group (userId, groupId)
	SELECT id, @lcgGroupId FROM [user] WHERE organizationId = @lcgOrgId
END

IF (@hl7OrgId IS NOT NULL AND @hl7GroupId IS NOT NULL)
BEGIN
	INSERT INTO user_group (userId, groupId)
	SELECT id, @hl7GroupId FROM [user] WHERE organizationId = @hl7OrgId
END