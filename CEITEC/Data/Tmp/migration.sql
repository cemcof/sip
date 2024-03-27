
insert into AppUsers (Id, Email, Firstname, Lastname, Affiliation, Identifier, UserName, NormalizedUserName, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, AdditionalEmailContacts)
select Id, Email, Firstname, Lastname, Affiliation, Identifier, Id, upper(Id), upper(Email), 0, '', uuid(), '', '', 0, 0, date(0), 0, 0, '[]'  from _UserEntity
where 1;

insert into Project (Id, Acronym, Title, ParentId, Closed, Discriminator, OwnerId)
SELECT Id, Name, Name, NULL, 0, 'EProject', OwnerId  from Project_1;





# Transfers

insert into Experiment (Id, Access_LinkToken, voltage_base, totalExposedDose, Discriminator, Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, AutopickingModel, Binning, Clean, Cs, DataProcessing, DefocusRangeMax, DefocusRangeMin, FmDose, OriginalDataPath, ParticleDiameter, PhasePlate, PixelSize, PreExpDose, ProcessingDataPath, Type, Voltage, amountAstigmatism, bgRadius, boxSize_CTF, c1aperture, c1lens, c2aperture, c2lens, c3aperture, c3lens, defocusStepSize, extractSize, imagePrefix, imageSizeX, imageSizeY, imageSuffix, minDefocus, minResolution, nominalDefocus, nominalMagnification, objAperture, particleDiameter_base, phasePlate_base, pixelSizeOnImage)
select Id, Access_LinkToken, '', '', 'TransferExperiment', Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, NULL, NULL, Clean, NULL, NULL, NULL, NULL, NULL, OriginalDataPath, NULL, NULL, NULL, NULL, ProcessingDataPath, NULL, NULL, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', NULL, NULL, ''
from _TransferData 
where 1;

insert into ExperimentMetadata (Id, Center, DtCreated, DtStarted, DtFinished, OperatorId, UserId, UserType, AccessRoute, ProjectId, SampleId, Instrument, GeneralNotes, emMicroscopeId, instrumentName, voltage, CS, detectorPixelSize, C2aperture, ObjAperture, C2lens, DataId)
SELECT Id, 'CEMCOF,EM-Instruct-CZ,Brno', DtCreated, DtStarted, DtFinished, OperatorId, UserId, UserType, AccessRoute, ProjectId, SampleId, Instrument, GeneralNotes, '', '', '', '', '', '', '', '', DataId from Experiment_3
where DataId is not null ;

# SPAS
insert into Experiment (Id, Access_LinkToken, voltage_base, totalExposedDose, Discriminator, Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, AutopickingModel, Binning, Clean, Cs, DataProcessing, DefocusRangeMax, DefocusRangeMin, FmDose, OriginalDataPath, ParticleDiameter, PhasePlate, PixelSize, PreExpDose, ProcessingDataPath, Type, Voltage, amountAstigmatism, bgRadius, boxSize_CTF, c1aperture, c1lens, c2aperture, c2lens, c3aperture, c3lens, defocusStepSize, extractSize, imagePrefix, imageSizeX, imageSizeY, imageSuffix, minDefocus, minResolution, nominalDefocus, nominalMagnification, objAperture, particleDiameter_base, phasePlate_base, pixelSizeOnImage)
select Id, Access_LinkToken, '', '', 'SPAExperiment', Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, AutopickingModel, Binning, Clean, Cs, DataProcessing, DefocusRangeMax, DefocusRangeMin, FmDose, OriginalDataPath, ParticleDiameter, PhasePlate, PixelSize, PreExpDose, ProcessingDataPath, NULL, Voltage, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', ''
from _SpaTomoData
where 1;

insert into ExperimentMetadata (Id, Center, DtCreated, DtStarted, DtFinished, OperatorId, UserId, UserType, AccessRoute, ProjectId, SampleId, Instrument, GeneralNotes, emMicroscopeId, instrumentName, voltage, CS, detectorPixelSize, C2aperture, ObjAperture, C2lens, DataId)
SELECT Id, 'CEMCOF,EM-Instruct-CZ,Brno', DtCreated, DtStarted, DtFinished, OperatorId, UserId, UserType, AccessRoute, ProjectId, SampleId, Instrument, GeneralNotes, '', '', '', '', '', '', '', '', SPAExperiment_DataId from Experiment_3
where Experiment_3.SPAExperiment_DataId is not null ;


insert into Experiment (Id, ExperimentMetadataId, Access_LinkToken, voltage_base, totalExposedDose, Discriminator, Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, AutopickingModel, Binning, Clean, Cs, DataProcessing, DefocusRangeMax, DefocusRangeMin, FmDose, OriginalDataPath, ParticleDiameter, PhasePlate, PixelSize, PreExpDose, ProcessingDataPath, Type, Voltage, amountAstigmatism, bgRadius, boxSize_CTF, c1aperture, c1lens, c2aperture, c2lens, c3aperture, c3lens, defocusStepSize, extractSize, imagePrefix, imageSizeX, imageSizeY, imageSuffix, minDefocus, minResolution, nominalDefocus, nominalMagnification, objAperture, particleDiameter_base, phasePlate_base, pixelSizeOnImage)
select TD.Id, E2.Id, Access_LinkToken, '', '', 'TransferExperiment', Access_InviteToken, Access_Label, Access_Path, Access_PublicUrl, Access_Space, Archive, NULL, NULL, Clean, NULL, NULL, NULL, NULL, NULL, OriginalDataPath, NULL, NULL, NULL, NULL, ProcessingDataPath, NULL, NULL, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', NULL, NULL, ''
from Experiment_3 E2 inner join _TransferData TD on TD.Id = E2.DataId
where 1;



