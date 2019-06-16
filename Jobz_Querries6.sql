
DROP DATABASE Jobz
CREATE DATABASE Jobz

USE Jobz


CREATE TABLE Users 
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  UserName NVARCHAR(100) NOT NULL UNIQUE,
  [Password] NVARCHAR(100) NOT NULL,
  Email NVARCHAR(100) NOT NULL,
  [Role] NVARCHAR(20) NOT NULL,
  RightsApproved BIT NOT NULL, 
)


CREATE TABLE Regions 
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [Name] NVARCHAR(100) NOT NULL  
)


CREATE TABLE Category 
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [Name] NVARCHAR(100) NOT NULL 
)


CREATE TABLE WorkHours
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [Name] NVARCHAR(100) NOT NULL
)


CREATE TABLE IndividualProfile
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  UserId INT NOT NULL UNIQUE,  
  FirstName NVARCHAR(255) NOT NULL,
  LastName NVARCHAR(255) NOT NULL,
  FOREIGN KEY (UserId) REFERENCES Users(Id)
)


CREATE TABLE CompanyProfile
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  UserId INT NOT NULL UNIQUE,     
  CompanyName NVARCHAR(255) NOT NULL,
  [Description] NVARCHAR(500) NOT NULL,
  Adress NVARCHAR(100) NOT NULL,
  AdressNumber NVARCHAR(5) NOT NULL,
  PostalCorridor NVARCHAR(5) NOT NULL,
  OfficialSite NVARCHAR(100) NOT NULL, 
  FOREIGN KEY (UserId) REFERENCES Users(Id)
)


CREATE TABLE Job
(
  Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  UserId INT NOT NULL,
  Title NVARCHAR(100) NOT NULL,
  CategoryId INT NOT NULL,
  RegionId INT NOT NULL,
  WorkHoursId INT NOT NULL,--part time κλπ
  Openings INT NOT NULL, --θεσεις εργασιας διεθεσιμες
  Content NVARCHAR(255) NOT NULL,
  Active BIT NOT NULL,
  Created DATETIME NOT NULL,
  FOREIGN KEY (UserId) REFERENCES Users(Id),
  FOREIGN KEY (CategoryId) REFERENCES Category(Id),
  FOREIGN KEY (RegionId) REFERENCES Regions(Id),
  FOREIGN KEY (WorkHoursId) REFERENCES WorkHours(Id)
)
 
 
CREATE TABLE CompanyJobsCVPool
(  
    Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	CompanyId INT NOT NULL,
	JobId INT NOT NULL,  
	UserId INT NOT NULL, 
    [Filename] NVARCHAR(255) NOT NULL, 
	DescriptiveFileName NVARCHAR(255) NOT NULL, 
    FileContent VARBINARY(MAX) NOT NULL,
	CVSubmitted DATETIME NOT NULL,
	FOREIGN KEY (UserId) REFERENCES Users(Id),
	FOREIGN KEY (JobId) REFERENCES Job(Id),
	CONSTRAINT UniqueIds UNIQUE(JobId, UserId)
)


INSERT INTO Category 
VALUES 
('Computer Information Systems'),
('information technology'),
('Logistics')


INSERT INTO Regions 
VALUES ('Athens'),
       ('Sallonica'),
       ('Sparta')

INSERT INTO WorkHours 
VALUES ('Part-Time'),
       ('Full-Time')


INSERT INTO Users
VALUES ('Batman', 12345678, 'Batman@Batserver.com', 'Admin', 1),
       ('Superman', 12345678, 'Superman@server.com', 'Individual', 1),
	   ('KillerCrock', 12345678, 'KillerCrock@server.com', 'Individual', 1),
	   ('WonderWoman', 12345678, 'WonderWoman@server.com', 'Individual', 1),
	   ('IronMan', 12345678, 'IronMan@server.com', 'Individual', 1),
       ('Ninty', 12345678, 'Ninty@Ninty.com', 'Company', 1),
	   ('Sony', 12345678, 'Sony@Sony.com', 'Company', 1)
 
GO
CREATE PROCEDURE GetLoggedInUserIdBasedOnUsername
(@_UserName NVARCHAR(100))
AS 
BEGIN
     SELECT Id FROM Users WHERE UserName = @_UserName
END
GO


CREATE PROCEDURE LogInUser
(@_UserName NVARCHAR(100))
AS 
BEGIN
     SELECT * FROM Users WHERE UserName = @_UserName                             
END
GO


CREATE PROCEDURE CreateNewAccount
(@UserName NVARCHAR(100), @Password NVARCHAR(100), @Email NVARCHAR(100),
@Role NVARCHAR(20), @RightsApproved BIT)
AS 
BEGIN
     INSERT INTO Users VALUES(@UserName, @Password, @Email, @Role, 
     @RightsApproved)
END
GO


CREATE PROCEDURE UserEditAccountGet
(@id INT)
AS 
BEGIN
     SELECT * FROM Users WHERE Id = @id                              
END
GO


CREATE PROCEDURE UserEditAccountPost
(@_UserName NVARCHAR(100), @_Email NVARCHAR(100), @_Id INT)
AS 
BEGIN
     UPDATE Users SET UserName = @_UserName, Email = @_Email
     WHERE Id = @_Id                                
END
GO


CREATE PROCEDURE GetUserRightState
(@id INT)
AS 
BEGIN
     SELECT RightsApproved FROM Users WHERE Id = @id                                
END
GO


CREATE PROCEDURE SetUserRightState
(@_RightsApproved BIT, @_Id INT)
AS 
BEGIN
     UPDATE Users SET RightsApproved = @_RightsApproved WHERE Id = @_Id                               
END
GO


CREATE PROCEDURE AdminCompaniesList
AS 
BEGIN
     SELECT * FROM Users WHERE Users.Role = 'Company'
END
GO


CREATE PROCEDURE AdminIndividualsList
AS 
BEGIN
     SELECT * FROM Users WHERE Users.[Role] = 'Individual'
END
GO


CREATE PROCEDURE CheckIfCompanyProfileExists
(@id INT)
AS 
BEGIN
     SELECT * FROM CompanyProfile WHERE UserId = @id
END
GO


CREATE PROCEDURE CheckIfIndividualProfileExists
(@id INT)
AS 
BEGIN
     SELECT * FROM IndividualProfile WHERE UserId = @id
END
GO


CREATE PROCEDURE CreateCompanyProfile
(@UserId INT, @CompanyName NVARCHAR(255), @Description NVARCHAR(500), @Adress NVARCHAR(100),
@AdressNumber NVARCHAR(5), @PostalCorridor NVARCHAR(5), @OfficialSite NVARCHAR(100))
AS 
BEGIN
     INSERT INTO CompanyProfile VALUES(@UserId, @CompanyName, @Description, @Adress,
     @AdressNumber, @PostalCorridor, @OfficialSite)
END
GO


CREATE PROCEDURE CreateIndividualProfile
(@UserId INT, @FirstName NVARCHAR(255), @LastName NVARCHAR(255))
AS 
BEGIN
     INSERT INTO IndividualProfile VALUES(@UserId, @FirstName, @LastName)
END
GO


CREATE PROCEDURE GetIndividualProfile
(@_Id INT)
AS 
BEGIN
     SELECT * FROM IndividualProfile WHERE userId = @_Id
END
GO


CREATE PROCEDURE GetCompanyProfile
(@_Id INT)
AS 
BEGIN
     SELECT * FROM CompanyProfile WHERE userId = @_Id
END
GO


CREATE PROCEDURE EditCompanyProfile
(@_Description NVARCHAR(500), @_Adress NVARCHAR(100), @_AdressNumber NVARCHAR(5), 
@_PostalCorridor NVARCHAR(5), @_OfficialSite NVARCHAR(100), @_Id INT)
AS 
BEGIN
     UPDATE CompanyProfile SET [Description] = @_Description, Adress = @_Adress, 
     AdressNumber = @_AdressNumber, PostalCorridor = @_PostalCorridor, 
     OfficialSite = @_OfficialSite WHERE Id = @_Id                                
END
GO


CREATE PROCEDURE EditIndividualProfile
(@_FirstName NVARCHAR(255), @_LastName NVARCHAR(255), @_Id INT)
AS 
BEGIN
     UPDATE IndividualProfile SET FirstName = @_FirstName, 
     LastName = @_LastName WHERE Id = @_Id                                
END
GO


CREATE PROCEDURE CreateNewJobAdvert
(@UserId INT, @Title NVARCHAR(255), @CategoryId INT, @RegionId INT, @WorkHoursId INT, 
@Openings INT, @Content NVARCHAR(255), @Active BIT)
AS 
BEGIN
     INSERT INTO Job 
	 VALUES(@UserId, @Title, @CategoryId, @RegionId, 
     @WorkHoursId, @Openings, @Content, @Active, CURRENT_TIMESTAMP)
END
GO


CREATE PROCEDURE EditJobPossition
(@_Title NVARCHAR(255), @_CategoryId INT, @_RegionId INT, @_WorkHoursId INT, 
@_Openings INT, @_Content NVARCHAR(255), @id INT)
AS 
BEGIN
     UPDATE Job SET Title = @_Title, CategoryId = @_CategoryId, 
     RegionId = @_RegionId, WorkHoursId = @_WorkHoursId, Openings = @_Openings, 
     Content = @_Content WHERE Id = @id
END
GO


CREATE PROCEDURE DeleteJobPossition
(@Active BIT, @id INT)
AS 
BEGIN
     UPDATE Job SET Active = @Active  WHERE Id = @id
END
GO


CREATE PROCEDURE GetAdminJobPossitions
AS 
BEGIN
     SELECT Job.Id, Job.Title, CompanyProfile.CompanyName AS CompanyName, Category.[Name] AS Category, 
     Regions.[Name] AS Region, WorkHours.[Name] AS WorkHours, Job.Openings, Job.Active, Job.Created 
     FROM Job INNER JOIN CompanyProfile ON Job.UserId = CompanyProfile.UserId 
     INNER JOIN Category ON Job.CategoryId = Category.Id 
     INNER JOIN Regions ON Job.RegionId = Regions.Id 
     INNER JOIN WorkHours ON Job.WorkHoursId = WorkHours.Id
END
GO


CREATE PROCEDURE GetCompanyJobPossitions
(@Id INT)
AS 
BEGIN
     SELECT Job.Id, Job.Title, CompanyProfile.CompanyName AS CompanyName, Category.[Name] AS Category, 
     Regions.[Name] AS Region, WorkHours.[Name] AS WorkHours, Job.Openings, Job.Active, Job.Created 
     FROM Job INNER JOIN CompanyProfile ON Job.UserId = CompanyProfile.UserId 
     INNER JOIN Category ON Job.CategoryId = Category.Id 
     INNER JOIN Regions ON Job.RegionId = Regions.Id 
     INNER JOIN WorkHours ON Job.WorkHoursId = WorkHours.Id WHERE Job.UserId = @Id
END
GO


CREATE PROCEDURE GetAllIndividualJobPossitions
(@Flag BIT)
AS 
BEGIN
     SELECT Job.Id, Job.Title, CompanyProfile.CompanyName AS CompanyName, Category.[Name] AS Category,
     Regions.[Name] AS Region, WorkHours.[Name] AS WorkHours, Job.Openings, Job.Active, Job.Created
     FROM Job INNER JOIN CompanyProfile ON Job.UserId = CompanyProfile.UserId
     INNER JOIN Category ON Job.CategoryId = Category.Id
     INNER JOIN Regions ON Job.RegionId = Regions.Id 
     INNER JOIN WorkHours ON Job.WorkHoursId = WorkHours.Id WHERE Job.Active = @Flag
END
GO


CREATE PROCEDURE GetCompanySpecificJobPossition
(@Id int)
AS 
BEGIN
     SELECT * FROM Job WHERE Id = @Id
END
GO


CREATE PROCEDURE CheckIfCVExistsInJobsPool
(@JobId INT, @UserId INT)  
AS
BEGIN  
SELECT Id FROM  CompanyJobsCVPool where JobId = @JobId 
AND UserId = @UserId
END 
GO


CREATE PROCEDURE UploadCVJobsPool
(@CompanyId INT,
@UserID INT,
@JobID INT,
@Filename NVARCHAR(255), 
@DescriptiveFileName NVARCHAR(255),
@FileContent VARBINARY(MAX))
AS
BEGIN  
DECLARE @CVDateUploaded NVARCHAR(MAX)
SET @CVDateUploaded = CONVERT (NVARCHAR, CURRENT_TIMESTAMP)
INSERT INTO CompanyJobsCVPool values(@CompanyId, @JobID, @UserId, @Filename, 
CONCAT(@DescriptiveFileName, @CVDateUploaded, '.pdf'), @FileContent, @CVDateUploaded)   
END 
GO


CREATE PROCEDURE UpdateIndividualCVPool
(@Id INT,
@Filename NVARCHAR(255), 
@FileContent VARBINARY(MAX))  
AS
BEGIN  
UPDATE CompanyJobsCVPool SET [Filename] = @Filename, 
FileContent = @FileContent  
WHERE Id = @Id 
END 
GO


CREATE PROCEDURE IndividualCheckIfJobPossitionExists
(@id INT)  
AS
BEGIN  
SELECT Id AS JobId, UserId AS CompanyId FROM Job WHERE Id = @id AND Active = 'True'
END 
GO


CREATE PROCEDURE CompanyCheckIfJobPossitionExists
(@id INT)  
AS
BEGIN  
SELECT UserId FROM Job WHERE Id = @id 
END 
GO


CREATE PROCEDURE CompanyViewAllCVs
(@CompanyId INT)
AS 
BEGIN
     SELECT Job.Id AS JobId, Job.Title, Category.[Name] AS Category, 
	 Regions.[Name] AS Region, WorkHours.[Name] AS WorkHours, Job.Openings, 
	 Job.Active, Job.Created, Users.UserName, Users.Id AS IndividualId
     FROM Job INNER JOIN Category ON Job.CategoryId = Category.Id 
     INNER JOIN Regions ON Job.RegionId = Regions.Id 
     INNER JOIN WorkHours ON Job.WorkHoursId = WorkHours.Id
	 INNER JOIN CompanyJobsCVPool ON Job.Id = CompanyJobsCVPool.JobId
	 INNER JOIN Users ON CompanyJobsCVPool.UserId = Users.Id
	 WHERE Job.UserId = @CompanyId
END
GO


CREATE PROCEDURE DownLoadCVBasedOnId
(@JobId INT, @IndividualId INT)
AS 
BEGIN
     SELECT FileContent, [Filename], DescriptiveFileName 
	 FROM CompanyJobsCVPool 
	 WHERE JobId = @JobId AND UserId = @IndividualId
END
GO

CREATE PROCEDURE DownLoadCVZip
(@CompanyId INT)
AS 
BEGIN
     SELECT FileContent, [Filename], DescriptiveFileName 
	 FROM CompanyJobsCVPool 
	 INNER JOIN Job ON CompanyJobsCVPool.JobId = Job.Id 
	 WHERE CompanyId = @CompanyId AND Active = 'True'
END

	
SELECT * FROM Users
SELECT * FROM Job
SELECT * FROM CompanyProfile
SELECT * FROM IndividualProfile
SELECT * FROM Category
SELECT * FROM Regions 
SELECT * FROM WorkHours
SELECT * FROM CompanyJobsCVPool