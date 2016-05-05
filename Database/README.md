Preamble
========

This directory stores the initial creation script for TDB and all associated change scripts that allow the database to morph over time, and over iterations of the TDB application.  The initial creation script must be run first, then all scripts in the DDL folder in order to get to the latest version of the Trifolia database.


Change Script Idiom
===================

In order to properly support the change script idiom, file ordering by file name will be used to support running each script in the correct order.  When a directory is sorted by file name, the order in which you need to run all scripts can be easily derived.  The naming convention used by all files is as follows:
	- "version_ordinalpositionletter_whatthescriptdoes.sql"
		- version is the version of Trifolia this applies to.  Note that this would typically be the current version of Trifolia that we're working on
		- ordinalposition is the order in which the script shall be run.  For foundational scripts such as schema creation, the number used in the file name should clearly indicate that the script shall be run before all others.  Therefore, a 1 is the most logical choice.  Note that ordinalposition is a type of grouping mechanism that can be thought of as a database update phase (e.g. phase 1 creates the database)
		- whatthescriptdoes allows the script creator to summarize what the script is doing