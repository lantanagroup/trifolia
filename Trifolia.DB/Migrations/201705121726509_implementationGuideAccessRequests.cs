namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class implementationGuideAccessRequests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.implementationguide_accessreq",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        implementationGuideId = c.Int(nullable: false),
                        permission = c.String(),
                        requestUserId = c.Int(nullable: false),
                        message = c.String(),
                        requestDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.user", t => t.requestUserId)
                .ForeignKey("dbo.implementationguide", t => t.implementationGuideId, cascadeDelete: true)
                .Index(t => t.implementationGuideId)
                .Index(t => t.requestUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.implementationguide_accessreq", "implementationGuideId", "dbo.implementationguide");
            DropForeignKey("dbo.implementationguide_accessreq", "requestUserId", "dbo.user");
            DropIndex("dbo.implementationguide_accessreq", new[] { "requestUserId" });
            DropIndex("dbo.implementationguide_accessreq", new[] { "implementationGuideId" });
            DropTable("dbo.implementationguide_accessreq");
        }
    }
}
