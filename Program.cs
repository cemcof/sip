// ===================== PROJMAN ENTRY POINT ==============================
// Global usings (imports) can be found in Usings.cs 
// ========================================================================

using sip.CEITEC.CIISB.DataMigration;
using System.Web;
using sip.Autoloaders;
using sip.Core;
using sip.Core.IndexRedirecting;
using sip.Dewars;
using sip.Environment;
using sip.Experiments;
using sip.LabIssues;
using sip.RemoteAccess;
using sip.Schedule;

var emanSetup = new EmanSetup(args);

emanSetup.AutoDefault(builder =>
{
    var services = builder.WebApplicationBuilder.Services;
    var config = builder.WebApplicationBuilder.Configuration;

    services.UseRuleIndexRedirection()
        .ToLoginIfNoUserIdentity()
        .ToAvailableOrganizationIfNone()
        .Done();
        // TODO - role specific rules
    
        
    services.AddDewars(config);
    services.AddAutoloaders(config);
    var ebuilser = new ExperimentsBuilder(services, config);
    services.AddIssues(config);
    services.AddEnvironment(config);
    services.AddSchedule(config);
    services.AddRemoteAccess(config);
    
    // Migration feature from old CIISB, del in production
    var cfmnigr = builder.WebApplicationBuilder.Configuration.GetSection("PhpMigrator");
    CiisbMigratorOptions.Configure(services, cfmnigr);
    
});