IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'IGFileManagement')
BEGIN
	INSERT INTO app_securable (name, displayName, [description])
	  VALUES ('IGFileManagement', 'IG File Management', 'Allows users to upload/update/remove files associated with an implementation guide.')
END

DECLARE @adminRole INT
DECLARE @securable INT
SET @adminRole = (SELECT id FROM [role] WHERE name='Administrators')
SET @securable = (SELECT id FROM app_securable WHERE name = 'IGFileManagement')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminRole AND appSecurableId = @securable)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@adminRole, @securable)
END