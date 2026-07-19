-- -- SELECT IS_SRVROLEMEMBER('sysadmin') AS IsSysAdmin, IS_SRVROLEMEMBER('dbcreator') AS IsDbCreator;
-- -- Go

Use AgentFramework
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
DROP TABLE [dbo].[__EFMigrationsHistory]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Chat].[ChatMessages]') AND type in (N'U'))
DROP TABLE [Chat].[ChatMessages]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Chat].[ChatSessions]') AND type in (N'U'))
DROP TABLE [Chat].[ChatSessions]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Chat].[Actors]') AND type in (N'U'))
DROP TABLE [Chat].[Actors]
GO