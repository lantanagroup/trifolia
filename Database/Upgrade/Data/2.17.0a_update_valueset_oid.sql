begin transaction

update valueset
set oid = 'urn:oid:' + oid
where oid not like 'urn:oid:%'

commit transaction