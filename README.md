# trifolia
Trifolia template/profile editor, repository and publication tool

[![Build status](https://ci.appveyor.com/api/projects/status/ch1e3gsaip09w2f2?svg=true)](https://ci.appveyor.com/project/seanmcilvenna/trifolia)

> This repository has the current Trifolia source code in it. However, it has not been fully prepared for open source users; authentication is linked to Active Directory and HL7, db installation scripts may not be fully functional, and no initial data is loaded with a new installation. These are tasks that Lantana intends to work on over the next 6 months.

## Developer Setup

- Visual Studio 2012 or 2015 (at least Express for Web)
- SQL Server 2012 or newer
- Create a SQL database named "templatedb" (or give it a custom name and update the web.config respectively)
- Create a SQL server alias to your instance of SQL server called "MSQLSERVER" (or update the web.config to use your own instance name)
- Run DB installation script (TODO: new installation script yet to be created)
- Seed the database (TODO)
- Restore all NuGet packages either manually using "nuget restore" or via Visual Studio
- Build all projects in the solution
- Run/debug the Trifolia.Web project

### Development authentication

When running in debug mode, the default Web.config is used which has a custom authentication provider that allows you to easily authenticate in Trifolia. A number of usernames and passwords are hard-coded in Trifolia's "development authentication provider":

| Username | Password |
| -------- | -------- |
| admin | tr1fol1atest |
| igAdmin | tr1fol1atest |
| schemaAdmin | tr1fol1atest |
| templateAuthor | tr1fol1atest |
| user | tr1fol1atest |
| sean.mcilvenna | tr1fol1atest1 |
| meenaxi.gosai | tr1fol1atest2 |
| keith.boone | UaUi5hdj |
| student1 | student |
| student2 - 20 | student |

## Production Installation/Upgrade

- Compile all projects (follow the "Developer Setup" steps above) in the solution
- Execute *prepare_package.bat . "Install Release"* from the SLN folder. This will place all files that need to be deployed in a "Dist" directory
- Create and seed a database for production use
- Create an IIS web site for the folder that uses a v4.5 app pool. Ensure that the app pool has permissions to the Trifolia directory and the directory where Trifolia data is stored
- Extract/copy all files from the Dist directory to a temporary location on the destination computer
- Execute "powershell.exe Install.ps1" from the temporary directory. Provide values for each of the parameters used by the script
- Execute "powershell.exe Database\InstallDB.ps1" from the temporary directory. Provide values for each parameter when asked
- Consider creating a batch file to easily execute the installation scripts for the app and database in the future with updates
- If creating a new installation of Trifolia, create an appSettings.user.config file that stores configuration values for each parameter in the <appSettings> portion of the Web.config file. This will ensure that the settings do not get overwritten.

## HL7 Authentication

To use HL7's website for authentication of HL7 users/members, you must contact HL7 to acquire an api key and shared key that can be used to authenticate redirects between HL7 and your installation of Trifolia.
After having acquired these keys, you can store them in your appSettings.user.config (or Web.config) file and the HL7 Login links will appear in Trifolia.

## Application Setup

Once the application is installed and running, you will need to perform (at least) a couple administrative tasks to make Trifolia functional/usable.

### Grant permissions for the first administrative user

1. Login with an account that you would like to make an administrative account.
2. After the profile has been created in Trifolia for your account, update the database directly to grant administrative rights to the account.

  ```
  insert into user_role select top 1 [user].id, [role].id from [user], [role] where email = 'my.email@domain.com' and [role].name = 'Administrators'
  ```
3. Refreshing the browser after adding the "Administrators" role to your user will show additional menus that allow management of Trifolia within the application.

### Add implementation guide types (schemas)

The implementation guide types (which are closely bound to the schemas) are what make the template/profile editor function, and the backbone for most functionality within Trifolia.

1. From the Administration menu, select "Implementation Guide Types"
2. Select "Add" in the top-right of the screen
3. Provide a name for the implementation guide type (ex: "CDA")
4. Select the schema (or zip file if there are multiple files for the schema) in the "Schema" field. You may download schemas that have been pre-tested from the "data" branch.
5. Specify a prefix for the schema. It is suggested that you use the same prefix for the schema that is used in the schema itself (ex: "cda")
6. Specify a namespace uri for the schema. This *must* be the correct value (matching the target namespace of the desired schema) for Trifolia to work correctly (ex: "urn:hl7-org:v3")
7. Specify one or more template types for the schema that match a ComplexType in the schema.
