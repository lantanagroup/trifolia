IF NOT EXISTS (SELECT * FROM app_securable WHERE name = 'ExportXML')
BEGIN

	insert into app_securable (name, displayName, description)
	  values ('ExportXML', 'Export Templates XML', 'Ability to export templates to an XML format')

	declare @appSecurableId int
	set @appSecurableId = (select id from app_securable where name = 'ExportXML')

	insert into appsecurable_role (appSecurableId, roleId)
	select @appSecurableId, id from [role] where name in ('Administrators', 'Template Authors', 'IG Admins')
END