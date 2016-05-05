ALTER VIEW [dbo].[v_implementationGuidePermissions] AS

select distinct userId, implementationGuideId, permission
from (
	select u.id as userId, igp.implementationGuideId as implementationGuideId, igp.permission as permission
	from [user] u
	  join implementationguide_permission igp on igp.organizationId = u.organizationId

	union all

	select u.id, igp.implementationGuideId, igp.permission
	from [user] u
	  join user_group ug on ug.userId = u.id
	  join implementationguide_permission igp on igp.groupId = ug.groupId

	union all

	select u.id, igp.implementationGuideId, igp.permission
	from [user] u
	  join implementationguide_permission igp on igp.userId = u.id
) p