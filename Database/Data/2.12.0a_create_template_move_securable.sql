IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'TemplateMove')
BEGIN
	INSERT INTO app_securable (name, displayName, [description])
		VALUES ('TemplateMove', 'Move Templates', 'Ability to move a template from one implementation guide to another')
END

DECLARE @templateMoveId INT
DECLARE @adminId INT

SET @templateMoveId = (SELECT id FROM app_securable WHERE name = 'TemplateMove')
SET @adminId = (SELECT id FROM [role] WHERE name = 'Administrators')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE appSecurableId = @templateMoveId AND roleId = @adminId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
		VALUES (@adminId, @templateMoveId)
END