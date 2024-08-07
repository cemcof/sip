// ========= Implementation of CIISB projects ============

using sip.CEITEC.CIISB.Proposals.Creation;
using sip.CEITEC.CIISB.Proposals.TechnicalFeasibility;
using sip.Core;
using sip.Documents.Proposals;
using sip.Projects.Statuses;

namespace sip.CEITEC.CIISB;

public class CProjectFilter : ProjectFilter, IProjectFilter<CProject>;

public class CProjectService(
        IDbContextFactory<AppDbContext>           dbContextFactory,
        ProjectManipulationHelperService          projectService,
        TimeProvider                              timeProvider,
        IOptionsMonitor<StatusOptions>           statusOptions,
        ProjectStatusHelperService                status,
        YearOrderIdGeneratorService               idGen,
        ProjectOrganizationMessageBuilderProvider messageBuilderProvider,
        DocumentService                           documentService,
        AppUserManager                            userManager)
    :
        IProjectDefine<CProject>,
        IProjectFactory<CProject>,
        IProjectLoader<CProject>,
        IManyProjectsLoader<CProject, CProjectFilter>,
        IProjectItemRenderProvider<CProject>,
        IProjectStatusManager<CProject>,
        IProjectMessaging<CProject>,
        IProjectDailyActionHandler<CProject>,
        IIdGenerator<CProject>,
        IDocumentProvider<CCreationProposal>,
        IDocumentSubmitter<CCreationProposal>,
        IDocumentFactory<CCreationProposal>,
        IDocumentRenderInfoProvider<CCreationProposal>,
        IDocumentProvider<TechnicalFeasiblility>,
        IDocumentSubmitter<TechnicalFeasiblility>,
        IDocumentFactory<TechnicalFeasiblility>,
        IDocumentRenderInfoProvider<TechnicalFeasiblility>,
        IProjectMessageConsumer<CProject>

{
    private readonly IOptionsMonitor<StatusOptions> _statusOptions = statusOptions;

    /// <summary>
    /// Defines the policy of project expiration - returns dt of project expriation
    /// </summary>
    /// <param name="fromDate"></param>
    /// <returns></returns>
    public DateTime ComputeExpirationDate(DateTime? fromDate = null)
    {
        if (fromDate == null)
        {
            fromDate = timeProvider.DtUtcNow();
        }

        DateTime result = new DateTime(fromDate.Value.Year + 1, fromDate.Value.Month, 1);
        result = result.AddMonths(2);
        result = result.AddDays(-1);
        return result;
    }


    public async Task SubmitDocumentAsync(CCreationProposal proposal)
    {
        // if (proposal.DtSubmitted != default) throw new InvalidOperationException("Form already submitted");
        
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var newProject = await CreateProjectAsync();
        
        // Bind this proposal with the new project
        proposal.DtSubmitted = timeProvider.DtUtcNow();
        
        // Extract some information from the proposal to the project itself
        var genProjInfo = proposal.CProposalFormModel.GeneralProjectInformation;
        newProject.Title = genProjInfo.ProjectTitle;
        newProject.Acronym = genProjInfo.Acronym;
        newProject.AffiliationDetails.Address = genProjInfo.InvoicingAddress.Address;  
        newProject.AffiliationDetails.Country = genProjInfo.InvoicingAddress.Country;  
        newProject.AffiliationDetails.Name = genProjInfo.InvoicingAddress.OrganizationName;
        newProject.AffiliationDetails.Type = genProjInfo.InvoicingAddress.Organization;
        newProject.DtCreated = DateTime.UtcNow;
        newProject.DtExpiration = ComputeExpirationDate(newProject.DtCreated);
        
        newProject.ProjectDocuments.Add(proposal);
        
        // Create and set project users - applicant, principal and additional members
        var applicant = await userManager.MatchEnsureUser(new Contact()
        {
            Firstname = genProjInfo.Applicant.FirstName, Lastname = genProjInfo.Applicant.Surname,
            Email = genProjInfo.Applicant.Email
        });
        
        var principal = await userManager.MatchEnsureUser(new Contact()
        {
            Firstname = genProjInfo.PrincipalInvestigator.FirstName,
            Lastname = genProjInfo.PrincipalInvestigator.Surname, Email = genProjInfo.PrincipalInvestigator.Email
        });

        var appMember = new ProjectMember()
        {
            MemberType = nameof(ApplicantMember),
            MemberUserId = applicant.Id,
        };

        var priMember = new ProjectMember()
        {
            MemberType = nameof(PrincipalMember),
            MemberUserId = principal.Id
        };
        
        newProject.ProjectMembers.Add(appMember);
        newProject.ProjectMembers.Add(priMember);
        
        // Additional members
        foreach (var additionalMember in genProjInfo.AdditionalMembers)
        {
            var usr = await userManager.MatchEnsureUser(new Contact()
            {
                Firstname = additionalMember.FirstName,
                Lastname = additionalMember.Surname, Email = additionalMember.Email
            });
            
            newProject.ProjectMembers.Add(new ProjectMember()
            {
                MemberType = nameof(AdditionalMember),
                MemberUserId = usr.Id 
            });
        }
        
        
        // Must save before following org
        // Project created, set the statuses for relevant organizations
        // Dirty way for now
        var forOrgs = new List<string>();
        var ceitecs = proposal.CProposalFormModel.CoreFacilities.CeitecCoreFacilities;
        if (ceitecs is not null)
        {
            if (ceitecs.CfBic is not null) forOrgs.Add(nameof(Ceitec.CfBic));
            if (ceitecs.CfProteo is not null) forOrgs.Add(nameof(Ceitec.CfProt));
            if (ceitecs.CFNMR is not null) forOrgs.Add(nameof(Ceitec.CfNmr));
            if (ceitecs.CfXray is not null) forOrgs.Add(nameof(Ceitec.CfXray));
            if (ceitecs.CfCryoEM is not null) forOrgs.Add(nameof(Ceitec.CfCryo));
            if (ceitecs.CfNanoBio is not null) forOrgs.Add(nameof(Ceitec.CfNano));
        }

        var biocevs = proposal.CProposalFormModel.CoreFacilities.BiocevCoreFacilities;
        if (biocevs is not null)
        {
            if (biocevs.CfBiBiotech is not null) forOrgs.Add(nameof(Biocev.BTech));
            if (biocevs.CfBiCryst is not null) forOrgs.Add(nameof(Biocev.BCryst));
            if (biocevs.CfBiDiff is not null) forOrgs.Add(nameof(Biocev.BDiff));
            if (biocevs.CfBiProt is not null) forOrgs.Add(nameof(Biocev.BProtProd));
            if (biocevs.CfBiSpec is not null) forOrgs.Add(nameof(Biocev.BSpec));
        }
        
        // Create tech feases
        foreach (var forOrg in forOrgs)
        {
            newProject.ProjectDocuments.Add(new TechnicalFeasiblility() { OrganizationId = forOrg, ProposalState = ProposalState.WaitingForSubmission });
        }
        
        // Save ...
        db.Update(newProject);
        await db.SaveChangesAsync();
        
        await status.ChangeStatusAsync<WaitingForTechnicalFeasibility>(newProject, forOrgs);
        
        // var targetOrgs = genProjInfo.Applicant.Surname
        
        // Reload the project
        var reloaded = await LoadAsync(newProject.Id);
        
        // Finally, send the email to the user
        await CNewProjectMailAsync(reloaded);
    }

    public async Task CNewProjectMailAsync(CProject project)
    {
        var dataContext = new CProjectContext(project);
        
        var bd = messageBuilderProvider.CreateBuilder();
        bd.BodyFromFileTemplate(dataContext,
            (project.PeerReviewRequired) ? "ProposalSubmitUserWithPr.hbs" : "ProposalSubmitUser.hbs");
        bd.AddAttachment(
            await documentService.RenderToPdfAsync(project.CreationProposal, $"Proposal {project.Id}"));
        bd.AddRecipient(project.Applicant);

        await bd.BuildAndSendAsync();
    } 

    public Task<CCreationProposal> CreateProposalAsync()
    {
        throw new NotImplementedException();
    }

    public Task SubmitDocumentAsync(TechnicalFeasiblility proposal)
    {
        throw new NotImplementedException();
    }

    public Task HandleDailyActionAsync(CProject project)
    {
        throw new NotImplementedException();
    }


    public Task ChangeStatusAsync(CProject project, string statusId, IEnumerable<string> organizationIds)
        => status.ChangeStatusAsync(project, statusId, organizationIds);

    public Task<IEnumerable<StatusInfo>> GetRelevantStatusInfosAsync(CProject project)
    {
        var result = new List<StatusInfo>();
        // All projects have technical feasiblity
        result.Add(status.GetStatusInfo<WaitingForTechnicalFeasibility>());
        result.Add(status.GetStatusInfo<TechnicalFeasibilityEvaluated>());

        if (project.PeerReviewRequired)
        {
            result.Add(status.GetStatusInfo<WaitingForPeerReviewRequest>());
            result.Add(status.GetStatusInfo<WaitingForPeerReview>());
        }

        if (project.IsGtcRequired())
        {
            result.Add(status.GetStatusInfo<WaitingForOrderConfirmation>());
        }
        
        result.Add(status.GetStatusInfo<ServiceInProgress>());
        result.Add(status.GetStatusInfo<ServiceFinished>());
        result.Add(status.GetStatusInfo<ProjectFinished>());
        return Task.FromResult(result.AsEnumerable());
    }

    public Task InStatus<TStatusRef, TOrganizationRef>(CProject project)
        => status.InStatus<TStatusRef, TOrganizationRef>(project);

    public Task<IEnumerable<MessageRecipient>> GetResendRecipientsAsync(ProjectOrganizationMessage message)
    {
        throw new NotImplementedException();
    }

    public async Task<CProject> CreateProjectAsync()
    {
        return await projectService.CreateProjectAsync(this);
    }

    public Task<string> GenerateNextIdAsync()
        => idGen.GenerateNextIdAsync<CProject>(new YearOrderIdGeneratorOptions {Postfix = "C"});

    public Task<CProject> LoadAsync(string id)
        => projectService.LoadProjectAsync<CProject>(id);


    Task<TechnicalFeasiblility?> IDocumentProvider<TechnicalFeasiblility>.GetDocumentAsync(Guid id)
        => documentService.GetProposalDocumentAsync<TechnicalFeasiblility>(id);
    
    public Task<CCreationProposal?> GetDocumentAsync(Guid id)
        => documentService.GetProposalDocumentAsync<CCreationProposal>(id);
    

    

    public async Task AddPublicationsAsync(CProject project, IEnumerable<string> dois)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Entry(project).State = EntityState.Unchanged; // TODO - is this ok? (unchanged not modified)
        project.Publications = dois.Concat(project.Publications).Distinct().ToList();
        await db.SaveChangesAsync();
    }

    public ComponentRenderInfo GetProjectItemComponent(CProject project)
    {
        var paramss = new Dictionary<string, object>
        {
            {nameof(CProjectItemComponent.Project), project},
        };
        
        return new ComponentRenderInfo(typeof(CProjectItemComponent), paramss);
    }


    public Type GetProjectComponent() => typeof(CProjectComponent);
    
    public async Task ConsumeMessageAsync(ProjectOrganizationMessage message)
    {
        var org = message.Organization;
        var proj = (CProject)message.Project;

        // Handle generals terms and conditions submission
        if (message.SubjectMatch("GTC"))
        {
            if (org.Is<Biocev>() && proj.InStatusAny<WaitingForOrderConfirmation, Biocev>())
            {
                // Biocev terms are handled 
            }

            if (org.Is<Ceitec>() && proj.InStatusAny<WaitingForOrderConfirmation, Ceitec>())
            {
                // Ceitec terms are handled
            }
        }

        if (message.SubjectMatch("Service finished"))
        {
            // Extract and save publications
            var dois = Doi.ExtractDoisFromText(message.MessageData.Message.HtmlBody + "\n" +
                                               message.MessageData.Message.TextBody);
            var doisList = dois.ToList();
            if (doisList.Count > 0)
            {
                await AddPublicationsAsync(proj, doisList);
            }
        }
    }

    public Task<ProjectLoadResults> LoadManyAsync(CProjectFilter? filter)
    {
        return projectService.LoadProjectsAsync<CProject>(filter);
    }

    Task<CCreationProposal> IDocumentFactory<CCreationProposal>.CreateDocumentAsync()
        => documentService.CreateDocumentAsync<CCreationProposal>();

    public DocumentRenderInfo GetRenderInfo(CCreationProposal document)
        => new()
        {
            SubmitRender = new(typeof(CProposalForm), new Dictionary<string, object?>()
            {
                {nameof(CProposalForm.Proposal), document}
            }),
            ViewRender = new(typeof(CCreationProposalRender), new Dictionary<string, object?>()
            {
                {nameof(CCreationProposalRender.CreationProposal), document}
            })
        };
    
    Task<TechnicalFeasiblility> IDocumentFactory<TechnicalFeasiblility>.CreateDocumentAsync()
        => documentService.CreateDocumentAsync<TechnicalFeasiblility>();

    public DocumentRenderInfo GetRenderInfo(TechnicalFeasiblility document)
        => new()
        {
            ViewRender = new(typeof(TechFeasibilityView), new Dictionary<string, object?>()
            {
                {nameof(TechFeasibilityView.Document), document}
            })
        };

    public Type ProjectType => typeof(CProject);
    public string DisplayName => "CIISB";
    public string Theme => "lightgreen";
}

