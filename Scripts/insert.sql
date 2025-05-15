
INSERT INTO Device (Id, Name, IsEnabled) VALUES
                                             ('E-1', 'Factory Controller', 1),
                                             ('P-1', 'Office PC', 1),
                                             ('SW-1', 'Smart Watch Pro', 1),
                                             ('E-2', 'Production Line Sensor', 0),
                                             ('P-2', 'Developer Laptop', 1),
                                             ('P-3', 'Fitness Tracker', 1),
                                             ('E-3', 'Network Router', 1);


INSERT INTO Embedded (IpAddress, NetworkName, DeviceId) VALUES
                                                            ('192.168.1.1', 'factory-controller-1', 'E-1'),
                                                            ('10.0.0.1', 'sensor-prod-line-1', 'E-2'),
                                                            ('172.16.0.1', 'main-router', 'E-3');


INSERT INTO PersonalComputer (OperationSystem, DeviceId) VALUES
                                                             ('Windows 10 Pro', 'P-1'),
                                                             ('Ubuntu 20.04 LTS', 'P-2'),
                                                             ('Windows 11 Home', 'P-3');


INSERT INTO Smartwatch (BatteryPercentage, DeviceId) VALUES
                                                         (85, 'SW-1');
