namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multipleCodeSystemIdentifiers : DbMigration
    {
        private const string MigrateIdentifiers = @"
insert into dbo.codesystem_identifier (codeSystemId, identifier, [type], isDefault)
select
  id as codeSystemId,
  oid as identifier,
  case
    when oid like 'urn:oid:%' then 0
	when oid like 'urn:hl7ii:%' then 1
	when oid like 'http://%' then 2
	when oid like 'https://%' then 2
  end as [type],
  1 as isDefault
from codesystem
";

        private const string UnMigrateIdentifiers = @"
DECLARE @codeSystemId INT
DECLARE @oid VARCHAR(255)

DECLARE db_cursor CURSOR FOR  
SELECT id FROM codesystem

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @codeSystemId

WHILE @@FETCH_STATUS = 0   
BEGIN
	SET @oid = (SELECT TOP 1 identifier FROM codesystem_identifier WHERE codeSystemId = @codeSystemId ORDER BY isDefault DESC)
	UPDATE codesystem SET oid = @oid WHERE id = @codeSystemId
	FETCH NEXT FROM db_cursor INTO @codeSystemId
END   

CLOSE db_cursor   
DEALLOCATE db_cursor
";

        public override void Up()
        {
            CreateTable(
                "dbo.codesystem_identifier",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        codeSystemId = c.Int(nullable: false),
                        identifier = c.String(maxLength: 255),
                        isDefault = c.Boolean(nullable: false),
                        type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.codesystem", t => t.codeSystemId, cascadeDelete: true)
                .Index(t => t.codeSystemId);

            this.Sql(MigrateIdentifiers);

            this.Sql(@"
ALTER view [dbo].[v_implementationGuideCodeSystems]
as
	WITH firstIdentifier (id, codeSystemId, identifier, rowNumber) AS (
		SELECT csi.id, 
			   csi.codeSystemId,
			   csi.identifier,
			   ROW_NUMBER() OVER (PARTITION BY csi.codeSystemId ORDER BY isDefault DESC)
		  FROM codesystem_identifier csi)

	SELECT DISTINCT *
	FROM (
		SELECT DISTINCT
			t.owningImplementationGuideId AS implementationGuideId,
			cs.id AS codeSystemId,
			cs.name AS codeSystemName,
			fi.identifier AS codeSystemIdentifier,
			cs.description AS codeSystemDescription
		FROM template t
			join template_constraint tc ON t.id = tc.templateId
			join codesystem cs ON cs.id = tc.codeSystemId
			join firstIdentifier fi ON fi.codeSystemId = cs.id and fi.rowNumber = 1
		WHERE
			tc.codeSystemId is not null

		UNION ALL

		SELECT DISTINCT
			t.owningImplementationGuideId,
			cs.id AS codeSystemId,
			cs.name AS codeSystemName,
			fi.identifier AS codeSystemIdentifier,
			cs.description AS codeSystemDescription
		FROM template t
			join template_constraint tc ON t.id = tc.templateId
			join valueset_member vsm ON vsm.valueSetId = tc.valueSetId
			join codesystem cs ON cs.id = vsm.codeSystemId
			join firstIdentifier fi ON fi.codeSystemId = cs.id and fi.rowNumber = 1
	) igcs
            ");
            
            DropColumn("dbo.codesystem", "oid");
        }
        
        public override void Down()
        {
            AddColumn("dbo.codesystem", "oid", c => c.String(nullable: false, maxLength: 255));

            this.Sql(UnMigrateIdentifiers);

            DropForeignKey("dbo.codesystem_identifier", "codeSystemId", "dbo.codesystem");
            DropIndex("dbo.codesystem_identifier", new[] { "codeSystemId" });
            DropTable("dbo.codesystem_identifier");

            // v_implementationGuideCodeSystems
            this.Sql(@"
ALTER view [dbo].[v_implementationGuideCodeSystems]
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
    }
}
