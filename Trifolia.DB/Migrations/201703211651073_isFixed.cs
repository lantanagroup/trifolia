namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isFixed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.template_constraint", "isFixed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.template_constraint", "isFixed");
        }
    }
}
