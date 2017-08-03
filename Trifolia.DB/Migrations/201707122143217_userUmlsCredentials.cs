namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userUmlsCredentials : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.user", "umlsUsername", c => c.String(maxLength: 255));
            AddColumn("dbo.user", "umlsPassword", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.user", "umlsPassword");
            DropColumn("dbo.user", "umlsUsername");
        }
    }
}
