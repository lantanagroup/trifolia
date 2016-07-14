IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'Import')
BEGIN
	INSERT INTO app_securable (name, displayName, [description])
	VALUES ('Import', 'Import', 'The ability to import implementation guides and templates into Trifolia')
END

DECLARE @importAppSecurableId INT
DECLARE @templateAuthorRoleId INT
DECLARE @igAdminRoleId INT
DECLARE @adminRoleId INT

SET @templateAuthorRoleId = (SELECT top 1 id FROM [role] WHERE name = 'Template Authors')
SET @igAdminRoleId = (SELECT top 1 id FROM [role] WHERE name = 'IG Admins')
SET @importAppSecurableId = (SELECT top 1 id FROM [app_securable] WHERE name = 'Import')
SET @adminRoleId = (SELECT top 1 id FROM [role] WHERE name = 'Administrators')

IF (@templateAuthorRoleId IS NOT NULL AND NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @templateAuthorRoleId AND appSecurableId = @importAppSecurableId))
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	VALUES (@templateAuthorRoleId, @importAppSecurableId)
END

IF (@igAdminRoleId IS NOT NULL AND NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @igAdminRoleId AND appSecurableId = @importAppSecurableId))
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	VALUES (@igAdminRoleId, @importAppSecurableId)
END

IF (@adminRoleId IS NOT NULL AND NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminRoleId AND appSecurableId = @importAppSecurableId))
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	VALUES (@adminRoleId, @importAppSecurableId)
END