# Adding document chunking to the SQL Server 2025 Semantic Search 
## 

Goal: Use Microsoft.Extensions.DataIngestion to create chunks for the documents

Trigger: SQL Server triggered function

Will need:
 - db changes (see SQL 2025 book)
 - new function with SQL trigger

### Tracking database changes

Change Tracking in SQL Server will let us know when rows have changed. We need to turn it on for the database first:
```
ALTER DATABASE CURRENT 
SET CHANGE_TRACKING = ON  
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)
```

and then for the table:
```
ALTER TABLE dbo.Documents
ENABLE CHANGE_TRACKING
WITH (TRACK_COLUMNS_UPDATED = ON)
```

Note that the ALTER DATABASE statement isn't allowed within a multi-statement transaction, so the above statements have been split into two files in the DatabaseDeployment project, and the ALTER DATABASE file has "server-configuration" added so the deployment knows not to use a transaction.


Other options include:
- Change Data Capture - another SQL Server technology that lets us monitor for table changes. In SQL Server 2025 Microsoft would prefer us to use the next one.
- The new Change Event Stream captures changes and sends them to Azure Event Hubs, so we could use an Event Hubs trigger for the function.

- ### SQL Triggered function

- Add a new function with a SQL Trigger. I've added this to the Ingestions function project, but in production I'd probably create a separate function app for it. 

See https://aka.ms/sqltrigger for details on how to use the SQL trigger binding.


### Source code
[!NOTE] 
> Source code is available on [GitHub](https://github.com/mikewild-wcl/sql-semanticsearch)

## References

- [Database and AI: solutions for keeping embeddings updated](https://devblogs.microsoft.com/azure-sql/database-and-ai-solutions-for-keeping-embeddings-updated/) talks about how to use an Azure Functions Sql Trigger binding
