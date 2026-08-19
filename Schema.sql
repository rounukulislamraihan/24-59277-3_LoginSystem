-- ============================================================
-- Schema.sql
-- Database: 24-59277-3_LoginDB
--
-- NOTE: the name is wrapped in [square brackets] below. That's
-- required (not optional) because it starts with a digit and
-- contains hyphens, so SQL Server won't accept it as a plain
-- unquoted identifier - [brackets] mark it as a delimited
-- identifier, which can contain almost any character. The
-- connection string in App.config does NOT need brackets, since
-- Initial Catalog there is just a string value, not T-SQL syntax.
-- ============================================================

CREATE DATABASE [24-59277-3_LoginDB];
GO

USE [24-59277-3_LoginDB];
GO

CREATE TABLE dbo.Users
(
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,   -- SHA-256 hash, never the real password
    Email        NVARCHAR(100) NULL,
    FullName     NVARCHAR(100) NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- Bonus table: LoginHistory
-- One row per login. LogoutTime starts NULL and is stamped when
-- the user logs out (see DatabaseHelper.RecordLogin / RecordLogout).
-- ============================================================
CREATE TABLE dbo.LoginHistory
(
    LoginHistoryID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT NOT NULL,
    LoginTime      DATETIME NOT NULL DEFAULT GETDATE(),
    LogoutTime     DATETIME NULL,
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserID)
        REFERENCES dbo.Users (UserID)
);
GO
