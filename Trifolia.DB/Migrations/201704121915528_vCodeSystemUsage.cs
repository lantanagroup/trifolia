namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vCodeSystemUsage : DbMigration
    {
        public override void Up()
        {
            this.SqlResource("Trifolia.DB.Migrations.201704121915528_vCodeSystemUsage_up.sql");
        }
        
        public override void Down()
        {
            this.SqlResource("Trifolia.DB.Migrations.201704121915528_vCodeSystemUsage_down.sql");
        }
    }
}
