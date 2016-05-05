IF EXISTS (SELECT * FROM app_securable WHERE name = 'EditGreenModel')
BEGIN
	UPDATE app_securable SET name = 'GreenModel', displayName = 'Green Modeling' WHERE name = 'EditGreenModel'
END