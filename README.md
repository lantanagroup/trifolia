# trifolia
Trifolia template/profile editor, repository and publication tool

[![Build status](https://ci.appveyor.com/api/projects/status/ch1e3gsaip09w2f2?svg=true)](https://ci.appveyor.com/project/seanmcilvenna/trifolia)

## Support/Issues

Change requests, issues, ideas can be submitted using JIRA Service Desk: https://trifolia.atlassian.net/servicedesk/customer/portal/2

The active backlog of triaged issues can be found here: https://trifolia.atlassian.net/secure/RapidBoard.jspa?rapidView=60&projectKey=TRIF

## Developer Setup

- Visual Studio 2012 or 2015 (at least Express for Web)
- SQL Server 2012 or newer
- ASP NET 2 Required [Download|[https://www.microsoft.com/en-us/download/details.aspx?id=34600]
- Run DB installation using EF migrate.exe
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

## Packing for distribution
1. Restore nuget packages
2. Run MSBuild
```
msbuild Trifolia.Web\Trifolia.Web.csproj /T:Package /p:Configuration="Install Release"
```
```
msbuild Trifolia.Web\Trifolia.Web.csproj /T:Package /p:Configuration="Install Debug"
```
3. Prepare the package
```
.\PreparePackage.ps1 -buildConfig "Install Release"
```
```
.\PreparePackage.ps1 -buildConfig "Install Debug"
```
4. Dist directory contains all files necessary for installation/distribution

## Installing the package

### Install.ps1
The install script performs the following:
* Installs/updates the application files
** Does not overwrite pre-existing web.config or appSettings.user.config files
** Removes any files from the application installation directory that are not part of the installation
* Installs/updates the database based on the connection information provided via install script parameters

**Options**

| Param | Description | Default |
| ----- | ----------- | ------- |
| -appServicePath | The destination directory to install the all of the application files to that IIS will host the application from. The directory must already exist. | c:\trifolia |
| -DBHost | The hostname of the server that has the SQL Server database on it | MSSQLSERVER |
| -DBName | The name of the database | trifolia |
| -ValidationKey | The validation key is used with sessions to encrypt the token used by forms authentication. This should be changed for production environments. | 87AC8F432C8DB844A4EFD024301AC1AB5808BEE9D1870689B63794D33EE3B55CDB315BB480721A107187561F388C6BEF5B623BF31E2E725FC3F3F71A32BA5DFC |
| -DecryptionKey | The decryption key is used with sessions to decrypt the token used by forms authentication. This should be changed for production environments | E001A307CCC8B1ADEA2C55B1246CDCFE8579576997FF92E7 |

### Steps
1. Create a database on the database server
2. From the prepared distribution, run Install.ps1.
```
powershell] .\Install.ps1 -appServicePath C:\trifolia -DBHost MSSQLSERVER -DBName trifolia
```
3. Create a site in IIS that points to the application installation directory.
** The site should use a .NET v4.5 application pool and ensure that the application pool has read/write access to the installation directory
4. Configure the Trifolia installation
** Per-environment configurations can be used by creating an appSettings.user.config file in the installation directory. Any properties specified in this file will overwrite the default properties in the Web.config file.

## Branches
* master = Active development. Often changing. May contain breaking changes
* latest = The latest version of changes released to trifolia.lantanagroup.com
* **others** = feature branches that may be later be merged into "master" when development is complete and ready for testing. Eventually becomes part of "latest" after testing is complete, and ready to upgrade.
