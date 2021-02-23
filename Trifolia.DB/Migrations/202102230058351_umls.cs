namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class umls : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.user", "umlsApiKey", c => c.String(maxLength: 255));
            DropColumn("dbo.user", "umlsUsername");
            DropColumn("dbo.user", "umlsPassword");
        }
        
        public override void Down()
        {
            AddColumn("dbo.user", "umlsPassword", c => c.String(maxLength: 255));
            AddColumn("dbo.user", "umlsUsername", c => c.String(maxLength: 255));
            DropColumn("dbo.user", "umlsApiKey");
        }
    }
}
