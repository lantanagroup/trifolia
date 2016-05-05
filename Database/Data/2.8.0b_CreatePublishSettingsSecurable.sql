SET NOCOUNT ON
GO

IF NOT EXISTS(SELECT * from app_securable WHERE name = 'PublishSettings')
BEGIN
	DECLARE @PublishSettingsSecurable INT;
	INSERT INTO app_securable (name, displayName) VALUES ('PublishSettings', 'Publish Settings');
	SELECT @PublishSettingsSecurable = SCOPE_IDENTITY();

	DECLARE @AdminRoleID INT;
	SELECT @AdminRoleID = id FROM [role] WHERE name = 'Administrators';

	IF (@AdminRoleID IS NOT NULL)
	BEGIN
		INSERT INTO appsecurable_role(appSecurableId, roleId) 
			VALUES(@PublishSettingsSecurable, @AdminRoleID);
	END
END