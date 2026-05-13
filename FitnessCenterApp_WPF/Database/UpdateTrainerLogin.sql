USE Fitness_Center;
GO

IF COL_LENGTH('Trainer', 'Email') IS NULL
BEGIN
    ALTER TABLE Trainer ADD Email VARCHAR(100) NULL;
END
GO

IF COL_LENGTH('Trainer', 'Password') IS NULL
BEGIN
    ALTER TABLE Trainer ADD [Password] VARCHAR(255) NULL;
END
GO

UPDATE Trainer
SET Email = CONCAT('trainer', ID_Trainer, '@fitness.ru'),
    [Password] = 'Trainer@123'
WHERE Email IS NULL OR [Password] IS NULL;
GO
