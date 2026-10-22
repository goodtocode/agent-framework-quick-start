/*
  Safe dynamic database cleanup script
  Purpose:
    Remove all user objects in the current database without dropping the database itself.
    This removes:
      - foreign keys
      - other table constraints
      - triggers
      - views
      - stored procedures
      - functions
      - tables

  IMPORTANT:
    Run this against the target database only.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @sql NVARCHAR(MAX) = N'';
DECLARE @loopCount INT = 0;
DECLARE @maxLoops INT = 50;

------------------------------------------------------------
-- 1) Drop all foreign keys (repeat until none remain)
------------------------------------------------------------
WHILE EXISTS (SELECT 1 FROM sys.foreign_keys WHERE is_ms_shipped = 0)
BEGIN
    SET @loopCount += 1;
    IF @loopCount > @maxLoops
    BEGIN
        RAISERROR('Foreign key cleanup did not converge. Check for circular dependencies or blocked constraints.', 16, 1);
        BREAK;
    END;

    SET @sql = N'';

    SELECT @sql +=
        N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(fk.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
        N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13) + CHAR(10)
    FROM sys.foreign_keys AS fk
    WHERE fk.is_ms_shipped = 0
    ORDER BY SCHEMA_NAME(fk.schema_id), OBJECT_NAME(fk.parent_object_id), fk.name;

    IF LEN(@sql) > 0
    BEGIN
        EXEC sys.sp_executesql @sql;
    END;
END;

------------------------------------------------------------
-- 2) Drop all other table constraints (PK, UNIQUE, CHECK, DEFAULT)
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) +
    N' DROP CONSTRAINT ' + QUOTENAME(c.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.objects AS c
INNER JOIN sys.tables AS t
    ON c.parent_object_id = t.object_id
WHERE c.type IN ('PK', 'UQ', 'C', 'D', 'F')
  AND c.is_ms_shipped = 0
  AND t.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(t.schema_id), t.name, c.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

------------------------------------------------------------
-- 3) Drop triggers
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'DROP TRIGGER ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(o.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.objects AS o
WHERE o.type = 'TR'
  AND o.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(o.schema_id), o.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

------------------------------------------------------------
-- 4) Drop views
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'DROP VIEW ' + QUOTENAME(SCHEMA_NAME(v.schema_id)) + N'.' + QUOTENAME(v.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.views AS v
WHERE v.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(v.schema_id), v.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

------------------------------------------------------------
-- 5) Drop stored procedures
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'DROP PROCEDURE ' + QUOTENAME(SCHEMA_NAME(p.schema_id)) + N'.' + QUOTENAME(p.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.procedures AS p
WHERE p.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(p.schema_id), p.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

------------------------------------------------------------
-- 6) Drop functions
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'DROP FUNCTION ' + QUOTENAME(SCHEMA_NAME(f.schema_id)) + N'.' + QUOTENAME(f.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.objects AS f
WHERE f.type IN ('FN', 'IF', 'TF', 'FS', 'FT')
  AND f.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(f.schema_id), f.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

------------------------------------------------------------
-- 7) Drop tables
------------------------------------------------------------
SET @sql = N'';

SELECT @sql +=
    N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.tables AS t
WHERE t.is_ms_shipped = 0
ORDER BY SCHEMA_NAME(t.schema_id), t.name;

IF LEN(@sql) > 0
BEGIN
    EXEC sys.sp_executesql @sql;
END;

PRINT 'Cleanup complete: all user-defined objects were dropped, database left intact.';