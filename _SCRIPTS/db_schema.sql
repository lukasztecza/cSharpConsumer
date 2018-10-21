IF NOT EXISTS (SELECT * FROM sys.Databases WHERE Name = 'TestBI')
CREATE DATABASE TestBI;
GO

USE TestBI
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Affiliates' and xtype='U')
CREATE TABLE Affiliates (
    Id int PRIMARY KEY,
    Name varchar(32),
    Location varchar(64),
    Notes varchar(256),
    CreationDate datetimeoffset,
    INDEX Index_Affiliates_CreationDate NONCLUSTERED (CreationDate)
)
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Sessions' and xtype='U')
CREATE TABLE Sessions (
    Id varchar(64) PRIMARY KEY,
    AffiliateId int FOREIGN KEY REFERENCES Affiliates(Id),
    CampaignId varchar(64),
    Ip varchar(15),
    SiteName varchar(32),
    CreationDate datetimeoffset,
    INDEX Index_Sessions_AffiliateId NONCLUSTERED (AffiliateId),
    INDEX Index_Sessions_CampaignId NONCLUSTERED (CampaignId),
    INDEX Index_Sessions_CreationDate NONCLUSTERED (CreationDate)
)
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Registrations' and xtype='U')
CREATE TABLE Registrations (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int,
    UserName varchar(32),
    Email varchar(256),
    SiteName varchar(32),
    SessionId varchar(64) FOREIGN KEY REFERENCES Sessions(Id),
    Ip varchar(15),
    CreationDate datetimeoffset,
    CONSTRAINT UniqueConstraint_Registrations_UserName_SiteName_CreationDate UNIQUE (UserName, SiteName, CreationDate),
    INDEX Index_Registrations_CreationDate NONCLUSTERED (CreationDate)
)
GO

QUIT
