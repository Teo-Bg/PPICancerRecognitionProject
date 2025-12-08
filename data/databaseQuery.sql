CREATE TABLE Patients (
    PatientID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL
);
GO


CREATE TABLE CTScans (
    ScanID INT IDENTITY(1,1) PRIMARY KEY,
    PatientID INT NULL,                
    FileCode NVARCHAR(255) NOT NULL,   
    RelativePath NVARCHAR(500) NOT NULL,
    ScanDate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_CTScans_Patients
        FOREIGN KEY (PatientID) REFERENCES Patients(PatientID)
        ON DELETE SET NULL 
);
GO

CREATE TABLE AI_Model_Outputs (
    OutputID INT IDENTITY(1,1) PRIMARY KEY,
    ScanID INT NOT NULL,              
    FileCode NVARCHAR(255) NOT NULL, 
    RelativePath NVARCHAR(500) NOT NULL,
    GenerationDate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_AIOutputs_CTScans
        FOREIGN KEY (ScanID) REFERENCES CTScans(ScanID)
        ON DELETE CASCADE 
);
GO

CREATE INDEX IX_CTScans_PatientID ON CTScans(PatientID);
CREATE INDEX IX_AIOutputs_ScanID ON AI_Model_Outputs(ScanID);
GO


USE MedicalImagingDB;
GO


ALTER TABLE AI_Model_Outputs
    ADD PredictedTypes NVARCHAR(MAX) NULL,
        PredictedClass NVARCHAR(255) NULL,
        TypeProbabilities NVARCHAR(MAX) NULL,
        ClassProbabilities NVARCHAR(MAX) NULL;
GO

SELECT COLUMN_NAME

FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AI_Model_Outputs'
ORDER BY COLUMN_NAME;