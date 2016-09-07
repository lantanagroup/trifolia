# trifolia
Trifolia template/profile editor, repository and publication tool

[![Build status](https://ci.appveyor.com/api/projects/status/ch1e3gsaip09w2f2?svg=true)](https://ci.appveyor.com/project/seanmcilvenna/trifolia)

> This repository has the current Trifolia source code in it. However, it has not been fully prepared for open source users; authentication is linked to Active Directory and HL7, db installation scripts may not be fully functional, and no initial data is loaded with a new installation. These are tasks that Lantana intends to work on over the next 6 months.

## Developer Setup

- Visual Studio 2012 or 2015 (at least Express for Web)
- SQL Server 2012 or newer
- ASP NET 2 Required [Download|[https://www.microsoft.com/en-us/download/details.aspx?id=34600]
- Run DB installation script
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

## Installation Scripts
Two installation scripts are provided (\Install.ps1 and \Database\InstallDB.ps1). Both scripts are powershell scripts and can be run from the machine's command-prompt (ex: "powershell Install.ps1").

### Application Install Script
The Install.ps1 script is responsible for deploying the packaged files to an arbitrary location. Additionally, it updates the web.config to account for environment-specific settings such as database server, database name, etc. This install script is generally only used to install Trifolia in a non-developer environment (such as a production server).

This script expects the files in the specified directory to be packaged by the "prepare_backage.bat" script. The prepare_package.bat script is responsible for creating a "Dist" directory and copying the appropriate files to their correct locations within the Dist directory. This script assumes the application has been built using Visual Studio or MSBuild with the /package flag.

```
code\root] powershell Install.ps1 -rootPath "Dist" -appServicePath "c:\destination\location\for\iis" -appServiceBaseUrl "https://trifolia.lantanagroup.com" -ADUsername "active_directory_user" -ADPassword "active_directory_pass" -DBHost MSSQLSERVER -DBName trifolia
```

**Options**

| Param | Description | Default |
| ----- | ----------- | ------- |
| -rootPath | The path to the directory that contains the install files (equivilant to the Dist directory after having run prepare_package.bat | Dist |
| -appServicePath | The destination directory to install the all of the application files to that IIS will host the application from. The directory must already exist. | c:\trifolia |
| -appServiceBaseUrl | The URL that Trifolia will be hosted from | http://trifolia |
| -ADConnection | The LDAP connection string for active directory authentications erver | TestADConnectionString |
| -ADUsername | The username that can authentication against the directory to validate credentials provided to Trifolia | TestADUser |
| -ADPassword | The password for the user that can authentication against the directory to validate credentials provided to Trifolia | TestADPass |
| -DBHost | The hostname of the server that has the SQL Server database on it | MSSQLSERVER |
| -DBName | The name of the database | trifolia |

### Database Installation Script
The database requires SQL Server 2012 or greater. If creating a brand new installation of Trifolia's database, the -new switch should be provided to the powershell script. The -new switch will trigger creating a fresh install of the database (using the scripts in the "Database\New" directory, as well as prompt the user running the install script to provide some additional information for administrative users and organizations. If the -new switch is omitted, then the "Upgrade" scripts will be executed for the version specified.

**Example creating new database**
```
code\root] powershell Database\InstallDB.ps1 -databaseDirectory Database -appVersion 4.0.0 -databaseServer MSSQLSERVER -databaseName trifolia -new
```

**Example upgrading existing database**
```
code\root] powershell Database\InstallDB.ps1 -databaseDirectory Database -appVersion 4.1.0 -databaseServer MSSQLSERVER -databaseName trifolia
```

**Options**

| Param | Description | Default |
| ----- | ----------- | ------- |
| -databaseDirectory | The directory in which the database scripts are stored (includes the "New" and "Upgrade" folders) | Database |
| -appVersion | The version number of the application to install/upgrade the database for | |
| -databaseServer | The host name of the server that SQL Server 2012+ is installed on | |
| -databaseName | The name of the database to run the installation/upgrade scripts against. If a new database, will attempt to create the database if it is not already created. |
| -new | Switch that indicates that the database should be installed as a new database, rather than upgrading an existing database | No/upgrade |

## Production Installation/Upgrade

- Compile all projects with MSBuild using the path Trifolia.Web\Trifolia.Web.csproj and the switches /T:Package and /p:Configuration="Install Release"
- Execute *prepare_package.bat ./ "Install Release"* from the SLN folder. This will place all files that need to be deployed in a "Dist" directory
- Create and seed a database for production use (see "Database Installation Script" above)
- Create an IIS web site for the folder that uses a v4.5 app pool. Ensure that the app pool has permissions to the Trifolia directory and the directory where Trifolia data is stored
- Extract/copy all files from the Dist directory to a temporary location on the destination computer
- Execute "powershell.exe Install.ps1" from the temporary directory. Provide values for each of the parameters used by the script based on what was used to create the repository and the IIS web site in the previous steps
- Execute "powershell.exe Database\InstallDB.ps1" from the temporary directory. Provide values for each parameter when asked
- Consider creating a batch file to easily execute the installation scripts for the app and database in the future with updates
- If creating a new installation of Trifolia, create an appSettings.user.config file that stores configuration values for each parameter in the <appSettings> portion of the Web.config file. This will ensure that the settings do not get overwritten.

## HL7 Authentication

To use HL7's website for authentication of HL7 users/members, you must contact HL7 to acquire an api key and shared key that can be used to authenticate redirects between HL7 and your installation of Trifolia.
After having acquired these keys, you can store them in your appSettings.user.config (or Web.config) file and the HL7 Login links will appear in Trifolia.

## Application Setup

Once the application is installed and running, you will need to perform (at least) a couple administrative tasks to make Trifolia functional/usable.

### Add implementation guide types (schemas)

The implementation guide types (which are closely bound to the schemas) are what make the template/profile editor function, and the backbone for most functionality within Trifolia.

1. From the Administration menu, select "Implementation Guide Types"
2. Select "Add" in the top-right of the screen
3. Provide a name for the implementation guide type (ex: "CDA")
4. Select the schema (or zip file if there are multiple files for the schema) in the "Schema" field. You may download schemas that have been pre-tested from the "data" branch.
5. Specify a prefix for the schema. It is suggested that you use the same prefix for the schema that is used in the schema itself (ex: "cda")
6. Specify a namespace uri for the schema. This *must* be the correct value (matching the target namespace of the desired schema) for Trifolia to work correctly (ex: "urn:hl7-org:v3")
7. Specify one or more template types for the schema that match a ComplexType in the schema.
