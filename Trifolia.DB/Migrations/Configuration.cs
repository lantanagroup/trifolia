namespace Trifolia.DB.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Trifolia.DB.TrifoliaDatabase>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Trifolia.DB.TrifoliaDatabase context)
        {
            context.ImplementationGuideTypes.AddOrUpdate(igt => igt.Id,
                new ImplementationGuideType()
                {
                    Id = 1,
                    Name = "CDA",
                    SchemaLocation = "cda.xsd",
                    SchemaPrefix = "cda",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 2,
                    Name = "eMeasure",
                    SchemaLocation = "schemas\\EMeasure.xsd",
                    SchemaPrefix = "ems",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 3,
                    Name = "HQMF R2",
                    SchemaLocation = "schemas\\EMeasure.xsd",
                    SchemaPrefix = "hqmf",
                    SchemaURI = "urn:hl7-org:v3"
                },
                new ImplementationGuideType()
                {
                    Id = 4,
                    Name = "FHIR DSTU1",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                },
                new ImplementationGuideType()
                {
                    Id = 5,
                    Name = "FHIR DSTU2",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                },
                new ImplementationGuideType()
                {
                    Id = 6,
                    Name = "FHIR DSTU3",
                    SchemaLocation = "fhir-all.xsd",
                    SchemaPrefix = "fhir",
                    SchemaURI = "http://hl7.org/fhir"
                }
            );
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
