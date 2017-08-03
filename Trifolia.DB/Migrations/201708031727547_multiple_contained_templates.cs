namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class multiple_contained_templates : DbMigration
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

        #region getImplementationGuideTemplates

        private const string GetImplementationGuideTemplatesUp = @"
ALTER PROCEDURE [dbo].[GetImplementationGuideTemplates]
	@implementationGuideId INT,
	@inferred BIT,
	@parentTemplateId INT = NULL,
	@categories AS CategoryList READONLY
AS
BEGIN
	DECLARE @currentImplementationGuideId INT = @implementationGuideId, @relationshipCount INT, @retiredStatusId INT
	DECLARE @False BIT = 0, @True BIT = 1
	DECLARE @categoryCount INT

	SET @categoryCount = (SELECT COUNT(*) FROM @categories)

	CREATE TABLE #implementationGuides (id INT, [version] INT)
	CREATE TABLE #templates (id INT)
	CREATE TABLE #relationships (id INT)

	SEt @retiredStatusId = (SELECT id FROM publish_status WHERE [status] = 'Retired')

	WHILE (@currentImplementationGuideId IS NOT NULL)
	BEGIN
		INSERT INTO #implementationGuides (id, [version])
		SELECT @currentImplementationGuideId, [version] FROM implementationguide WHERE id = @currentImplementationGuideId

		SET @currentImplementationGuideId = (SELECT previousVersionImplementationGuideId FROM implementationguide WHERE id = @currentImplementationGuideId)
	END

	-- Loop through the IG versions from beginning to end adding/removing templates as appropriate
	IF (@parentTemplateId IS NULL)
	BEGIN
		DECLARE ig_cursor CURSOR
			FOR SELECT id FROM #implementationGuides ORDER BY [version]
		OPEN ig_cursor
		FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM #templates
			WHERE id in (SELECT previousVersionTemplateId FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId)

			INSERT INTO #templates (id)
			SELECT id FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId

			FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;
		END

		CLOSE ig_cursor;
		DEALLOCATE ig_cursor;
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT @parentTemplateId
	END

	-- Remove any retired templates that aren't part of this version of the IG
	DELETE FROM #templates WHERE id IN (SELECT id FROM template WHERE owningImplementationGuideId != @implementationGuideId AND statusId = @retiredStatusId)

	insert_relationships:

	INSERT INTO #relationships (id)
	-- Contained templates
	SELECT t.id
	FROM template_constraint tc
	    join template_constraint_reference tcr on tcr.templateConstraintId = tc.id
		join template t on t.oid = tcr.referenceIdentifier
		JOIN #templates tt ON tt.id = tc.templateId
	WHERE
	    tcr.referenceType = 0 AND
		t.id NOT IN (SELECT id FROM #templates) AND
		-- Either not filtering by categories, constraint's category is not set, or the category matches one of the specified categories
		(@categoryCount = 0 OR dbo.GetConstraintCategory(tc.id) = '' OR dbo.GetConstraintCategory(tc.id) IN (SELECT category FROM @categories))

	UNION ALL

	-- Implied templates
	SELECT t.impliedTemplateId
	FROM template t
		JOIN #templates tt ON tt.id = t.id
	WHERE
		t.impliedTemplateId IS NOT NULL AND
		t.impliedTemplateId NOT IN (SELECT id FROM #templates)

	IF (@inferred = 1)
	BEGIN
		INSERT INTO #templates
		SELECT id
		FROM #relationships
		SET @relationshipCount = @@ROWCOUNT
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT #relationships.id
		FROM #relationships
			JOIN template t ON #relationships.id = t.id
			JOIN #implementationGuides ON t.owningImplementationGuideId = #implementationGuides.id
		SET @relationshipCount = @@ROWCOUNT
	END

	DELETE FROM #relationships

	IF (@relationshipCount > 0)
	BEGIN
		GOTO insert_relationships
	END

	SELECT DISTINCT id FROM #templates

	DROP TABLE #relationships
	DROP TABLE #templates
	DROP TABLE #implementationGuides
END
";
        private const string GetImplementationGuideTemplatesDown = @"
ALTER PROCEDURE [dbo].[GetImplementationGuideTemplates]
	@implementationGuideId INT,
	@inferred BIT,
	@parentTemplateId INT = NULL,
	@categories AS CategoryList READONLY
AS
BEGIN
	DECLARE @currentImplementationGuideId INT = @implementationGuideId, @relationshipCount INT, @retiredStatusId INT
	DECLARE @False BIT = 0, @True BIT = 1
	DECLARE @categoryCount INT

	SET @categoryCount = (SELECT COUNT(*) FROM @categories)

	CREATE TABLE #implementationGuides (id INT, [version] INT)
	CREATE TABLE #templates (id INT)
	CREATE TABLE #relationships (id INT)

	SEt @retiredStatusId = (SELECT id FROM publish_status WHERE [status] = 'Retired')

	WHILE (@currentImplementationGuideId IS NOT NULL)
	BEGIN
		INSERT INTO #implementationGuides (id, [version])
		SELECT @currentImplementationGuideId, [version] FROM implementationguide WHERE id = @currentImplementationGuideId

		SET @currentImplementationGuideId = (SELECT previousVersionImplementationGuideId FROM implementationguide WHERE id = @currentImplementationGuideId)
	END

	-- Loop through the IG versions from beginning to end adding/removing templates as appropriate
	IF (@parentTemplateId IS NULL)
	BEGIN
		DECLARE ig_cursor CURSOR
			FOR SELECT id FROM #implementationGuides ORDER BY [version]
		OPEN ig_cursor
		FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			DELETE FROM #templates
			WHERE id in (SELECT previousVersionTemplateId FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId)

			INSERT INTO #templates (id)
			SELECT id FROM template WHERE owningImplementationGuideId = @currentImplementationGuideId

			FETCH NEXT FROM ig_cursor INTO @currentImplementationGuideId;
		END

		CLOSE ig_cursor;
		DEALLOCATE ig_cursor;
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT @parentTemplateId
	END

	-- Remove any retired templates that aren't part of this version of the IG
	DELETE FROM #templates WHERE id IN (SELECT id FROM template WHERE owningImplementationGuideId != @implementationGuideId AND statusId = @retiredStatusId)

	insert_relationships:

	INSERT INTO #relationships (id)
	-- Contained templates
	SELECT tc.containedTemplateId AS id
	FROM template_constraint tc
		JOIN #templates tt ON tt.id = tc.templateId
	WHERE
		tc.containedTemplateId IS NOT NULL AND
		tc.containedTemplateId NOT IN (SELECT id FROM #templates) AND
		-- Either not filtering by categories, constraint's category is not set, or the category matches one of the specified categories
		(@categoryCount = 0 OR dbo.GetConstraintCategory(tc.id) = '' OR dbo.GetConstraintCategory(tc.id) IN (SELECT category FROM @categories))

	UNION ALL

	-- Implied templates
	SELECT t.impliedTemplateId
	FROM template t
		JOIN #templates tt ON tt.id = t.id
	WHERE
		t.impliedTemplateId IS NOT NULL AND
		t.impliedTemplateId NOT IN (SELECT id FROM #templates)

	IF (@inferred = 1)
	BEGIN
		INSERT INTO #templates
		SELECT id
		FROM #relationships
		SET @relationshipCount = @@ROWCOUNT
	END
	ELSE
	BEGIN
		INSERT INTO #templates
		SELECT #relationships.id
		FROM #relationships
			JOIN template t ON #relationships.id = t.id
			JOIN #implementationGuides ON t.owningImplementationGuideId = #implementationGuides.id
		SET @relationshipCount = @@ROWCOUNT
	END

	DELETE FROM #relationships

	IF (@relationshipCount > 0)
	BEGIN
		GOTO insert_relationships
	END

	SELECT DISTINCT id FROM #templates

	DROP TABLE #relationships
	DROP TABLE #templates
	DROP TABLE #implementationGuides
END
";

        #endregion

        #region vContainedTemplateCount

        private const string ViewContainedTemplateCountUp = @"
ALTER VIEW [dbo].[v_containedtemplatecount] AS 
select 
  t.id AS containedTemplateId, 
  count(*) AS total 
from template_constraint_reference tcr
  join template t on t.id = tcr.referenceIdentifier
where
  tcr.referenceType = 0
group by t.id;
";

        private const string ViewContainedTemplateCountDown = @"
ALTER VIEW [dbo].[v_containedtemplatecount] AS 
select 
  template_constraint.containedTemplateId AS containedTemplateId, 
  count(*) AS total 
from template_constraint 
group by template_constraint.containedTemplateId;
";

        #endregion

        #region vTemplateUsage

        private const string ViewTemplateUsageUp = @"
ALTER view [dbo].[v_templateusage] as
select distinct 
  t.id as templateId, 
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join implementationguide ig on t.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId

union all

select distinct 
  t.id as templateId, 
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join template_constraint_reference tcr on t.oid = tcr.referenceIdentifier
join template_constraint tc on tcr.templateConstraintId = tc.id
join template t2 on t2.id = tc.templateId
join implementationguide ig on t2.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId
where tcr.referenceType = 0

union all

select distinct 
  t.id as templateId,
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join template t2 on t2.impliedTemplateId = t.id
join implementationguide ig on t2.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId
";

        private const string ViewTemplateUsageDown = @"
ALTER view [dbo].[v_templateusage] as
select distinct 
  t.id as templateId, 
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join implementationguide ig on t.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId

union all

select distinct 
  t.id as templateId, 
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join template_constraint tc on t.id = tc.containedTemplateId
join template t2 on t2.id = tc.templateId
join implementationguide ig on t2.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId

union all

select distinct 
  t.id as templateId,
  t.name + ' (' + t.oid + ')' as templateDisplay,
  ig.id as implementationGuideId,
  ig.name + ' (' + igt.name + ')' as implementationGuideDisplay
from template t
join template t2 on t2.impliedTemplateId = t.id
join implementationguide ig on t2.owningImplementationGuideId = ig.id
join implementationguidetype igt on igt.id = ig.implementationGuideTypeId
";

        #endregion

        #region vTemplateRelationship

        private const string ViewTemplateRelationshipUp = @"
CREATE VIEW v_templateRelationship
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
";

        private const string ViewTemplateRelationshipDown = @"
DROP VIEW v_templateRelationship
";

        #endregion

        #region vCodeSystemUsage

        private const string ViewCodeSystemUsageUp = @"
CREATE VIEW v_codeSystemUsage
AS
	select
	  distinct
	  t.id as templateId,
	  t.oid as templateIdentifier,
	  t.name as templateName,
	  t.bookmark as templateBookmark,
	  cs.id as codeSystemId,
	  cs.oid as codeSystemIdentifier,
	  cs.name as codeSystemName
	from template_constraint tc
	  join template t on t.id = tc.templateId
	  join codesystem cs on cs.id = tc.codeSystemId

	union all

	select
	  distinct
	  t.id as templateId,
	  t.oid as templateIdentifier,
	  t.name as templateName,
	  t.bookmark as templateBookmark,
	  cs.id as codeSystemId,
	  cs.oid as codeSystemIdentifier,
	  cs.name as codeSystemName
	from template_constraint tc
	  join template t on t.id = tc.templateId
	  join valueset_member vsm on vsm.valueSetId = tc.valueSetId
	  join codesystem cs on cs.id = vsm.codeSystemId
";

        private const string ViewCodeSystemUsageDown = @"
DROP VIEW v_codeSystemUsage
";

        #endregion

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

            this.Sql(GetImplementationGuideTemplatesUp);
            this.Sql(ViewContainedTemplateCountUp);
            this.Sql(ViewTemplateUsageUp);
            this.Sql(ViewTemplateRelationshipUp);
            this.Sql(ViewCodeSystemUsageUp);

            DropForeignKey("dbo.template_constraint", "containedTemplateId", "dbo.template");
            DropIndex("dbo.template_constraint", new[] { "containedTemplateId" });
            DropColumn("dbo.template_constraint", "containedTemplateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.template_constraint", "containedTemplateId", c => c.Int());

            this.Sql(UnMigrateContainedTemplates);
            this.Sql(GetImplementationGuideTemplatesDown);
            this.Sql(ViewContainedTemplateCountDown);
            this.Sql(ViewTemplateUsageDown);
            this.Sql(ViewTemplateRelationshipDown);
            this.Sql(ViewCodeSystemUsageDown);

            DropForeignKey("dbo.template_constraint_reference", "templateConstraintId", "dbo.template_constraint");
            DropIndex("dbo.template_constraint_reference", new[] { "templateConstraintId" });
            DropTable("dbo.template_constraint_reference");
            CreateIndex("dbo.template_constraint", "containedTemplateId");
            AddForeignKey("dbo.template_constraint", "containedTemplateId", "dbo.template", "id");
        }
    }
}
