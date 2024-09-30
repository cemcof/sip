using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using sip.Experiments;
using sip.Utils.Crm;

namespace sip.Schedule;

public class CrmReservationsEngine(
        CrmService                     crmService,
        IMemoryCache                   cache,
        ILogger<CrmReservationsEngine> logger,
        TimeProvider                   timeProvider)
    : IScheduleEngine
{
    private readonly Dictionary<int, string> _subtypeMap = new()
    {
        {1, "Measurement"},
        {2, "Service / Servis"},
        {3, "Maintenance / Udrzba"},
        {4, "Training / Skoleni"},
        {5, "Training / Vyuka"},
        {6, "Rezervace FS"},
        {7, "Blokace"}
    };

    private const string REQUEST_TEMPLATE = @"
        <fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""true"">
          <entity name=""serviceappointment"">
            <attribute name=""subject""/>
            <attribute name=""statecode""/>
            <attribute name=""scheduledstart""/>
            <attribute name=""scheduledend""/>
            <attribute name=""activityid""/>
            <!-- <attribute name=""new_nazev_pristroje_text""/> -->
            <attribute name=""modifiedon""/>
            <attribute name=""createdon""/>
            <attribute name=""statuscode""/>
            <attribute name=""description""/>
            <attribute name=""psa_timerequirement""/>
            <attribute name=""ge_res_groupid""/>
            <attribute name=""psa_projectid""/>
            <attribute name=""ge_realised_for_contactid""/>
            <!-- <order attribute=""subject"" descending=""false""/> -->
            <filter type=""and"">
              <!-- <condition attribute=""scheduledstart"" operator=""on-or-after"" value=""2019-12-16""/> -->
              <condition attribute=""scheduledstart"" operator=""on-or-after"" value=""MiB_UID_PLACEHOLDER_FROM""/>
            </filter>
            <link-entity name=""activityparty"" from=""activityid"" to=""activityid"" alias=""am"">
              <filter type=""and"">
                <!-- <condition attribute=""partyid"" operator=""eq"" uiname=""Prozařovací elektronový mikroskop Titan Krios"" uitype=""equipment"" value=""{014C42A8-3453-E311-85A1-005056991551}""/> -->
                <!--<condition attribute=""partyid"" operator=""eq"" value=""{014C42A8-3453-E311-85A1-005056991551}""/> -->
                <condition attribute=""partyid"" operator=""eq"" value=""{MiB_UID_PLACEHOLDER_EQUIPMENT}""/>
              </filter>
            </link-entity>
            <link-entity name=""ge_res_group"" from=""ge_res_groupid"" to=""ge_res_groupid"" visible=""false"" link-type=""outer"" alias=""a_6b2bc151e589e41186ce005056991551"">
              <attribute name=""ge_name_eng"" alias=""research_group""/>
            </link-entity>
            <link-entity name=""psa_projects"" from=""psa_projectsid"" to=""psa_projectid"" visible=""false"" link-type=""outer"" alias=""a_ff8558252f02e211ac8a005056991763"">
              <attribute name=""ge_name_eng"" alias=""project""/>
            </link-entity>
            <link-entity name=""systemuser"" from=""systemuserid"" to=""createdby"" visible=""false"" link-type=""outer"" alias=""CREATOR"">
              <attribute name=""fullname"" />
            </link-entity>
            <link-entity name=""contact"" from=""contactid"" to=""ge_realised_for_contactid"" visible=""false"" link-type=""outer"" alias=""CUSTOMER"">
              <attribute name=""fullname"" />
            </link-entity>
          </entity>
        </fetch>
       ";

    private string PrepareTemplate(Guid forInstrumentId, int daysPast)
    {
        var t0 = timeProvider.DtUtcNow().Date;
        var anchor = t0 - TimeSpan.FromDays((int)t0.DayOfWeek);
        var dtStart = anchor - TimeSpan.FromDays(daysPast);
        logger.LogDebug("Limit for past (scheduled start): {}", dtStart);

        // Start date
        var template = REQUEST_TEMPLATE.Replace("MiB_UID_PLACEHOLDER_FROM", dtStart.ToString("yyyy-MM-dd"));
        // Instrument ID
        return template.Replace("MiB_UID_PLACEHOLDER_EQUIPMENT", forInstrumentId.ToString());
        
    }

    private string GetCacheKey(IOrganization organization)
        => "reservations/" + organization.Id;
    
    public Task<ScheduleData> GetReservationsDataAsync(IOrganization organization)
    {
        if (!cache.TryGetValue<ScheduleData>(GetCacheKey(organization), out var scheduleData))
        {
            throw new NotAvailableException($"Planning board data is not available at the moment for {organization}");
        }

        return Task.FromResult(scheduleData!);
    }

    private ReservationType GetReservationType(string subtype, int statecode, int statuscode)
    {
        if (subtype != "Measurement")
            return ReservationType.Special;
        if (statecode != 3 && statuscode == 4)
            return ReservationType.Warning;

        return ReservationType.Normal;
    }

    public async Task RefreshAsync(IOrganization organization, IEnumerable<IInstrument> reservationSubjects,
        int daysPast)
    {
        var now = timeProvider.DtUtcNow();
        var reservationInstruments = new List<ScheduleInstrument>();
        var result = new ScheduleData(now, reservationInstruments);
        
        // Start by preparing generic template
        foreach (var reservationSubject in reservationSubjects)
        {
            var xmlFetch = PrepareTemplate(reservationSubject.Id, daysPast);
            var tmpRawResult = await crmService.FetchRawByXmlRaw("serviceappointments", xmlFetch);
            logger.LogDebug("Fetched for instrument: \n {}", reservationSubject);

            var scheduleInstrumentReservations = new List<ScheduleReservation>();
            
            // Iterate over reservation data and extract them from JSON
            var jsonReservations = tmpRawResult["value"]!.AsArray();
            foreach (var jsonReservation in jsonReservations)
            {
                // Extract subtype, statecode and status code
                var (subtype, statecode, statuscode) = (
                    _subtypeMap[jsonReservation!["psa_timerequirement"]!.GetValue<int>()], 
                    jsonReservation["statecode"]!.GetValue<int>(), 
                    jsonReservation["statuscode"]!.GetValue<int>()
                    );
                
                // Skip cancelled reservations (they might still be present in dataset)
                if (statecode == 2 && statuscode == 9) continue;
                 
                // Extract ppl names and create ppl representations: 
                var customerNames = jsonReservation["CUSTOMER_x002e_fullname"]?.GetValue<string>().Split(", ") ?? new []{"<unknown>", "<unknown>"};
                var forUser = new UserInfo(default, default, customerNames[1], customerNames[0]);
                var createdNames = jsonReservation["CREATOR_x002e_fullname"]?.GetValue<string>().Split(", ") ?? new []{"<unknown>", "<unknown>"};
                var createdBy = new UserInfo(default, default, createdNames[1], createdNames[0]);
                
                // Extract dates
                var dtUntil = DateTime.Parse(jsonReservation["scheduledend"]!.GetValue<string>(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var dtSince = DateTime.Parse(jsonReservation["scheduledstart"]!.GetValue<string>(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                var dtCreated = DateTime.Parse(jsonReservation["createdon"]!.GetValue<string>(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                
                // Extract research group, subject and project - they might not be present
                var (researchGroup, subject, project) = (
                    jsonReservation["research_group"]?.GetValue<string>() ?? "<no research group>", // TODO - maybe use PID as Michal did
                    jsonReservation["subject"]?.GetValue<string>() ?? "<no subject>",
                    jsonReservation["project"]?.GetValue<string>() ?? "<no project>"
                    );
                
                // Eventually, create and add reservation representation using the information above
                scheduleInstrumentReservations.Add(new ScheduleReservation(
                     dtSince,
                     dtUntil,
                     forUser,
                     createdBy,
                     dtCreated,
                     GetReservationType(subtype, statecode, statuscode),
                     subject,
                     project,
                     subtype,
                     researchGroup
                    ));
            }
            
            // Add processed instrument to the results
            reservationInstruments.Add(new ScheduleInstrument(reservationSubject, scheduleInstrumentReservations, 
                !string.IsNullOrEmpty(reservationSubject.DisplayTheme) && 
                reservationSubject.DisplayTheme != "#ffffff"));// TODO - this render "if" is kinda bad way to decide, maybe introduce another flag?
        }
        
        // Save result to the cache
        cache.Set(GetCacheKey(organization), result);
    }
}