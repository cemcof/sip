INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('ImpersonatorRole', null, 'Impersonator role', 'User with this role can impersonate as other users within the system or use arbitrary claims. This is essentially a god mode role and can be granted only directly in the database.');
INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('LabOperatorRole', null, 'Lab operator role', '');
INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('LabUserRole', null, 'Lab user role', '');
INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('RemoteMicAdminRole', null, 'Remote mic admin role', '');
INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('RemoteMicUserRole', null, 'Remote mic user role', '');
INSERT INTO `lims-staging`.AppRole (Id, ParentId, Name, Description) VALUES ('UserAdminRole', null, 'User admin role', 'User with this role can manage other users within the system');
