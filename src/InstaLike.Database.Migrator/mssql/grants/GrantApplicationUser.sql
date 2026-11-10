-- Runs on every deployment: maps the web app's managed identity to a database user.
-- The SID comes from the client ID, so the SQL server needs no Microsoft Graph permissions.
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE [name] = N'$ApplicationUserName$')
BEGIN
	DECLARE @sid nvarchar(max) = CONVERT(nvarchar(max), CONVERT(varbinary(16), CAST(N'$ApplicationUserClientId$' AS uniqueidentifier)), 1)
	EXEC (N'CREATE USER [$ApplicationUserName$] WITH SID = ' + @sid + N', TYPE = E')
END

IF NOT EXISTS (
	SELECT 1
	FROM sys.database_role_members rm
		INNER JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
		INNER JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
	WHERE r.[name] = N'db_datareader' AND m.[name] = N'$ApplicationUserName$')
	ALTER ROLE db_datareader ADD MEMBER [$ApplicationUserName$]

IF NOT EXISTS (
	SELECT 1
	FROM sys.database_role_members rm
		INNER JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
		INNER JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id
	WHERE r.[name] = N'db_datawriter' AND m.[name] = N'$ApplicationUserName$')
	ALTER ROLE db_datawriter ADD MEMBER [$ApplicationUserName$]
