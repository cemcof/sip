INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19001', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-01-10 00:00:00.000000', '2019-01-10 00:00:00.000000', '2019-01-27 00:00:00.000000', '2019-01-10 00:00:00.000000', 'Tecnai F20', 'Gun Tilt alignment - there is no apparent
minimum in the midle of one of the GT axes', 'LM: the system evolves, this should not be considered constant. 
If unsure, do the alignment on side lobes
JN: use focused beam to find minimum', 'Resolved', 'Auto', '2019-01-10 00:00:00.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19002', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-01-18 00:00:00.000000', '2019-01-18 00:00:00.000000', '2019-01-27 00:00:00.000000', '2019-01-18 00:00:00.000000', 'Titan Krios', 'Corrosion of connector of PPal', 'LM: Only related to plastic cap, pins are OK.', 'Resolved', 'Auto', '2019-01-18 00:00:00.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19003', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-02-02 00:00:00.000000', '2019-02-02 00:00:00.000000', '2019-05-29 08:59:14.000000', '2019-02-02 00:00:00.000000', 'Titan Krios', '"The computer randomly crashes. 
The event log was inspected and the crashes 
did not have consistent reasons.

Reaction of smbd who inspected the core dumps:
 possible SMP exploit in MS Windows, do anti-malware scan and win upd.
Service personell (Bernd Kraus) recommended:
0) backup folders:
 Program Files[.*]\\Gatan
 ProgramData\\Gatan
 D:\\Gatan\\Sw&Ins\\...
1) install windows defender, update it and run full scan
2) do full windows update (recommended settings)
3) write an e-mail about the results to service
This was performed. No issues were found.

Should the defender be left active (originally nor present to not interefere with data collection)?

Additional info in e-mail sent to gatan and JN


Their remote control SW only requires ports 443 and 80, 
connected to the active socket for FEI service,
used the metallic connection for FEI PC (sth. like TEM network)
had to temporarily change the IP address:
 original: 192.168.012.002, mask: FF.FF.FF.00, others are empty
 new: DHCP"', 'system update + re-installation', 'Resolved', 'Auto', '2019-02-02 00:00:00.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19004', null, null, '2019-04-26 10:54:42.000000', '2019-04-26 10:54:42.000000', '2019-09-10 07:22:48.000000', '2019-04-26 10:54:42.000000', 'Incubator', 'installation of the instrument', 'Je vyreseno', 'Resolved', 'Auto', '2019-04-26 10:54:42.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19005', null, null, '2019-04-26 10:57:12.000000', '2019-04-26 10:57:12.000000', '2019-09-10 07:22:42.000000', '2019-04-26 10:57:12.000000', 'Flow box', 'installation of the instrument - adjustment of the electricity wiring on the wall before the we bring the instrument to full operation', 'Je vyreseno', 'Resolved', 'Auto', '2019-04-26 10:57:12.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19006', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-05-06 14:07:00.000000', '2019-05-06 14:07:00.000000', '2019-05-06 14:08:45.000000', '2019-05-06 14:07:00.000000', 'Titan Krios', 'Talk v projekce se zhorsuje, koreluje s pohybem stinitka.', '190502 Lukas vymenil hlavni o-krouzek stinitka.', 'Resolved', 'Auto', '2019-05-06 14:07:00.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19007', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-05-07 07:25:25.000000', '2019-05-07 07:25:25.000000', '2019-05-07 07:28:16.000000', '2019-05-07 07:25:25.000000', 'Talos Arctica', 'Velke vykyvy v emisnim proudu, nekolik DARu odeslano Lukasovi
Nahle se zvysila jasnost zdroje, emisni proud stoupl o ~ 3/4
Zrejme se neco udalo v GUNu', '190426 - LM:
Mam zpravu od kolegu, ze podobne chovani bylo jiz evidovano jinde, a jedna se s nejvetsi pravdepodobnosti o chybu odectu emisniho proudu, nebo neco s pripojenim zdroje extraktoru. Dokud ovsem nepozorujete vliv na vysledky z mikroskopu, nedoporucuje se to resit. Znamenalo by to vypusteni SF6, kontrolu elektroniky, zavreni a napusteni, bez zaruky na efekt.

LM dodatecny ustni komentar:
stav tipu lze overit merenim SpotSize dependent Beam Current ... ? Nebylo udelano, lze jenom v servisnim rezimu, Lukas udela prilezitostne pozdeji.', 'Resolved', 'Auto', '2019-05-07 07:25:25.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19008', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-05-14 16:40:14.000000', '2019-05-14 16:40:14.000000', '2019-05-27 08:10:35.000000', '2019-05-14 16:40:14.000000', 'Vitrobot', '- polystyrene cup broken - LN2 leaking into the ethane cup area
- Lukas Macek contacted 14th May  - will order two polystyrene cups', 'cap delivered on 15th May 2019', 'Resolved', 'Auto', '2019-05-14 16:40:14.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19009', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-05-27 08:11:02.000000', '2019-05-27 08:11:02.000000', '2019-06-03 09:52:15.000000', '2019-05-27 08:11:02.000000', 'Titan Krios', 'Issues with samples reloading on Titan Krios. The following error message issued: Load cartridge failed. The stage is not available: Stage usage is restricted: reason unknown.
The problem was observed on 25th May 2019.

Current workaround: reset stage before reloading specimen

current status: DAR generated and sent to Lukas Macek', 'LM: Ahoj Michale,
Prave jsem konzultoval s Autoload expertem a je to znama vec. Nevim jestli to presne vysvetlim, ale je to zase sw problem. Jde o to, ze se informace o stavu stage dostava do autoloadru se zpozdenim. Typicky by k tomu melo dochazet pouze pri opakovanem loadovani rychle za sebou. Stage je sice plne fungcni, enablovany a pripraven k prejezdu do loadovaci polohy, ale autoloader tu to informaci jeste nema a hlasi error. Doufam, ze vas to v praci nebude moc brzdit, ale neni to jak v tuto chvili resit. ', 'Resolved', 'Auto', '2019-05-27 08:11:02.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19010', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-05-29 08:54:20.000000', '2019-05-29 08:54:20.000000', '2019-05-29 08:57:51.000000', '2019-05-29 08:54:20.000000', 'Tecnai F20', 'objective aperture doesn''t insert into field of view (even in the low mag).
 - Lukas Macek contacted', 'stopping screw on the aperture insertion mechanism loosen to allow full insertion of the aperture mechanism into electron beam position', 'Resolved', 'Auto', '2019-05-29 08:54:20.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19011', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-05-29 08:59:50.000000', '2019-05-29 08:59:50.000000', '2019-05-31 08:41:06.000000', '2019-05-29 08:59:50.000000', 'Vitrobot', 'humidifier in Vitrobot doesn''t work (no mist observed in the chamber)
 - Lukas Macek contacted', 'LM did preventive maintenance. The plastic receptor for humidifier cylinder was replaced (damaged by ultrasound). The interior of the humidifier was dirty, it was also cleaned mechanically. All tests OK.', 'Resolved', 'Auto', '2019-05-29 08:59:50.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19012', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-03 09:52:48.000000', '2019-06-03 09:52:48.000000', '2019-06-03 09:54:06.000000', '2019-06-03 09:52:48.000000', 'Versa 3D', 'On Monday morning, the vacuum in prep chamber was bad. According to SW, the TMP was ON, but was 0 RPM.', 'System restarted, so far everything looks OK.', 'Resolved', 'Auto', '2019-06-03 09:52:48.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19013', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-06 10:46:13.000000', '2019-06-06 10:46:13.000000', '2019-06-20 14:56:15.000000', '2019-06-06 10:46:13.000000', 'Talos Arctica', 'Casette temperature sensor is in error. Ask LM
Update 190606: LM arrived. It is needed to first inspect the AL. This has to be done when it''s warm. As the system is still working, this should be scheduled together with next cryocycle.
190614 - cassette stuck in Autoloader. (Samples difrosted). LM opened the autoloader, scratches observed on the cassette arm, cassette arm runs OK no change done to system
190615 - replacement of temperature sensor wiring required - part ordered', 'AL dismounted, possible problem localised and solved. During the service, AL internal board with connectors had to be replaced too.
Should be now monitored. So far (190620) works OK.', 'Resolved', 'Auto', '2019-06-06 10:46:13.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19014', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-06 10:51:26.000000', '2019-06-06 10:51:26.000000', '2019-06-06 10:52:49.000000', '2019-06-06 10:51:26.000000', 'Vitrobot', 'Vitrobot unexpectedly unresponsive, will not boot. Tried multiple times. LM (at that time present) checked it, no visible problem, during inspection, it started to work again.', 'At this moment, considered solved...', 'Resolved', 'Auto', '2019-06-06 10:51:26.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19015', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-06 10:52:53.000000', '2019-06-06 10:52:53.000000', '2020-05-28 14:20:40.000000', '2019-06-06 10:52:53.000000', 'Talos Arctica', 'Dirt was observed in low CameraLenght in D mode. MB will try to localize it, if it persisted after cryocycle. In case it is still present, LM will inspect it on next visit.

LM: Probably close to lower column valve. Will be dealt with occasionally (when the column is warmed up), as it does not influence the data collection.', 'Disappeared sontaneously
', 'Resolved', 'Auto', '2019-06-06 10:52:53.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19017', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-06-12 05:43:13.000000', '2019-06-12 05:43:13.000000', '2019-12-09 07:58:38.000000', '2019-06-12 05:43:13.000000', '1S118', 'high temperature and humidity in the laboratory.
190611 - P. Mokros contacted to regulate the cooling unit
190619 - the unit can only cool down the air, but the consequent heating is disabled due to the fact that heating water is off in Summer at the whole campus, system switched back to auto mode, will re-visit the issue after P. Saranchuk is back from vacation (190624).
- no response from SUKB for several month.', 'The issue will be solved during facility reconstruction.', 'Resolved', 'Auto', '2019-06-12 05:43:13.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19018', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-06-12 05:45:28.000000', '2019-06-12 05:45:28.000000', '2019-12-09 07:46:11.000000', '2019-06-12 05:45:28.000000', '1S116', 'high humidity in the microscope laboratories
190611 - temperature and humidity monitoring
190612 - the cooling of the air unit delivering fresh air to precise clima control units set to maximum
190613 - fresh air inlet for Krios and Arctica sealed with tape
190613 - manual operation of the precise clima control units enabled', 'the units under facility control, the chiller at the ceiling of 1S114 turned of, external AC unit for 1S114 set to state where it keeps the temperature at ~24.5C in 1S114  ', 'Resolved', 'Auto', '2019-06-12 05:45:28.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19019', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-20 14:53:03.000000', '2019-06-20 14:53:03.000000', '2019-06-20 14:55:10.000000', '2019-06-20 14:53:03.000000', 'AirCon unit', 'Maintenance', 'changed filters on AC: Arctica, Krios, Vers
new humidifier installed to Arctica
refurbished humidifier installed to Vers
increased prop. gains for dehumidification', 'Resolved', 'Auto', '2019-06-20 14:53:03.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19020', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-20 14:54:06.000000', '2019-06-20 14:54:06.000000', '2019-06-20 15:48:00.000000', '2019-06-20 14:54:06.000000', 'Titan Krios', 'Grid lost, Martin Polak, AL position 3. LM contacted. Discussed. Autogrid observed on stage, gripper empty.', 'Autogrid base retrieved from stage. Clip ring and grid location unknown.', 'Resolved', 'Auto', '2019-06-20 14:54:06.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19021', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-20 14:57:37.000000', '2019-06-20 14:57:37.000000', '2019-07-03 07:57:31.000000', '2019-06-20 14:57:37.000000', 'other', 'Papouch connectivity lost. CIT contacted.', 'is still on DHCP, but works. Reason unknown', 'Resolved', 'Auto', '2019-06-20 14:57:37.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19022', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-20 14:58:48.000000', '2019-06-20 14:58:48.000000', '2020-05-28 14:21:14.000000', '2019-06-20 14:58:48.000000', 'AirCon unit', 'Discussion with Peter Mokros: IF the inlets for fresh airs are disabled for Krios and Arctica, even thought dehumidifier power is low, the humidity should slowly drop down. This is not the case. Ergo, there must be some other source of humidity in the laboratories.
Possibilieties:
Leakage in the A/C tubings
External air brought to laboratory via safety (O2 / SF6 tubes)
Leakage under / around the door
Any other?

Could we perform any test? Possibly odorant test?

Probably irrelevant now? 200528?
', '', 'InProgress', 'Auto', '2019-06-20 14:58:48.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19023', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-06-20 15:00:05.000000', '2019-06-20 15:00:05.000000', '2020-05-28 14:19:56.000000', '2019-06-20 15:00:05.000000', 'other', 'borowed grids from Virology:

10 boxes of QF Cu200 R2/1
10 boxes of QF Cu300 R2/1
4 boxes of QF Au??? R2/4 ???? (RD)

190716 Ordered first batch of grids', 'Returned
', 'Resolved', 'Auto', '2019-06-20 15:00:05.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19024', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-07-03 07:57:42.000000', '2019-07-03 07:57:42.000000', '2019-07-03 07:58:39.000000', '2019-07-03 07:57:42.000000', 'Talos Arctica', '190702 evening: optic boards degraded without obvious reason.', ' Recover successful.', 'Resolved', 'Auto', '2019-07-03 07:57:42.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19025', '08d9aa96-3527-450c-8a33-fedf5c637bf1', null, '2019-07-03 08:58:24.000000', '2019-07-03 08:58:24.000000', '2019-07-03 09:00:42.000000', '2019-07-03 08:58:24.000000', 'Titan Krios', 'When IS calib is requested, error message appears (approx.): cannot invert the matrix
IS calib cannot be done, the same for atlas acq. ', 'check that reported pixel size in preview image has reasonable value
possibly restart TIA and take an image(s) in preview before retrying IS calib.', 'Resolved', 'Auto', '2019-07-03 08:58:24.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19026', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-07-16 15:10:14.000000', '2019-07-16 15:10:14.000000', '2019-07-16 16:05:16.000000', '2019-07-16 15:10:14.000000', 'Talos Arctica', 'Tibor Fuzik reported behavior of stage that is similar to tracking errors on Krios, except no error is reported.

This happens at ~ atlas mags at lower part of the screen, the motors move with very high speed suddenly.


Wim Hagen EMBL Heidelberg [4:00 PM]
Just had a stage tracking error, happens every now and then on Krios, Arctica and probably also Glacios. The UI does not give an error (which is a bug!), it’s only throwing errors when run through scripting (e.g. SerialEM). It''s a known issue: hanging Y measurement decoder, it just gets stuck when going to extreme negative Y positions (al the way to the left). Just move it back and forth a few times and it’s loose again for some months.', 'The ruler (axis inside/outside column) was indeed stuck at position cca -890 um, gently moved, now works fine.

', 'Resolved', 'Auto', '2019-07-16 15:10:14.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19027', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-07-16 15:11:28.000000', '2019-07-16 15:11:28.000000', '2019-09-10 07:56:45.000000', '2019-07-16 15:11:28.000000', 'Titan Krios', 'Tibor Fuzik reported again the necessity of restart of K2 system because of detector segment off.

Did we ever discuss this with Gatan service?

+ did we ask about possibility of using "previous" gain refs. and just reupload them whereever appropriate?
+ ask about the slit alignment procedures, maybe rather on Slack !?!?!?!?!

same problem repeated on 20th July  - soft restart (only digitizer) did not work. It was necessary to reboot the system completely

- go ahead, contact Gatan
', '', 'InProgress', 'Auto', '2019-07-16 15:11:28.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19028', null, '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-07-16 16:17:11.000000', '2019-07-16 16:17:11.000000', '2019-07-24 06:53:11.000000', '2019-07-16 16:17:11.000000', 'Versa 3D', 'Vyt Vykoukal, Misa Prochazkova: podezreni, ze jeden z heateru na verze ma poruchu

', 'both heaters worked without any issues during week 22nd-27th July. Complex service of the Quorum system is planned for 8th-9th August 2019', 'Resolved', 'Auto', '2019-07-16 16:17:11.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19029', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-07-24 06:54:06.000000', '2019-07-24 06:54:06.000000', '2019-07-26 06:06:43.000000', '2019-07-24 06:54:06.000000', 'Versa 3D', 'microscope stage cannot move. Inspection in service software revealed "Encoder interpolator error" for R axis

service contacted (workaround: disabling R axis in TAD)', 'there was a contamination on the sensor. Removed. The way to get to the sensor is to take out the three screws on the top of the stage, then take out additional three screws (torx) and remove the stage disc. The sensor is right under the disc.', 'Resolved', 'Auto', '2019-07-24 06:54:06.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19030', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-08-21 11:56:13.000000', '2019-08-21 11:56:13.000000', '2019-09-10 07:15:01.000000', '2019-08-21 11:56:13.000000', '1S114', 'Doslo k poskozeni paru zasuvka / vidlice pro chladak Titan Krios

', '190822 Elektrikar zasuvku vymenil. ', 'Resolved', 'Auto', '2019-08-21 11:56:13.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19031', null, '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-08-29 07:30:12.000000', '2019-08-29 07:30:12.000000', '2019-09-18 08:35:45.000000', '2019-08-29 07:30:12.000000', 'Versa 3D', 'Stage heater "terribly slow"
Should be consulted with Quorum service among other things', 'The heating wire is broken inside the stage. It would be necessary to send the stage to Quorum for repair. There is no need to use the heater for warming up the stage. The recommended procedure is to turn the flow to 5l/min and take the CHE3000 out of the dewar. The stage and cold finger inside the microscope will return to RT within 15mins.', 'Resolved', 'Auto', '2019-08-29 07:30:12.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19032', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-09-09 16:30:53.000000', '2019-09-09 16:30:53.000000', '2019-09-09 16:37:55.000000', '2019-09-09 16:30:53.000000', 'Titan Krios', '+ also Talos Arctica:
SW upgrades, Gatan filter leakage, advanced scripting licenses', 'Both instruments upgradet to suites .15.1
Few problems during installation, had to upgrade firmwares (notably Vacuum)
On Krios, typical problem with boards occured.
Staged homed only on second attempt.
EPU was also upgraded to 2.4.x on both.
Both are now prepared for advscript licenses, but not tested, as not yet officially licensed (being solved)

Gatan filter completely replaced.

Sherpa working
AutoCTF throwing error, but will probably work after condig file adjustment.

TODO: AFIS?? abberation freee image shift - should be aligned by service ?!?!?!?!?!

Time on TA adjusted

', 'Resolved', 'Auto', '2019-09-09 16:30:53.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19033', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2019-09-10 07:08:13.000000', '2019-09-10 07:08:13.000000', '2019-09-10 07:11:13.000000', '2019-09-10 07:08:13.000000', 'Titan Krios', 'AdvScript Licensinsg still unknown
Acquisition servers may still need upgrades
GMS upgrade?
AutoCTF issue - btw, autoCTF is no longer supported
Sherpa upgrades???
AFIS setup? by service enngineer - Lukas!

Note down new versions of sotware. Can this be automated?', '', 'InProgress', 'Auto', '2019-09-10 07:08:13.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19034', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-09-10 07:11:35.000000', '2019-09-10 07:11:35.000000', '2019-12-09 07:56:50.000000', '2019-09-10 07:11:35.000000', '1S114', 'Kvuli problemum se zasuvkou 230V bylo pozorovano poskozeni (teplem) optickych kabelu. JN resi s firmou, ktera instalovala optiku.

Ve ctvrtek 26.9. probehne promereni jednotlivych vlaken a cilem zjistit mnozstvi funkcnich a nefunkcnich spojeni.

Dostali jsme nabidku na opravu v hodnote 46tis. Kc. 
', 'Realizace prozatim odsunuta na neurcito z duvodu vysokych financnich nakladu.', 'Resolved', 'Auto', '2019-09-10 07:11:35.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19035', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-09-18 08:36:27.000000', '2019-09-18 08:36:27.000000', '2019-09-23 14:54:13.000000', '2019-09-18 08:36:27.000000', '1S106', 'The UPS in error status with message "Transfer impossible". The system runs on by-pass. The service visit scheduled for 18th September.

Update 18th September: The service did not manage to recover the UPS back to full operation. Possible issue with UPS control unit which was already replaced before. The system will be service by another subcontractor. UPS is running on bypass.

THE FACILITY IS CURRENTLY RUNNING WITHOUT ANY ELECTRICITY BACKUP, ANY ISSUES WITH ELECTRICITY WILL RESULT IN COMPLETE BLACK-OUT', 'hard restart of the UPS solved the issue.', 'Resolved', 'Auto', '2019-09-18 08:36:27.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('19036', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2019-12-09 07:58:50.000000', '2019-12-09 07:58:50.000000', '2020-02-26 07:19:33.000000', '2019-12-09 07:58:50.000000', 'Talos Arctica', 'one of the nanocabs broken. LM was notified with request for replacement.', 'NanoCab arrived 20th December and is currently in use. The old one is in JN''s office.', 'Resolved', 'Auto', '2019-12-09 07:58:50.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20001', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-01-07 09:51:51.000000', '2020-01-07 09:51:51.000000', '2020-05-28 14:21:44.000000', '2020-01-07 09:51:51.000000', 'Chiller', 'When Arctica chiller is turned off, level rises. Suspicion about leak of air into the tubing. On next poweroff inspect the auto-venting stuff, eventually close them permanently. Consult with P. Mokros before this happens.

STILL UNSOLVED, MERGE WITH SERVICE INDUCED SHUTDOWN', '', 'InProgress', 'Auto', '2020-01-07 09:51:51.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20002', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-01-07 09:53:29.000000', '2020-01-07 09:53:29.000000', '2020-01-12 16:58:56.000000', '2020-01-07 09:53:29.000000', 'other', 'UPS - requested a training for UPS tests without SUKB presence.
Pokorna and Marcolla involved', 'There is another blackout scheduled for (mostly probably) february, will be performed during the blackout.
The request to train sbd (Mokros, Novacek, Babiak) to be able to perform the test was rised (Mokros and Marcolla involved)', 'Resolved', 'Auto', '2020-01-07 09:53:29.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20003', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2020-01-12 16:24:02.000000', '2020-01-12 16:24:02.000000', '2020-02-27 08:38:58.000000', '2020-01-12 16:24:02.000000', 'Tecnai F20', 'Optical network card

SolarFlare optical card from packet grabber is PCI-e 8x card

Slots in F20 PC: 
  PCI-e x16 GPU
  PCI-e 4x  free
  PCI-e 1x  free
  PCI-e 1x  free
  PCI       Metallic network
  PCI       free
  PCI       free
', 'je poptano u firem, jestli umi dodat 4 lanes optiku s 10GiB', 'Resolved', 'Auto', '2020-01-12 16:24:02.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20004', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-01-12 16:36:58.000000', '2020-01-12 16:36:58.000000', '2020-01-12 16:37:43.000000', '2020-01-12 16:36:58.000000', 'Titan Krios', 'Replacement of the Falcon filter', 'Falcon filter was replaced 200111, other 4 new filters left in Krios room.', 'Resolved', 'Auto', '2020-01-12 16:36:58.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20005', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-01-27 11:14:37.000000', '2020-01-27 11:14:37.000000', '2020-01-27 11:15:56.000000', '2020-01-27 11:14:37.000000', 'Talos Arctica', 'Dirt in low CameraLenght in diffraction mode.
JN consulted this with FEI people, probably it will be lose to lower column valve.

Also, new noises are heard when AL TMP is starting / stopping.', 'LM will deal with this 2.3.
', 'Resolved', 'Auto', '2020-01-27 11:14:37.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20006', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-01-29 14:06:23.000000', '2020-01-29 14:06:23.000000', '2020-01-29 14:09:22.000000', '2020-01-29 14:06:23.000000', '1S114', 'Kapani vody na Chiller Arcticy

Chiller odsunut nabok. V teto poloze muze zustat.', 'koncem ledna konstatoval pan (delegoval P. Mokros), ze filtr pro odvod kondenzatu byl ucpan. Toto bylo opraveno. Zaroven konstatoval, ze v tepelne izolaci kolem dotcene trubky byl jenom maly objem rezidualniho kondenzatu.

S ohledem na material potrubi (Cu?) zrejme nehrozi koroze.

S ohledem na dlouhodobejsi (?) odstaveni klimatizace mistnosti by nemelo hrozit pribyvani kondenzatu.

Budeme (CEMCOF) ted sledovat.', 'Resolved', 'Auto', '2020-01-29 14:06:23.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20007', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-02-06 13:41:51.000000', '2020-02-06 13:41:51.000000', '2020-02-06 13:44:46.000000', '2020-02-06 13:41:51.000000', 'Talos Arctica', 'Software crash (insuff. RAM) during AL operation, AL ended in unreasonable state.
tools finaly used: both cockpit and tad
Checked the cartrdige gripper, grid correctly positioned in it.
Manually replaced the casette with empty one.
Manually moved the autogrid into empty casette.
casette arm should be parked during autogrid movement
Do not forget to restore the cartridge gripper state!', 'Solved successfully, some time was needed to let everything cool down.

It seems that one more grid may be lost in AL (pos 5)', 'Resolved', 'Auto', '2020-02-06 13:41:51.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20008', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-02-27 08:39:03.000000', '2020-02-27 08:39:03.000000', '2020-02-27 08:39:53.000000', '2020-02-27 08:39:03.000000', 'Titan Krios', 'APM', 'dle LM (20022x) bychom to meli mit, je na to potreba licenci, ktera se jmenuje
TEM direct alignments (?!?!?!)

je v reseni', 'Resolved', 'Auto', '2020-02-27 08:39:03.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20009', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '2020-03-17 11:47:47.000000', '2020-03-17 11:47:47.000000', '2020-04-17 06:05:01.000000', '2020-03-17 11:47:47.000000', '1S106', 'An error message on UPS unit. The INVERTOR is by-passed. Any power-outage will cause shut-down of all microscopes.', 'the service engineer just restarted the UPS and it went back to normal state', 'Resolved', 'Auto', '2020-03-17 11:47:47.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20010', null, '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-04-06 07:30:47.000000', '2020-04-06 07:30:47.000000', '2020-04-06 07:35:19.000000', '2020-04-06 07:30:47.000000', 'Talos Arctica', 'Ventil na XL240 nejde uzavrit. Bylo zjisteno, ze plastove tesneni na kapalinovem ventilu ma povytazene a znicene plastove tesneni. Po konzultaci s p. Poulem bylo domluveno, ze bude uznano jako reklamace a ze posle dva nahradni ventily (oba modre).', 'Kapalinovy ventil byl vymenen.', 'Resolved', 'Auto', '2020-04-06 07:30:47.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20011', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-04-06 07:30:49.000000', '2020-04-06 07:30:49.000000', '2020-04-06 12:04:53.000000', '2020-04-06 07:30:49.000000', 'Talos Arctica', 'JN: o weekendu kolize v AL; MB schladi a bude testovat inventory a detekci sitek.

', 'MB:
naloadovano pozice 1 3 5 7  9 11 - load a unload vsech pozic OK
naloadovano pozice 2 4 6 8 10 12 - load a unload vsech pozic OK', 'Resolved', 'Auto', '2020-04-06 07:30:49.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20012', '08d9aa96-3527-450c-8a33-fedf5c637bf1', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-05-28 14:16:52.000000', '2020-05-28 14:16:52.000000', '2020-05-28 14:17:27.000000', '2020-05-28 14:16:52.000000', 'other', 'CEMCOF1: unexpected reboots', 'both PSUs changed 200528', 'Resolved', 'Auto', '2020-05-28 14:16:52.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20013', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-07-08 08:48:54.000000', '2020-07-08 08:48:54.000000', '2021-02-12 12:55:51.000000', '2020-07-08 08:48:54.000000', 'Tecnai F20', 'camera segment fail
happened between 1:24 and 2:06 5.7.2020', 'no longer relevant', 'Resolved', 'Auto', '2020-07-08 08:48:54.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('20014', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2020-09-02 13:18:51.000000', '2020-09-02 13:18:51.000000', '2021-02-12 12:55:39.000000', '2020-09-02 13:18:51.000000', 'Talos Arctica', 'AL - problems with casette retrieval ', 'LM: problems with casette retrieval may be connected to the sensor of presence of casette at the cassette gripper (P. Malek), when the system is warm, the response of the sensor should be checked', 'Resolved', 'Auto', '2020-09-02 13:18:51.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('21001', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2021-02-10 08:07:04.000000', '2021-02-10 08:07:04.000000', '2021-02-12 12:42:25.000000', '2021-02-10 08:07:04.000000', 'other', 'licensing (EPU etc.) reacts to HW changes with one month delay (need to check state in windows event log)', 'perform HW upgrade in licensing', 'Resolved', 'Auto', '2021-02-10 08:07:04.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('21002', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2021-02-12 12:42:27.000000', '2021-02-12 12:42:27.000000', '2021-02-12 12:49:37.000000', '2021-02-12 12:42:27.000000', 'other', 'Talos Arctica and Talos F200:
after optic boards degradation and successful recover action, the dose protector remains in error state (calculation error).
This was reported:

Dear support team,

occasionally we are observing problems with optic boards (Optic boards
degraded) on Talos Arctica instrument. This happens randomly (approximately 4-10 times a year, last time today around 14:00) and we have confirmed that it is not related to water flow. When this happens, we are able to recover the state of the optic boards, however, dose protector always remains in error state (calculation error).

We haven''t found any other way how to deal with this, than restart of TEM server. Could you please advice if there is any known procedure to somehow reinitialize the dose protector?

If not, is there any chance of diagnosing the causes of optics boards degradation?', 'EU111187 || new cm call || MU CEITEC Talos G2 9950740

Suggested solution:
1) Disable and enable all apertures
2) Slightly change and return the HT (-/+) kV
3) If still not OK: reload FEG registers
4) If still not OK: reload big alignments', 'Resolved', 'Auto', '2021-02-12 12:42:27.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('21003', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '08d9be4b-1700-49e2-89a8-449680dfa0b7', '2021-09-01 08:07:20.000000', '2021-09-01 08:07:20.000000', '2021-09-01 08:09:13.000000', '2021-09-01 08:07:20.000000', 'Tecnai F20', 'Sediment in Quadro cooling system flowmeter.
', 'Flushed 210901.
Babiak
', 'Resolved', 'Auto', '2021-09-01 08:07:20.000000', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('21004', '08d9b9d6-4a41-4a01-82bc-97e12a89f653', '08d9b9d6-4a41-4a01-82bc-97e12a89f653', '2021-12-14 12:07:35.435737', '2021-12-14 12:08:24.591160', '2021-12-14 12:08:24.591132', '2021-12-14 12:08:24.591161', 'Test issue ', 'Testovaci issue', '', 'Initiated', 'Auto', '2021-12-14 12:08:25.992680', 7);
INSERT INTO lims.Issue (Id, InitiatedById, ResponsibleId, DtObserved, DtCreated, DtLastChange, DtAssigned, Subject, Description, SolutionDescription, Status, Urgency, DtLastNotified, NotifyIntervalDays) VALUES ('21005', '08d9b9d6-4a41-4a01-82bc-97e12a89f653', '08d9b9d6-4a41-4a01-82bc-97e12a89f653', '2021-12-14 13:27:52.823708', '2021-12-14 13:28:20.595110', '2021-12-14 13:28:20.595062', '2021-12-14 13:28:20.595111', 'qwefqw', 'efqwefqwefqwef', '', 'InProgress', 'Auto', '2021-12-14 13:28:21.937281', 7);
