CREATE DATABASE TestBI
GO
USE TestBI

CREATE TABLE InovioTransaction (
    ID int IDENTITY(1,1) PRIMARY KEY,
    Type varchar(255),
    CustomerId int,
    CreatedAt datetime,
    INDEX CUSTOMER_ID_INDEX (CustomerId)
)
GO

QUIT
