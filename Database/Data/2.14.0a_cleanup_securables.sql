DECLARE @appSecurableId INT
SET @appSecurableId = (SELECT id FROM app_securable WHERE name = 'ReportTemplateUsage')

IF (@appSecurableId IS NOT NULL)
BEGIN
	DELETE FROM appsecurable_role WHERE appSecurableId = @appSecurableId
	DELETE FROM app_securable WHERE id = @appSecurableId
END

SET @appSecurableId = (SELECT id FROM app_securable WHERE name = 'ReportValuesetComparison')

IF (@appSecurableId IS NOT NULL)
BEGIN
	DELETE FROM appsecurable_role WHERE appSecurableId = @appSecurableId
	DELETE FROM app_securable WHERE id = @appSecurableId
END

SET @appSecurableId = (SELECT id FROM app_securable WHERE name = 'ReportCodeValidation')

IF (@appSecurableId IS NOT NULL)
BEGIN
	DELETE FROM appsecurable_role WHERE appSecurableId = @appSecurableId
	DELETE FROM app_securable WHERE id = @appSecurableId
END

SET @appSecurableId = (SELECT id FROM app_securable WHERE name = 'ReportViewValuesetActivity')

IF (@appSecurableId IS NOT NULL)
BEGIN
	DELETE FROM appsecurable_role WHERE appSecurableId = @appSecurableId
	DELETE FROM app_securable WHERE id = @appSecurableId
END