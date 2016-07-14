IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'WebIG')
BEGIN
	INSERT INTO app_securable (name, displayName, description)
	  VALUES ('WebIG', 'Web-based IG', 'Ability to view an implementation guide''s web-based IG')
END

DECLARE @webIgId INT
DECLARE @templateAuthorsId INT
SET @templateAuthorsId = (SELECT id from [role] where name = 'Template Authors')
SET @webIgId = (SELECT id FROM app_securable WHERE name = 'WebIG')

IF NOT EXISTS (SELECT * FROM appsecurable_role WHERE roleId = @templateAuthorsId AND appSecurableId = @webIgId)
BEGIN
	INSERT INTO appsecurable_role (roleId, appSecurableId)
	  VALUES (@templateAuthorsId, @webIgId)
END