namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multipleValueSetIdentifiers : DbMigration
    {
        #region Cleanup Identifiers SQL

        private const string CleanupIdentifiers = @"
update valueset set oid = 'urn:oid:' + oid
where oid not like 'urn:oid:%' and oid not like 'http://%' and oid not like 'https://%' and oid not like 'urn:hl7ii:%' and oid like '[0-9]%'

update valueset set oid = REPLACE(oid, 'htttp://', 'http://')
where oid like 'htttp://%'

update valueset set oid = 'http://' + oid
where oid not like 'urn:oid:%' and oid not like 'http://%' and oid not like 'https://%' and oid not like 'urn:hl7ii:%' and oid like '[a-Z]%'

select id, oid from valueset
where oid not like 'urn:oid:%' and oid not like 'http://%' and oid not like 'https://%' and oid not like 'urn:hl7ii:%'
";

        #endregion

        #region [Un]Migrate Identifiers SQL

        private const string MigrateIdentifiers = @"
insert into dbo.valueset_identifier
select
  id as valueSetId,
  oid as identifier,
  case
    when oid like 'urn:oid:%' then 0
	when oid like 'urn:hl7ii:%' then 1
	when oid like 'http://%' then 2
	when oid like 'https://%' then 2
  end as [type],
  1 as isDefault
from valueset
";

        private const string UnMigrateIdentifiers = @"
DECLARE @valueSetId INT
DECLARE @oid VARCHAR(255)

DECLARE db_cursor CURSOR FOR  
SELECT id FROM valueset

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @valueSetId

WHILE @@FETCH_STATUS = 0   
BEGIN
	SET @oid = (SELECT TOP 1 identifier FROM valueset_identifier WHERE valueSetId = @valueSetId ORDER BY isDefault DESC)
	UPDATE valueset SET oid = @oid WHERE id = @valueSetId
	FETCH NEXT FROM db_cursor INTO @valueSetId
END   

CLOSE db_cursor   
DEALLOCATE db_cursor
";

        #endregion

        #region Drop OID Index SQL

        private const string DropOidIndex = @"
IF EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_valueset_oid' AND object_id = OBJECT_ID('dbo.valueset'))
BEGIN
	DROP INDEX IDX_valueset_oid ON dbo.valueset
END
";

        #endregion

        public override void Up()
        {
            this.Sql(CleanupIdentifiers);

            CreateTable(
                "dbo.valueset_identifier",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        valueSetId = c.Int(nullable: false),
                        identifier = c.String(maxLength: 255),
                        isDefault = c.Boolean(nullable: false),
                        type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.valueset", t => t.valueSetId, cascadeDelete: true)
                .Index(t => t.valueSetId);

            this.Sql(MigrateIdentifiers);
            this.SqlResource("Trifolia.DB.Migrations.201703292147411_searchValueSet_up.sql");
            this.Sql(DropOidIndex);
            
            DropColumn("dbo.valueset", "oid");
        }
        
        public override void Down()
        {
            AddColumn("dbo.valueset", "oid", c => c.String(nullable: false, maxLength: 255));

            this.Sql(UnMigrateIdentifiers);
            this.SqlResource("Trifolia.DB.Migrations.201703292147411_searchValueSet_down.sql");

            DropForeignKey("dbo.valueset_identifier", "valueSetId", "dbo.valueset");
            DropIndex("dbo.valueset_identifier", new[] { "valueSetId" });
            DropTable("dbo.valueset_identifier");
        }
    }
}
