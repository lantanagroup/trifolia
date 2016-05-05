begin transaction

update template
set oid = 'urn:' + oid
where oid like 'oid:%'

commit transaction