namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vValueSetMemberWhiteSpace : DbMigration
    {
        public override void Up()
        {
            this.Sql(@"
CREATE VIEW vValueSetMemberWhiteSpace
AS
SELECT
  vs.id AS valueSetId,
  vs.name AS valueSetName,
  vsm.code AS code,
  vsm.displayName AS displayName
FROM valueset vs
JOIN valueset_member vsm ON vs.id = vsm.valueSetId
WHERE
  DATALENGTH(vsm.code) != DATALENGTH(LTRIM(RTRIM(vsm.code)))
  OR DATALENGTH(vsm.displayName) != DATALENGTH(LTRIM(RTRIM(vsm.displayName)))
");
        }
        
        public override void Down()
        {
            this.Sql(@"DROP VIEW vValueSetMemberWhiteSpace");
        }
    }
}
