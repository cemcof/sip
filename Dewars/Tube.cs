


// From old emcf to this 
// SELECT concat(dewar, '/', holder, '/', position) as Id, responsible as `User`, notes as `Description`, DATE_FORMAT(FROM_UNIXTIME(`dt_change`), '%Y-%m-%d %H:%i:%s') as "LastChange" FROM `sample_storage` WHERE 1

/*
INSERT INTO `sample_storage` (`Id`, `User`, `Description`, `LastChange`) VALUES
('A/1/B/12', 'DedicE', '40S', '2019-04-05 16:27:31'),
('A/1/T/12', 'KissE', '', '2019-05-15 13:33:14'),
('A/1/B/3', 'Krasny', 'RNAP Bsub', '2019-04-05 16:10:16'),
('A/1/T/3', 'RiedelC', '', '2019-04-05 16:30:58'),
('A/1/B/6', 'Stefl', 'Spt6, RNAP, Dicer', '2020-10-15 14:30:59'),
('A/1/T/6', 'KourilR', '', '2019-04-05 16:31:41'),
('A/1/B/9', 'Litvinchuk ', '25.11.19', '2020-11-10 08:32:50'),
('A/1/T/9', 'KereicheS/Piano', '', '2020-04-22 11:12:09'),
('A/2/B/12', 'Blazek D', 'ckd13', '2021-02-02 21:21:43'),
('A/2/T/12', 'Ruskamo', 'CIISB', '2020-03-02 16:16:18'),
('A/2/B/3', 'Jiracek', 'granule,IR', '2021-01-16 06:03:46'),
('A/2/T/3', 'Pinkas D', 'Daniel Pinkas', '2021-10-26 15:30:00'),
('A/2/B/6', 'HaselbachD', 'CIISB', '2020-02-22 23:15:54'),
('A/2/T/6', 'Nedvedova', 'CIISB', '2020-11-06 11:12:23'),
('A/2/B/9', 'Rozbesky', 'CIISB', '2021-11-27 21:20:24'),
('A/2/T/9', 'Pinkas', 'Pinkas A', '2021-10-26 15:29:48'),
('A/3/B/12', 'KoubaT', '', '2021-08-25 06:45:57'),
('A/3/T/12', 'Jiracek 2', 'Granules', '2021-04-14 17:22:58'),
('A/3/B/3', 'Perrakis', 'iNEXT', '2021-01-16 06:04:57'),
('A/3/T/3', 'IKS', 'CIISB', '2021-09-29 20:58:47'),
('A/3/B/6', 'Rezacova P', 'Cggr, DeoR', '2019-11-15 08:30:21'),
('A/3/T/6', 'Hritz', 'Gytis', '2021-05-27 07:33:43'),
('A/3/B/9', 'Wilson', 'Samples 4.12.20', '2020-12-09 11:58:09'),
('A/3/T/9', 'SuluR', 'Instruct', '2021-10-15 15:14:28'),
('A/4/B/12', 'Demo', 'DemoLAB', '2021-12-01 12:34:55'),
('A/4/T/12', 'Skubnik', 'RNAPol', '2021-03-23 17:44:21'),
('A/4/B/3', 'Pinkas', '', '2021-06-29 10:57:20'),
('A/4/T/3', 'Pinkas', 'Pinkas', '2020-12-16 09:08:15'),
('A/4/B/6', 'Tars', 'Kaspars Tars', '2021-07-18 21:02:56'),
('A/4/T/6', 'Pinkas', 'Pinkas 2', '2020-12-16 09:08:28'),
('A/4/B/9', 'MP', 'Pinkas 4', '2021-06-29 10:58:09'),
('A/4/T/9', 'Pinkas', 'MP X', '2021-06-29 10:57:46'),
('A/5/B/12', 'EYEN', '911', '2021-06-01 14:34:19'),
('A/5/T/12', 'EYEN', 'CERS EYEN', '2021-10-05 12:22:35'),
('A/5/B/3', 'EYEN', 'P', '2019-04-05 16:14:30'),
('A/5/T/3', 'EYEN', 'Sanofi 2.4.19', '2019-04-05 16:21:31'),
('A/5/B/6', 'EYEN', 'ATM II', '2019-04-05 16:15:04'),
('A/5/T/6', 'EYEN', 'X', '2019-04-05 16:22:52'),
('A/5/B/9', 'EYEN', '911', '2019-04-05 16:20:57'),
('A/5/T/9', 'EYEN', 'ATM', '2019-04-05 16:23:23'),
('A/6/B/12', 'Vykoukal V.', 'HPF 25.5.21 - 4xGB, 5 samples', '2021-05-26 08:32:14'),
('A/6/T/12', 'Kereiche, Kutejova', 'CIISB', '2021-02-19 17:57:43'),
('A/6/B/3', 'Babiak', 'Own + training M. Marek ', '2021-05-06 14:08:50'),
('A/6/T/3', 'V.Vykoukal', 'Apo E', '2020-11-10 08:49:55'),
('A/6/B/6', 'Filimonenko', '', '2021-06-01 14:34:40'),
('A/6/T/6', 'Trebichalska', '', '2020-11-10 08:50:32'),
('A/6/B/9', 'Albert Oulu', 'CPLX3-13', '2021-11-09 11:49:01'),
('A/6/T/9', 'Skubnik', '', '2020-11-10 08:50:59'),
('B/1/B/12', 'Subhash_Zidek-PSD', 'Tubulin aliquots(6Nos)-2nd Batch', '2021-10-05 13:12:15'),
('B/1/T/12', 'Jitka_Zidek-PSD', 'Tubulin_2nd batch aliquots- 4nos', '2021-10-05 13:04:03'),
('B/1/B/3', 'Virology - Hrebik', 'Domki - External', '2021-06-01 13:23:20'),
('B/1/T/3', 'Virology - Buttner', 'Carina 2', '2021-06-01 13:23:33'),
('B/1/B/6', 'Virology - Homola', 'Mirek - Titan', '2021-06-01 13:23:56'),
('B/1/T/6', 'Subhash- Zidek/PSD', 'Tubulin aliquots(7 nos)  batch 1 - 6Nos, Batch 2 - 1Nos', '2021-10-05 13:11:57'),
('B/1/B/9', 'Virology - Gondova', '', '2021-06-01 13:24:11'),
('B/1/T/9', 'Virology - Valentova', 'Lucie', '2021-06-01 13:24:16'),
('B/2/B/12', 'Virology - Ishemgulova', 'Aygul #1', '2021-06-01 13:28:03'),
('B/2/T/12', 'Virology - _RESPONSIBLE_PERSON_?_', 'Titan samples', '2021-06-01 14:02:45'),
('B/2/B/3', 'Virology - Hrebik', 'Domi', '2021-06-01 13:28:46'),
('B/2/T/3', 'Virology - Grybchuk', 'Danyil', '2021-06-01 13:29:01'),
('B/2/B/6', 'Virology - Homola', 'Mirek - General', '2021-06-01 13:29:20'),
('B/2/T/6', 'Virology - Büttner', 'Carina', '2021-06-01 13:29:36'),
('B/2/B/9', 'Virology - Palkova', '', '2021-06-01 13:14:50'),
('B/2/T/9', 'Virology - Mukhamedova', 'Liya', '2021-06-01 13:30:05'),
('B/3/B/12', 'Virology - Fuzik', 'Tibi 2', '2021-06-01 13:33:05'),
('B/3/T/12', 'Virology - Fuzik', 'Tibi - iTBEV', '2021-06-01 13:33:13'),
('B/3/B/3', 'Virology - Homola', 'Temporary - Declan Samples', '2021-06-01 13:47:44'),
('B/3/T/3', 'Virology - Palkova', '', '2021-10-04 13:20:06'),
('B/3/B/6', 'Virology - Prochazkova', 'LRV, L-A, FIB samples', '2021-06-01 13:15:52'),
('B/3/T/6', 'Virology - Cienikova', 'ZC', '2021-06-01 13:34:15'),
('B/3/B/9', 'Virology - Skubnik', 'K.S.', '2021-06-01 13:34:34'),
('B/3/T/9', 'Virology - Siborova', 'Marta', '2021-06-01 13:34:43'),
('B/4/B/12', 'CF int. - Vacha', '', '2021-06-01 14:39:02'),
('B/4/T/12', 'CF int. - Polak Sr', 'II Infl', '2021-06-01 13:46:17'),
('B/4/B/3', 'CF int. - Koutna', 'Nucleosome, others', '2021-06-01 13:46:52'),
('B/4/T/3', 'CF int. - Polovinkin', 'Vitaly - Lyso, BR', '2021-06-01 13:47:09'),
('B/4/B/6', 'CF int. - Babiak', 'Rozbesky - Session 210531', '2021-06-01 14:15:48'),
('B/4/T/6', 'CF int. - Polak Jr', '?', '2021-06-01 13:49:53'),
('B/4/B/9', 'CF int. - Polak Sr', 'PVY, Infl.', '2021-06-01 13:50:06'),
('B/4/T/9', 'CF int. - Klapstova', 'BrcaBard', '2021-06-01 13:50:27'),
('B/5/B/12', 'CF int. - Loschmidt Labs', 'Martin Marek, Nemergut', '2021-06-01 14:01:07'),
('B/5/T/12', 'CF int. - Zapletal', 'DicerOO/SM', '2021-06-01 14:00:30'),
('B/5/B/3', 'CF int. - Papageorgiou', 'Anna - RNAPssDNA', '2021-06-01 14:00:55'),
('B/5/T/3', 'CF int. - Lukavsky', 'PJL Group', '2021-06-01 14:01:23'),
('B/5/B/6', 'CF int. - Sebesta', '', '2021-06-01 14:01:38'),
('B/5/T/6', 'CF int. - Zapletal', '', '2021-09-01 08:30:43'),
('B/5/B/9', 'CF int. - Jekabsons', 'Atis', '2021-06-01 14:02:01'),
('B/5/T/9', 'CF int. - Monkova', 'HTH', '2019-04-05 16:35:32'),
('B/6/B/12', 'CF int. - Ishemgulova A.', '#3', '2021-05-17 16:47:39'),
('B/6/T/12', 'CF int. - Novacek', '', '2020-11-24 10:04:21'),
('B/6/B/3', 'CF int. - Ishembulova A.', '#2', '2021-04-28 18:13:42'),
('B/6/T/3', 'CF int. - Filimonenko', 'Training samples', '2020-11-24 10:06:02'),
('B/6/B/6', 'CF int. - Papageorgiou', 'RPA-DNA', '2021-05-17 16:47:07'),
('B/6/T/6', 'CF int. - Papageorgiou', '9.9.+10.9.2019', '2020-11-24 10:10:05'),
('B/6/B/9', 'CF int. - Subhash', 'SHSY5  Neural cells', '2020-11-24 10:11:55'),
('B/6/T/9', 'CF int. - Shiv, Oulo', '', '2020-11-24 10:12:16');

 * 
 */

namespace sip.Dewars;

/// <summary>
/// Data model for a tube in a dewar holder. 
/// </summary>
public class Tube
{
    /// <summary>
    /// Id is in format:
    /// dewar/holder/deck/position
    /// e.g. A/2/T/6
    /// </summary>
    public string Structure { get; private set; } = "///";

    public string OrganizationId { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
       
    public string User { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime LastChange { get; set; }

    public string Dewar { get => Structure.Split("/")[0]; set => Structure = $"{value}/{Holder}/{Deck}/{Position}"; }
    public string Holder { get => Structure.Split("/")[1]; set => Structure = $"{Dewar}/{value}/{Deck}/{Position}"; } 
    public string Deck { get => Structure.Split("/")[2]; set => Structure = $"{Dewar}/{Holder}/{value}/{Position}"; } 
    public string Position { get => Structure.Split("/")[3]; set => Structure = $"{Dewar}/{Holder}/{Deck}/{value}"; }

    public string TrimmedId => Structure.Trim('/');

    public bool IsEmpty => string.IsNullOrWhiteSpace(Description) && string.IsNullOrWhiteSpace(User);
    
    private Tube() { }
    
    public Tube(IOrganization organization)
    {
        OrganizationId = organization.Id;
        Organization = (Organization)organization;
    }

}