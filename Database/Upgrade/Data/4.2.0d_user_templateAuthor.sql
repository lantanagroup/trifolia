DECLARE @templateAuthorRoleId INT
SET @templateAuthorRoleId = (SELECT TOP 1 id FROM [role] WHERE name = 'Template Authors')

INSERT INTO user_role (userId, roleId)
SELECT id, @templateAuthorRoleId FROM [user] WHERE id NOT IN (SELECT userId FROM user_role WHERE roleId = @templateAuthorRoleId)