namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multipleContainedTemplates : DbMigration
    {
        private const string DropContainedTemplateForeignKey = @"
IF EXISTS (select * from sys.foreign_keys where name = 'FK_template_constraint_template1' AND parent_object_id = OBJECT_ID('dbo.template_constraint'))
BEGIN
	ALTER TABLE dbo.template_constraint DROP CONSTRAINT FK_template_constraint_template1
END
";

        private const string DropContainedTemplateIndex = @"
IF EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_template_constraint_containedTemplateId' AND object_id = OBJECT_ID('dbo.template_constraint'))
BEGIN
	DROP INDEX IDX_template_constraint_containedTemplateId ON dbo.template_constraint
END
";

        private const string MigrateContainedTemplates = @"
insert into template_constraint_reference (templateConstraintId, referenceIdentifier, referenceType)
select tc.id, t.oid, 0 from template_constraint tc
  join template t on t.id = tc.containedTemplateId
";

        private const string UnMigrateContainedTemplates = @"
update template_constraint set containedTemplateId = t.id
from template_constraint_reference tcr
  join template t on t.oid = tcr.referenceIdentifier
where
  tcr.referenceType = 0 and template_constraint.id = tcr.templateConstraintId
";
        public override void Up()
        {
            CreateTable(
                "dbo.template_constraint_reference",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        templateConstraintId = c.Int(nullable: false),
                        referenceIdentifier = c.String(),
                        referenceType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.template_constraint", t => t.templateConstraintId, cascadeDelete: true)
                .Index(t => t.templateConstraintId);

            this.Sql(MigrateContainedTemplates);
            this.Sql(DropContainedTemplateIndex);
            this.Sql(DropContainedTemplateForeignKey);

            DropForeignKey("dbo.template_constraint", "containedTemplateId", "dbo.template");
            DropIndex("dbo.template_constraint", new[] { "containedTemplateId" });
            DropColumn("dbo.template_constraint", "containedTemplateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.template_constraint", "containedTemplateId", c => c.Int());

            this.Sql(UnMigrateContainedTemplates);

            DropForeignKey("dbo.template_constraint_reference", "templateConstraintId", "dbo.template_constraint");
            DropIndex("dbo.template_constraint_reference", new[] { "templateConstraintId" });
            DropTable("dbo.template_constraint_reference");
            CreateIndex("dbo.template_constraint", "containedTemplateId");
            AddForeignKey("dbo.template_constraint", "containedTemplateId", "dbo.template", "id");
        }
    }
}
