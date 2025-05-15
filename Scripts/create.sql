

CREATE TABLE Device (
                        Id VARCHAR(255) PRIMARY KEY,
                        Name NVARCHAR(255),
                        IsEnabled BIT,
                        RowVersion ROWVERSION

);

CREATE TABLE Embedded (
                          Id INT PRIMARY KEY IDENTITY(1,1),
                          IpAddress VARCHAR(255),
                          NetworkName VARCHAR(255),
                          DeviceId VARCHAR(255),
                          FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);

CREATE TABLE PersonalComputer (
                                  Id INT PRIMARY KEY IDENTITY(1,1),
                                  OperationSystem VARCHAR(255),
                                  DeviceId VARCHAR(255),
                                  FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);

CREATE TABLE Smartwatch (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            BatteryPercentage INT,
                            DeviceId VARCHAR(255),
                            FOREIGN KEY (DeviceId) REFERENCES Device(Id)
);