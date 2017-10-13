namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class vTemplateRelationshipRequired : DbMigration
    {
        public override void Up()
        {
            this.Sql(@"
CREATE FUNCTION IsConstraintRequired (@templateConstraintId INT)
RETURNS BIT
AS
BEGIN
	DECLARE @isRequired BIT
	DECLARE @currentTemplateConstraintId INT
	SET @currentTemplateConstraintId = @templateConstraintId

	WHILE (@currentTemplateConstraintId IS NOT NULL)
	BEGIN
		SELECT @isRequired = CASE 
			WHEN conformance = 'SHALL' OR conformance = 'SHALL NOT' 
			THEN 1 ELSE 0 
		END FROM template_constraint where id = @currentTemplateConstraintId

		IF (@isRequired = 0) SET @currentTemplateConstraintId = NULL
		ELSE SET @currentTemplateConstraintId = (SELECT parentConstraintId FROM template_constraint WHERE id = @currentTemplateConstraintId)
	END

	RETURN @isRequired
END
");

            this.Sql(@"
ALTER VIEW [dbo].[v_templateRelationship]
AS
	select
		pt.id as parentTemplateId,
		pt.[name] as parentTemplateName,
		pt.oid as parentTemplateIdentifier,
		pt.bookmark as parentTemplateBookmark,
		t.id as childTemplateId,
		t.[name] as childTemplateName,
		t.oid as childTemplateIdentifier,
		t.bookmark as childTemplateBookmark,
		dbo.IsConstraintRequired(tc.id) as required
	from template t
		join template_constraint_reference tcr on tcr.referenceIdentifier = t.oid
		join template_constraint tc on tc.id = tcr.templateConstraintId
		join template pt on pt.id = tc.templateId
	where
		tcr.referenceType = 0
");
        }
        
        public override void Down()
        {
            this.Sql(@"
ALTER VIEW [dbo].[v_templateRelationship]
AS
	select
		pt.id as parentTemplateId,
		pt.[name] as parentTemplateName,
		pt.oid as parentTemplateIdentifier,
		pt.bookmark as parentTemplateBookmark,
		t.id as childTemplateId,
		t.[name] as childTemplateName,
		t.oid as childTemplateIdentifier,
		t.bookmark as childTemplateBookmark,
		ISNULL(tc.conformance, 'MAY') as conformance
	from template t
		join template_constraint_reference tcr on tcr.referenceIdentifier = t.oid
		join template_constraint tc on tc.id = tcr.templateConstraintId
		join template pt on pt.id = tc.templateId
	where
		tcr.referenceType = 0
");

            this.Sql("DROP FUNCTION IsConstraintRequired");
        }
    }
}
