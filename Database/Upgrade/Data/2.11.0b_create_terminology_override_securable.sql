-- Add the 'TerminologyOverride' securable
IF NOT EXISTS (SELECT * FROM app_securable WHERE name like 'TerminologyOverride')
BEGIN
	insert into app_securable (name, displayName, [description])
	  values ('TerminologyOverride', 'Terminology Override', 'Ability to override locked value sets and code systems.')
END

DECLARE @terminologyOverrideId INT
DECLARE @adminId INT
DECLARE @editValueSetId INT
DECLARE @listValueSetId INT
DECLARE @editCodeSystemId INT
DECLARE @listCodeSystemId INT
SET @terminologyOverrideId = (SELECT id FROM app_securable WHERE Name = 'TerminologyOverride')
SET @adminId = (SELECT id FROM [role] WHERE name = 'Administrators')
SET @editValueSetId = (SELECT id FROM app_securable WHERE Name='ValueSetEdit')
SET @listValueSetId = (SELECT id FROM app_securable WHERE Name='ValueSetList')
SET @editCodeSystemId = (SELECT id FROM app_securable WHERE Name='CodeSystemEdit')
SET @listCodeSystemId = (SELECT id FROM app_securable WHERE Name='CodeSystemList')

-- Add the 'TerminologyOverride' securable to the admin role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminId AND appSecurableId = @terminologyOverrideId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@adminId, @terminologyOverrideId)
END

-- Add the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM [role] WHERE name = 'Terminology Admins')
BEGIN
	INSERT INTO [role] (name)
	  VALUES ('Terminology Admins')
END

DECLARE @termAdminId INT
SET @termAdminId = (SELECT id FROM [role] WHERE name = 'Terminology Admins')

-- Add the 'TerminologyOverride' securable to the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @termAdminId AND appSecurableId = @terminologyOverrideId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@termAdminId, @terminologyOverrideId)
END

-- Add the 'ValueSetEdit' securable to the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @termAdminId AND appSecurableId = @editValueSetId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@termAdminId, @editValueSetId)
END

-- Add the 'ValueSetList' securable to the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @termAdminId AND appSecurableId = @listValueSetId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@termAdminId, @listValueSetId)
END

-- Add the 'CodeSystemEdit' securable to the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @termAdminId AND appSecurableId = @editCodeSystemId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@termAdminId, @editCodeSystemId)
END

-- Add the 'CodeSystemList' securable to the 'Terminology Admins' role
IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @termAdminId AND appSecurableId = @listCodeSystemId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@termAdminId, @listCodeSystemId)
END