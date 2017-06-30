namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vImplementationGuideCodeSystems : DbMigration
    {
        public override void Up()
        {
            this.Sql(@"
create view v_implementationGuideCodeSystems
as
	select distinct *
	from (
		select distinct
			t.owningImplementationGuideId as implementationGuideId,
			cs.id as codeSystemId,
			cs.name as codeSystemName,
			cs.oid as codeSystemIdentifier,
			cs.description as codeSystemDescription
		from template t
			join template_constraint tc on t.id = tc.templateId
			join codesystem cs on cs.id = tc.codeSystemId
		where
			tc.codeSystemId is not null

		union all

		select distinct
			t.owningImplementationGuideId,
			cs.id as codeSystemId,
			cs.name as codeSystemName,
			cs.oid as codeSystemIdentifier,
			cs.description as codeSystemDescription
		from template t
			join template_constraint tc on t.id = tc.templateId
			join valueset_member vsm on vsm.valueSetId = tc.valueSetId
			join codesystem cs on cs.id = vsm.codeSystemId
	) igcs
");
        }
        
        public override void Down()
        {
            this.Sql(@"
drop view v_implementationGuideCodeSystems
");
        }
    }
}
