IF EXISTS(SELECT * FROM app_securable WHERE name = 'EditGreenModel')
BEGIN
	UPDATE app_securable SET name = 'GreenModel' WHERE name = 'EditGreenModel'
END
ELSE IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'GreenModel')
BEGIN
	INSERT INTO app_securable (name, displayName)
		VALUES ('EditGreenModel', 'Edit Green Models')
END

DECLARE @adminRoleId INT
DECLARE @greenSecurableId INT
SET @adminRoleId = (SELECT id FROM role WHERE name = 'Administrators')
SET @greenSecurableId = (SELECT id FROM app_securable WHERE name = 'EditGreenModel')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @adminRoleId AND appSecurableId = @greenSecurableId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
		VALUES (@adminRoleId, @greenSecurableId)
END