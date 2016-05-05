IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'ExportGreen')
BEGIN
	INSERT INTO app_securable (name, displayName)
		VALUES ('ExportGreen', 'Export green artifacts')
END

DECLARE @adminRoleId INT
DECLARE @exportGreenSecurableId INT
SET @adminRoleId = (SELECT id FROM role WHERE name = 'Administrators')
SET @exportGreenSecurableId = (SELECT id FROM app_securable WHERE name = 'ExportGreen')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminRoleId AND appSecurableId = @exportGreenSecurableId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
		VALUES (@adminRoleId, @exportGreenSecurableId)
END