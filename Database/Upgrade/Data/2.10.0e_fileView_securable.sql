IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'IGFileView')
BEGIN
	INSERT INTO app_securable (name, displayName, [description])
	  VALUES ('IGFileView', 'IG File View', 'Allows users view files associated with implementation guides they have permission to access.')
END

DECLARE @adminRole INT
DECLARE @securable INT
SET @adminRole = (SELECT id FROM [role] WHERE name='Administrators')
SET @securable = (SELECT id FROM app_securable WHERE name = 'IGFileView')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminRole AND appSecurableId = @securable)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@adminRole, @securable)
END