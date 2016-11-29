ALTER VIEW [dbo].[v_templatePermissions] AS

select distinct userId, templateId, permission
from (
	-- templates not associated with an ig are available to everyone
	select u.id as userId, t.id as templateId, 'View' as permission
	from [user] u, 
	  template t
	where t.owningImplementationGuideId is null

	union all

	select u.id, t.id, 'Edit'
	from [user] u, 
	  template t
	where t.owningImplementationGuideId is null

	union all

	-- templates associated with implementation guides
	-- the entire organization is available
	select u.id, t.id, igp.permission
	from [user] u,
	  implementationguide_permission igp
	  join template t on t.owningImplementationGuideId = igp.implementationGuideId
	where
	  igp.[type] = 'Everyone'

	union all 

	-- templates associated with implementation guides
	-- the user is part of a group given permission
	select u.id, t.id, igp.permission
	from [user] u
	  join user_group ug on ug.userId = u.id
	  join implementationguide_permission igp on igp.groupId = ug.groupId
	  join template t on t.owningImplementationGuideId = igp.implementationGuideId

	union all 

	-- templates associated with an implementation guide
	-- the user is assigned directly to the ig
	select u.id, t.id, igp.permission
	from [user] u
	  join implementationguide_permission igp on igp.userId = u.id
	  join template t on t.owningImplementationGuideId = igp.implementationGuideId
) p

GO


