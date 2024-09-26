using Microsoft.AspNetCore.JsonPatch;
using sip.Core;
using sip.Experiments.Model;

namespace sip.Experiments;

public class ExperimentPublicationService(ExperimentsService experimentsService,
     IDbContextFactory<AppDbContext> dbContextFactory,
     TimeProvider timeProvider)
{
     public async Task RequestPublicationAsync(Experiment exp, CancellationToken ct = default)
    {
        var patch = new JsonPatchDocument<Experiment>();
        if (exp.Storage.State is not (StorageState.Archived or StorageState.Archiving))
        {
            // We must also request archivation
            patch.Replace(p => p.Storage.State, StorageState.ArchivationRequested);
        }
        
        patch.Replace(p => p.Publication.State, PublicationState.PublicationRequested);
        await experimentsService.PatchExperimentAsync(exp, patch, ct);
    }
    
    public bool CanDisablePublication(Experiment exp) =>
        exp is
        {
            Publication.State: PublicationState.DraftCreated or PublicationState.DraftCreationRequested,
            Storage.State: StorageState.Idle,
            Storage.Archive: true
        };

    public async Task DisablePublicationAsync(Experiment exp, CancellationToken ct = default)
    {
        if (!CanDisablePublication(exp)) 
            throw new InvalidOperationException("Cannot disable publication for this experiment");
        
        await experimentsService.PatchExperimentAsync(exp, e =>
        {
            e.Storage.Archive = false;
            e.Publication.State = PublicationState.DraftRemovalRequested;
            e.Storage.DtExpiration = timeProvider.DtUtcNow() + e.Storage.ExpirationPeriod;
        });
    }

    public bool CanEnablePublication(Experiment exp)
        => exp is
        {
            Publication.State: PublicationState.Unpublished or PublicationState.None,
            Storage.State: StorageState.Idle,
            Storage.Archive: false
        };

    public async Task EnablePublicationAsync(Experiment exp, CancellationToken ct = default)
    {
        if (!CanEnablePublication(exp)) 
            throw new InvalidOperationException("Cannot enable publication for this experiment");

        await experimentsService.PatchExperimentAsync(exp, e =>
        {
            e.Storage.Archive = true;
            e.Publication.State = PublicationState.DraftCreationRequested;
        });
    }
}