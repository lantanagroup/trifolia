IF NOT EXISTS (SELECT * FROM templatetype where implementationGuideTypeId = 5 and name = 'Extension')
BEGIN
	INSERT INTO templatetype (implementationGuideTypeId, name, outputOrder, rootContext, rootContextType)
	  VALUES (5, 'Extension', 93, 'Extension', 'Extension')
END