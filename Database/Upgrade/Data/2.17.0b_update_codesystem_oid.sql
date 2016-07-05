begin transaction

update codesystem
set oid = 'urn:oid:' + oid
where oid not like 'urn:oid:%'

commit transaction