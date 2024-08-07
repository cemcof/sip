// This is a single purpose script to migrate data from old PHP project management implementation into the new .NET sip

// How to use: 
// 1) Prepare data folder locally and download files from the server
//    scp -r stigmator:/var/www/private_data ./private_data
// 2) Login to projman, copy the cookies and put them to the configuration, they will be used to access and migrate authorized resources
// 3) Run the migration on page /migr

using System.Data;
using System.Text.Json;
using System.Web;
using MySqlConnector;
using sip.CEITEC.CIISB.Proposals.Creation;
using sip.CEITEC.CIISB.Proposals.Extension;
using sip.CEITEC.CIISB.Proposals.PeerReview;
using sip.CEITEC.CIISB.Proposals.TechnicalFeasibility;
using sip.Core;
using sip.Documents.Proposals;
using sip.Projects.Statuses;

namespace sip.CEITEC.CIISB.DataMigration;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class CiisbMigratorOptions
{
    public string Constring { get; set; }
    public string RequestBaseAddress { get; set; }
    public Func<string, string, string> ProjfileUrlProvider { get; set; }
    public Func<string, string> ProjProposalPdfUrlProvider { get; set; }
    public List<Cookie> RequestCookies { get; set; }
    public string PrivateDataPath { get; set; }

    public static void Configure(IServiceCollection services, IConfigurationSection cfmnigr)
    {
        services.AddSingleton<CiisbMigratorService>();
        
        services.AddOptions<CiisbMigratorOptions>()
            .Bind(cfmnigr)
            .Configure(o =>
            {
                o.RequestCookies = new List<Cookie>()
                {
                    new(
                        cfmnigr.GetValue<string>("ShibName"),
                        cfmnigr.GetValue<string>("ShibValue"), "/", cfmnigr.GetValue<string>("CookieDomain")),
                    new("PHPSESSID", cfmnigr.GetValue<string>("PhpSessionId"), "/",
                        cfmnigr.GetValue<string>("CookieDomain"))
                };
            
                o.ProjfileUrlProvider = (p, f) => $"proxy/project_file.php?name={HttpUtility.UrlEncode(f)}&pid={p}";
                o.ProjProposalPdfUrlProvider = (pid) => $"proxy/pdf_proposal.php?pid={pid}";
            });
    }
}

public class CiisbMigratorService(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<CiisbMigratorService> logger, 
                                  AppUserManager appUserManager,
                                  IHttpClientFactory httpClientFactory,
                                  ProjectManipulationHelperService manipulationHelperService,
                                  OrganizationService organizationService,
                                  CProjectService projectService)
{
    private readonly IHttpClientFactory               _httpClientFactory         = httpClientFactory;
    private readonly ProjectManipulationHelperService _manipulationHelperService = manipulationHelperService;


    public async Task MigrateUser(string constr, string userEmail)
    {
        // Constringo is "server=localhost;userid=radem;database=projman_old;pwd=radem
        await using var dbctx = await dbContextFactory.CreateDbContextAsync();
        await using var mysqlcon = new MySqlConnection(constr);
        mysqlcon.Open();
        await using var com = mysqlcon.CreateCommand();
        com.CommandType = CommandType.Text;
        com.CommandText = $"SELECT * FROM people where email = '{userEmail}'";
        var reader = await com.ExecuteReaderAsync();
        await reader.ReadAsync();
        var email = reader.GetString("email");
        var fname = reader.GetString("firstname");
        var sname = reader.GetString("surname");
        var perunId = reader.GetString("perun_id");
        
        logger.LogInformation("Found user: {}, {}, {}", email, fname, sname);

        // Is the user already in db? if yes, skip him
        if (dbctx.Set<Contact>().Any(u => u.Email == email))
        {
            logger.LogInformation("Skipping user {} because it exists", email);
            return;
        }

        var newusesr = appUserManager.CreateItem();
        newusesr.PrimaryContact.Firstname = fname;
        newusesr.PrimaryContact.Lastname = sname;
        newusesr.PrimaryContact.Email = email;
        newusesr.PrimaryContact.Phone = reader.GetString("phone");
        await appUserManager.NewUserAsync(new NewUserModel() {UserDetails = newusesr});
        await reader.CloseAsync();
        
        // Login 
        if (!string.IsNullOrWhiteSpace(perunId))
        {
            var provider = perunId.EndsWith("ibt.cas.cz") ? "https://idp.ibt.cas.cz/idp/shibboleth" : "https://idp2.ics.muni.cz/idp/shibboleth";
            var display = perunId.EndsWith("ibt.cas.cz") ? "BIOCEV" : "MUNI";
            await appUserManager.AddLoginAsync(newusesr, new UserLoginInfo(provider, perunId, display));
        }
        
        // Handle roles
        await using var comRoles = mysqlcon.CreateCommand();
        comRoles.CommandType = CommandType.Text;
        comRoles.CommandText = $"SELECT role FROM rolemap where email = '{userEmail}'";
        var readerRoles = await comRoles.ExecuteReaderAsync();
        while (await readerRoles.ReadAsync())
        {
            var role = readerRoles.GetString("role");
            if (role == "r_superuser")
            {
                await appUserManager.EnsureUserInRole(new UserInRole()
                {
                    UserId = newusesr.Id, OrganizationId = nameof(InfrastructureOrg), RoleId = nameof(ImpersonatorRole)
                });
                continue;
            }
            
            if (role == "r_project_admin_guest")
            {
                await appUserManager.EnsureUserInRole(new UserInRole()
                {
                    UserId = newusesr.Id, OrganizationId = nameof(InfrastructureOrg), RoleId = nameof(AdminObserverRole)
                });
                continue;
            }

            var plitted = role.Split("_");
            var org = plitted.Last();
            role = string.Join("_", plitted.Take(plitted.Length - 1));
            
            var targetRole = role switch
            {
                "r_facility_head" => nameof(CfHeadRole),
                "r_peer_reviewer" => nameof(PeerReviewerRole),
                "r_responsible" => nameof(CfStaffRole),
                "r_project_admin" => nameof(ProjectAdminRole),
                _ => throw new ArgumentOutOfRangeException()
            };

            var targetOrg = GetTargetOrg(org);
            
            await appUserManager.EnsureUserInRole(new UserInRole()
            {
                UserId = newusesr.Id, OrganizationId = targetOrg, RoleId = targetRole
            });
        }

        await readerRoles.CloseAsync();
    }

    public string GetTargetOrg(string src)
    {
        var targetOrg = src switch
        {
            "BiSpec" => nameof(Biocev.BSpec),
            "BiDiff" => nameof(Biocev.BDiff),
            "BiCrys" => nameof(Biocev.BCryst),
            "BiBioTech" => nameof(Biocev.BTech),
            "BiProt" => nameof(Biocev.BProtProd),
            "CeJoda" => nameof(Ceitec.CfNmr),
            "CeBiomo" => nameof(Ceitec.CfBic),
            "CeNano" => nameof(Ceitec.CfNano),
            "CeCryo" => nameof(Ceitec.CfCryo),
            "CeXray" => nameof(Ceitec.CfXray),
            "CeProteo" => nameof(Ceitec.CfProt),
            "ceitec" => nameof(Ceitec),
            "biocev" => nameof(Biocev),
            _ => throw new ArgumentOutOfRangeException($"Argument src is bad: {src}")
        };
        return targetOrg;
    }   

    public async Task MigrateUsers(string constr)
    {
        await using var mysqlcon = new MySqlConnection(constr);
        mysqlcon.Open();
        await using var com = mysqlcon.CreateCommand();
        com.CommandType = CommandType.Text;
        com.CommandText = $"SELECT email FROM people where 1";
        var reader = await com.ExecuteReaderAsync();
        var usermails = new List<string>();
        while (await reader.ReadAsync())
        {
            usermails.Add(reader.GetString("email"));
        }
        
        logger.LogInformation("Found {} users to migrate", usermails.Count);
        foreach (var usermail in usermails)
        {
            await MigrateUser(constr, usermail);
        }
    }

    public async Task MigrateProjects(CiisbMigratorOptions opts)
    {
        await using var mysqlcon = new MySqlConnection(opts.Constring);
        mysqlcon.Open();
        await using var com = mysqlcon.CreateCommand();
        com.CommandType = CommandType.Text;
        com.CommandText = $"SELECT project_id FROM projects where 1";
        var reader = await com.ExecuteReaderAsync();
        var projectids = new List<string>();
        while (await reader.ReadAsync())
        {
            projectids.Add(reader.GetString("project_id"));
        }

        await reader.CloseAsync();
        
        foreach (var pid in projectids)
        {
            await MigrateProject(opts, pid);
        }
    }

    public async Task MigrateProject(CiisbMigratorOptions opts, string projectId)
    {
        // Links

        // Project proposal pripraveno na PDF: https://stigmator.ceitec.muni.cz/proxy/project.php?pid=220023C&format=pdf&hl
        // Pod auth: https://stigmator.ceitec.muni.cz/authzone/project/220037C/?pdf_render=true

        await using var dbctx = await dbContextFactory.CreateDbContextAsync();
        await using var mysqlcon = new MySqlConnection(opts.Constring);
        mysqlcon.Open();
        await using var com = mysqlcon.CreateCommand();
        com.CommandType = CommandType.Text;
        com.CommandText = $"SELECT * FROM projects where project_id = '{projectId}'";
        var reader = await com.ExecuteReaderAsync();
        await reader.ReadAsync();

        // Is the project already in db? if yes, skip him
        if (dbctx.Set<Project>().Any(p => p.Id == projectId))
        {
            logger.LogInformation("Skipping project {} because it exists", projectId);
            return;
        }

        // Now the difficult shit begins

        // Configure and establish httpclient
        var cookiec = new CookieContainer();
        foreach (var c in opts.RequestCookies)
        {
            cookiec.Add(c);
        }

        var httpc = new HttpClient(new HttpClientHandler()
        {
            UseCookies = true,
            CookieContainer = cookiec,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        {
            BaseAddress = new Uri(opts.RequestBaseAddress),
        };

        // Extract variables
        
        var porlOutcome = reader.GetString("prolong_outcome");
        var prolJustif = reader.GetString("prolong_justification");
        var prolongedPid = reader.GetString("prolonged_pid");
        var cfPeerReq = reader.GetString("cf_peer_req");
        var isForcedInternal = reader.GetString("intext").ToLower() == "internal";
        var dtPeerSent = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32("dt_peer_sent")).UtcDateTime;
        var dtServiceFinished = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32("dt_service_finished")).UtcDateTime;
        var peerReviewerMail = reader.GetString("peer_reviewer_mail");
        var appEmail = reader.GetString("app_email");
        var priEmail = reader.GetString("pri_email");
        var memberMailsJson = reader.GetString("member_mails");
        var respstr = reader.GetString("responsible_people");
        var dtCreated = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt32("dt_created")).UtcDateTime;
        var files = JsonSerializer.Deserialize<List<string>>(reader.GetString("files_and_figures")) ?? new List<string>();
        var projStatus = reader.GetString("status");
        var orgs = JsonSerializer.Deserialize<List<string>>(reader.GetString("facilities"))!.Select(GetTargetOrg).ToList(); 
        var finishedFacsStr = reader.GetString("finished_facs");
        var finishedOrgs = finishedFacsStr == "[]"
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(finishedFacsStr)!.Select(GetTargetOrg).ToList();

        // Firstly, establish the project
        var proj = new CProject
        {
            Id = projectId,
            Acronym = reader.GetString("acronym"),
            Title = reader.GetString("project_title"),
            DtCreated = dtCreated,
            DtExpiration = projectService.ComputeExpirationDate(dtCreated),
            AffiliationDetails = new AffiliationDetails()
            {
                Address = reader.GetString("address"),
                Country = reader.GetString("country"),
                Type = reader.GetString("organization_type"),
                Name = reader.GetString("organization")
            }
        };

        await reader.CloseAsync();

        // Is this project prolonging other project?
        if (!string.IsNullOrWhiteSpace(prolongedPid))
        {
            proj.ParentId = prolongedPid;
        }

        // Handle project members
        var applicant = await appUserManager.EnsureUserByEmailAsync(appEmail);
        var principle = await appUserManager.EnsureUserByEmailAsync(priEmail);
        var addMemberMails = JsonSerializer.Deserialize<List<string>>(memberMailsJson)!;
        foreach (var addMember in addMemberMails)
        {
            var memb = await appUserManager.EnsureUserByEmailAsync(addMember);
            proj.ProjectMembers.Add(new ProjectMember() {MemberType = nameof(AdditionalMember), ProjectId = projectId, OrganizationId = nameof(InfrastructureOrg), MemberUserId = memb.Id});
        }

        proj.ProjectMembers.Add(new ProjectMember() {MemberType = nameof(ApplicantMember), ProjectId = projectId, OrganizationId = nameof(InfrastructureOrg), MemberUserId = applicant.Id });
        proj.ProjectMembers.Add(new ProjectMember() {MemberType = nameof(PrincipalMember), ProjectId = projectId, OrganizationId = nameof(InfrastructureOrg), MemberUserId = principle.Id });
        var responsibles = (respstr == "[]")
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(respstr)!;
        foreach (var (fac, email) in responsibles)
        {
            var usr = await appUserManager.EnsureUserByEmailAsync(email);
            proj.ProjectMembers.Add(new ProjectMember() {MemberType = nameof(ResponsibleMember), ProjectId = projectId, OrganizationId = GetTargetOrg(fac), MemberUserId = usr.Id});
        }
        // -----------------------
        
        // Handle internal/external type, peer review and gtc requirement
        if (isForcedInternal) proj.ProjectType = CProjectType.Internal;
        
        // Handle publications
        await using var pubcom = mysqlcon.CreateCommand();
        pubcom.CommandText = $"SELECT doi, pid FROM publications where pid = '{projectId}'";
        var pubreader = await pubcom.ExecuteReaderAsync();
        var pubs = new List<string>();
        while (pubreader.Read())
        {
            pubs.Add(pubreader.GetString("doi"));
        }

        proj.Publications = pubs;
        await pubreader.CloseAsync();
        // -----------------------
        
        // Handle project proposal document and its attachments
        var proposal = new CCreationProposal()
        {
            ProjectId = projectId, 
            CProposalFormModel = new CProposalFormModel(),
            DtEvaluated = dtCreated,
            Name = nameof(CCreationProposal),
            DtSubmitted = dtCreated,
            ProposalState = ProposalState.Evaluated,
            FilesInDocuments = new List<FileInDocument>()
        };
        
        proj.ProjectDocuments.Add(proposal);
        
        var proposalfile = await httpc.GetByteArrayAsync(opts.ProjProposalPdfUrlProvider(projectId));
        proposal.FilesInDocuments.Add(new FileInDocument()
        {
            DocumentFileType = DocumentFileType.Primary,
            FileMetadata = new FileMetadata()
            {
                FileName = $"proposal_{projectId}.pdf",
                ContentType = new ContentType("application", "pdf"),
                Length = proposalfile.Length,
                DtCreated = dtCreated,
                DtModified = dtCreated,
                FileData = new FileData()
                {
                    Data = proposalfile
                }
            }
        });
        
        foreach (var file in files)
        {
            var fileData = await httpc.GetByteArrayAsync(opts.ProjfileUrlProvider(projectId, file));
            proposal.FilesInDocuments.Add(new FileInDocument
            {
                DocumentFileType = DocumentFileType.Attachment,
                FileMetadata = new FileMetadata
                {
                    FileName = file,
                    Length = fileData.Length,
                    ContentType = ContentType.Parse(MimeKit.MimeTypes.GetMimeType(file)),
                    DtCreated = dtCreated,
                    DtModified = dtCreated,
                    FileData = new FileData
                    {
                        Data = fileData
                    }
                }
            });
        }
        // --------------------------

        
        // Project proposals - technical feas, peer review a project extension
        await using var evalscommand = mysqlcon.CreateCommand();
        evalscommand.CommandText = $"SELECT * FROM evals where project_id = '{projectId}'";
        var evalsreader = await evalscommand.ExecuteReaderAsync();
        while (await evalsreader.ReadAsync())
        {
            var user = (await appUserManager.GetItems(evalsreader.GetString("eval_by"))).FirstOrDefault();
            var outcome = evalsreader.GetString("eval_outcome");
            var evalComments = evalsreader.GetString("eval_comments");
            var evalDt = DateTimeOffset.FromUnixTimeSeconds(evalsreader.GetInt32("eval_dt")).UtcDateTime;
            var evalFac = evalsreader.GetString("eval_facility");
            
            if (evalsreader.GetString("eval_type") == "tech")
            {
                // Tech fiasibility
                var tf = new TechnicalFeasiblility()
                {
                    Comments = evalComments,
                    DtEvaluated = evalDt,
                    OrganizationId = GetTargetOrg(evalFac),
                    DtSubmitted = evalDt,
                    Result = outcome switch
                    {
                        "accepted" => TechFeasibilityResult.Accepted,
                        "rejected" => TechFeasibilityResult.Rejected,
                        "acceptedupon" => TechFeasibilityResult.AcceptedUpon,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    Name = nameof(TechnicalFeasiblility),
                    ProjectId = projectId,
                    EvaluatedById = user?.Id,
                    ProposalState = ProposalState.Evaluated,
                    ExpectedEvaluatorId = user?.Id
                };
                proj.ProjectDocuments.Add(tf);
            }
            else
            {
                // Peer review
                var pr = new PeerReview()
                {
                    Comments = evalComments,
                    DtEvaluated = evalDt,
                    OrganizationId = GetTargetOrg(cfPeerReq),
                    Result = outcome switch
                    {
                        "accepted" => PeerReviewResult.Accepted,
                        "rejected" => PeerReviewResult.Rejected,
                        "acceptedupon" => PeerReviewResult.Rejected,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    Name = nameof(PeerReview),
                    ProjectId = projectId,
                    EvaluatedById = user?.Id,
                    ProposalState = ProposalState.Evaluated,
                    ExpectedEvaluatorId = user?.Id
                };
                proj.ProjectDocuments.Add(pr);
            }
            
        }

        await evalsreader.CloseAsync();

        if (!string.IsNullOrWhiteSpace(porlOutcome))
        {
            // There is extension proposal
            var extprop = new CExtensionProposal()
            {
                Justification = prolJustif,
                ProjectId = projectId,
                ExtensionResult = porlOutcome switch
                {
                    "nopub" => ExtensionResult.NoPublicationSoFar,
                    "pubs" => ExtensionResult.Publications,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Name = nameof(CExtensionProposal)
            };
            
            proj.ProjectDocuments.Add(extprop);
        }
        // ----------------------------

        // Project emails
        
        // Get threads
        await using var threadscom = mysqlcon.CreateCommand();
        threadscom.CommandText = $"SELECT thread_id, th_subject FROM email_threads where related_pid = '{projectId}'";
        var threadreader = await threadscom.ExecuteReaderAsync();
        var messageConn = new MySqlConnection(opts.Constring);
        await messageConn.OpenAsync();
        
        while (await threadreader.ReadAsync())
        {
            // For each thread, handle messages, order them by date
            await using var messagescom = messageConn.CreateCommand();
            messagescom.CommandText = $"SELECT * FROM emails where thread_id = {threadreader.GetInt32("thread_id")} order by dt_created";
            var messagereader = await messagescom.ExecuteReaderAsync();
            var subject = threadreader.GetString("th_subject");
            
            while (await messagereader.ReadAsync())
            {
                var direction = messagereader.GetString("direction");
                var category = messagereader.GetString("category");
                var sender = messagereader.GetString("sender");
                var recipients = JsonSerializer.Deserialize<List<string>>(messagereader.GetString("recipients"))!;
                var recipientsAppUsers = new List<AppUser>();
                foreach (var recipient in recipients)
                {
                    recipientsAppUsers.Add(await appUserManager.EnsureUserByEmailAsync(recipient));
                }
                var type = (direction == "incoming") ? MessageType.UserIn : MessageType.SystemOut;

                var mimeMessage = new MimeMessage();
                var messageFilePath = Path.Combine(opts.PrivateDataPath, "mails", messagereader.GetInt32("email_id").ToString(),
                    "message");
                if (File.Exists(messageFilePath))
                {
                    mimeMessage = await MimeMessage.LoadAsync(messageFilePath);
                    // Add recipients from mime message
                    foreach (var internetAddress in mimeMessage.To)
                    {
                        if (internetAddress is GroupAddress) continue;
                        var mbadd = (MailboxAddress) internetAddress;
                        var usr = await appUserManager.EnsureUserByEmailAsync(mbadd.Address);
                        if (!recipientsAppUsers.Contains(usr)) recipientsAppUsers.Add(usr);
                    }
                }
                else
                {
                    var bb = new BodyBuilder
                    {
                        HtmlBody = messagereader.GetString("body_html"),
                        TextBody = messagereader.GetString("body_text")
                    };
                    mimeMessage.Body = bb.ToMessageBody();
                    mimeMessage.Subject = messagereader.GetString("subject");
                    mimeMessage.To.AddRange(recipients.Select(MailboxAddress.Parse));
                }
                
                var message = new ProjectOrganizationMessage()
                {
                    OrganizationId = nameof(InfrastructureOrg),
                    ProjectId = projectId,
                    DtCreated = DateTimeOffset.FromUnixTimeSeconds(messagereader.GetInt32("dt_created")).UtcDateTime,
                    Subject = subject,
                    MessageType = type,
                    MessageData = new MessageData()
                    {
                        Message = mimeMessage
                    },
                    SenderId = (await appUserManager.EnsureUserByEmailAsync(sender)).Id,
                    Recipients = recipientsAppUsers.Select(rap => new MessageRecipient()
                    {
                        Type = MessageRecipientType.Primary,
                        UserId = rap.Id
                    }).ToList()
                };
                proj.ProjectOrganizationMessages.Add(message);

            }

            await messagereader.CloseAsync();
        }

        await messageConn.CloseAsync();
        // ---------------
        
        // Get GTC files CEITEC/BIOCEV
        var path = Path.Combine(opts.PrivateDataPath, "projects", projectId, "terms/confirmed");
        var gtcfiles = (Directory.Exists(path)) ? Directory.GetFiles(path) : Array.Empty<string>();
        var biocevFiles = gtcfiles.Where(f => f.ToLower().Contains("biocev")).Select(fp => (Path.GetFileName(fp), File.ReadAllBytes(fp))).ToArray();
        var ceitecFiles = gtcfiles.Where(f => f.ToLower().Contains("ceitec")).Select(fp => (Path.GetFileName(fp), File.ReadAllBytes(fp))).ToArray();
        if (ceitecFiles.Length > 0)
        {
            proj.ProjectDocuments.Add(new Document()
            {
                Name = nameof(GtcDocument),
                OrganizationId = nameof(Ceitec),
                ProjectId = projectId,
                FilesInDocuments = ceitecFiles.Select(cf => new FileInDocument()
                {
                    DocumentFileType = DocumentFileType.Primary,
                    FileMetadata = new FileMetadata()
                    {
                        Length = cf.Item2.Length,
                        ContentType = ContentType.Parse(MimeKit.MimeTypes.GetMimeType(cf.Item1)),
                        FileName = cf.Item1,
                        FileData = new FileData() { Data = cf.Item2 }
                    }
                }).ToList()
            });
        }
        if (biocevFiles.Length > 0)
        {
            proj.ProjectDocuments.Add(new Document()
            {
                Name = nameof(GtcDocument),
                OrganizationId = nameof(Biocev),
                ProjectId = projectId,
                FilesInDocuments = biocevFiles.Select(cf => new FileInDocument()
                {
                    DocumentFileType = DocumentFileType.Primary,
                    FileMetadata = new FileMetadata()
                    {
                        Length = cf.Item2.Length,
                        ContentType = ContentType.Parse(MimeKit.MimeTypes.GetMimeType(cf.Item1)),
                        FileName = cf.Item1,
                        FileData = new FileData() { Data = cf.Item2 }
                    }
                }).ToList()
            });
        }
        // -------------
        
        // Project statuses 
        if (projStatus == "Service in progress")
        {
            // Some of the facilities may be finished
            foreach (var org in orgs)
            {
                proj.ProjectStatuses.Add(new Status()
                {
                    OrganizationId = org, Active = true,
                    StatusInfoId = finishedOrgs.Contains(org) ? nameof(ServiceFinished) : nameof(ServiceInProgress),
                    DtEntered = finishedOrgs.Contains(org) ? dtServiceFinished : default
                });
            }
            
        }
        else if (projStatus == "Waiting for peer-review request")
        {
            var requesterOrg = GetTargetOrg(cfPeerReq);
             
            // Put that org as peer review request status and leave others in tech feas evaluated
            proj.ProjectStatuses.Add(new Status()
            {
                OrganizationId = requesterOrg, Active = true,
                StatusInfoId = nameof(WaitingForPeerReviewRequest)
            });
            
            foreach (var org in orgs.Where(o => o != requesterOrg))
            {
                proj.ProjectStatuses.Add(new Status()
                {
                    OrganizationId = org, Active = true,
                    StatusInfoId = nameof(TechnicalFeasibilityEvaluated)
                });
            }
        }
        else if (projStatus == "Waiting for order confirmation")
        {
            foreach (var org in orgs)
            {
                var statuss = nameof(WaitingForOrderConfirmation);
                var orginfo = organizationService.GetOrganizationInfo(org);
                if ((orginfo.ParentId == nameof(Ceitec) && ceitecFiles.Length > 0) ||
                    (orginfo.ParentId == nameof(Biocev) && biocevFiles.Length > 0))
                {
                    statuss = nameof(OrderConfirmed);
                }
                
                proj.ProjectStatuses.Add(new Status()
                {
                    OrganizationId = org, Active = true,
                    StatusInfoId = statuss
                });
            }
        }
        else
        {
            foreach (var org in orgs)
            {
                proj.ProjectStatuses.Add(new Status()
                {
                    OrganizationId = org, Active = true,
                    StatusInfoId = projStatus switch
                    {
                        "Waiting for peer-review" => nameof(WaitingForPeerReview),
                        "Service finished" => nameof(ServiceFinished),
                        "Project finished" => nameof(ProjectFinished),
                        "Waiting for technical feasibility" => nameof(WaitingForTechnicalFeasibility),
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    DtEntered = projStatus switch
                    {
                        "Waiting for peer-review" => dtPeerSent,
                        "Service finished" => dtServiceFinished,
                        _ => default
                    }
                });
            }
        }

        
        dbctx.Attach(proj);
        dbctx.Entry(proj).State = EntityState.Added;
        dbctx.Entry(proj.AffiliationDetails).State = EntityState.Added;
        await dbctx.SaveChangesAsync();
        
        // Check that all tech feases and peer reviews exist
        await using var dbctx2 = await dbContextFactory.CreateDbContextAsync();
        proj = await projectService.LoadAsync(proj.Id);
        foreach (var projOrganization in proj.Organizations)
        {
            // Is there tech feasilibity for the org? 
            if (!proj.TechnicalFeasiblilityProposal.Any(t => t.OrganizationId == projOrganization.Id))
                dbctx2.Add(new TechnicalFeasiblility()
                {
                    Name = nameof(TechnicalFeasiblility),
                    OrganizationId = projOrganization.Id,
                    ProjectId = proj.Id,
                    ProposalState = ProposalState.WaitingForSubmission
                });
        }

        if (proj.PeerReviewProposal is null && !string.IsNullOrWhiteSpace(cfPeerReq))
        {
            var pr = new PeerReview()
            {
                Name = nameof(PeerReview),
                ProjectId = proj.Id,
                OrganizationId = GetTargetOrg(cfPeerReq),
                ProposalState = ProposalState.WaitingForSubmission,
            };

            if (!string.IsNullOrWhiteSpace(peerReviewerMail))
            {
                var reviewer = await appUserManager.FindByEmailAsync(peerReviewerMail, CancellationToken.None);
                if (reviewer is not null)
                {
                    pr.ExpectedEvaluatorId = reviewer.Id;
                    pr.ProposalState = ProposalState.WaitingForEvaluation;
                }
            }

            dbctx2.Add(pr);

        }
        
        await dbctx2.SaveChangesAsync();
        
        
        logger.LogInformation("Successfully migrated project {}", projectId);
    }
    
    
}