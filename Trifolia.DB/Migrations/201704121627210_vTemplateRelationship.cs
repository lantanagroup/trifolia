namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vTemplateRelationship : DbMigration
    {
        public override void Up()
        {
            this.SqlResource("Trifolia.DB.Migrations.201704121627210_vTemplateRelationship_up.sql");

        }
        
        public override void Down()
        {
            this.SqlResource("Trifolia.DB.Migrations.201704121627210_vTemplateRelationship_down.sql");
        }
    }
}
