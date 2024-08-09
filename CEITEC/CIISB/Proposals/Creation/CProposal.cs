using Microsoft.EntityFrameworkCore.Metadata.Builders;
using sip.Documents.Proposals;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace sip.CEITEC.CIISB.Proposals.Creation;
#nullable disable

public class CCreationProposal : Proposal
{
    public CProposalFormModel CProposalFormModel { get; set; } = new();
}

public class CProposalEConf : IEntityTypeConfiguration<CCreationProposal> 
{
    public void Configure(EntityTypeBuilder<CCreationProposal> builder)
    {
        builder.Property(cp => cp.CProposalFormModel)
            .ToJsonConvertedProperty();
    }
}


#region COMMON

public static class ProposalCommonStrings
{
    public const string SPA = "Single project access";
    public const string BAG = "BAG (block allocation group) access";
    public const string NO_NEED_OTHER_FORM = "If at any point of your measurements you need to use a different CMS core facility, you don´t need to fill out another request form. In such a case, please contact the facility you have requested access for and they will instruct you further.";
}


public class CProposalFormModel
{
    public CommonProposalModel GeneralProjectInformation { get; set; } = new();

    public CoreFacilities CoreFacilities { get; set; } = new();
}

public class CommonProposalModel
{
    [Required]
    public string ProjectTitle { get; set; }
    [Required]
    [Render(Tip = "Short title (10 characters max) for identification of the project, used by humans.")]
    [MaxLength(10)]
    public string Acronym { get; set; }

    [Render(GroupClass = "group-compactbox")]
    public PersonDetails Applicant { get; set; } = new();
    public bool PrincipalIsSameAsApplicant { get; set; } = false; // TODO - somehow bind this
    // [Binded("PrincipalIsSameAsApplicant", "False")] TODO
    
    [Render(GroupClass = "group-compactbox")]
    public PersonDetails PrincipalInvestigator { get; set; } = new();
    
    [Render(GroupClass = "group-compactbox")]
    public InvoicingAddress InvoicingAddress { get; set; } = new();
    [Render(Tip = "Add other people involved in the the project, if any. They will receive emails regarding the project workflow and will be able to participate in the general email communication with facilities about the project.")]
    public List<PersonDetails> AdditionalMembers { get; set; } = new();

    public GeneralProjectDetails ProjectDetails { get; set; } = new();

}

public class PersonDetails
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string Surname { get; set; }
    [EmailAddress]
    [Required]
    [Render(Tip = "If you actively use multiple email addresses, please provide all of them (comma separated), so we can identify your future email messages. Emails with unknown sender address will be rejected and not processed.")]
    public string Email { get; set; }
    [Phone]
    public string PhoneNumber { get; set; }
    [Selection("Researcher", "Ph.D. Student", "MSc student")]
    public string Position { get; set; }
}

public class InvoicingAddress
{
    [Selection("University", "Public research organization", "Industry company")]
    [Required]
    public string Organization { get; set; }
    [Required]
    [MinLength(3)]
    public string OrganizationName { get; set; }
    [Required]
    [Render(Tip = "Street, city, postal code.")]
    public string Address { get; set; }
    [CountrySelection]
    [Required]
    public string Country { get; set; } = "Czech Republic";
}

public class GeneralProjectDetails
{
    [Render(Sizing = Sizing.Big), MinLength(500), MaxLength(6000)]
    [Required]
    public string ScientificBackgroundOfTheProject { get; set; }
    [Render(Sizing = Sizing.Big), MinLength(500), MaxLength(3000)]
    [Required]
    public string BackgroundInTheLabAndPreliminaryData { get; set; }

    [Render(Sizing = Sizing.Big), MinLength(250), MaxLength(3000)]
    public string ProjectObjectivesAndExperimentalPlan { get; set; }
}

// ----------- CORE FACILITIES -----------------
public class CoreFacilities
{
    [Render(NoTitle = true), Selection(ItemsCollapsible = true)]
    public CeitecCoreFacilities CeitecCoreFacilities { get; set; } = new();
    
    [Render(NoTitle = true), Selection(ItemsCollapsible = true)]
    public BiocevCoreFacilities BiocevCoreFacilities { get; set; } = new();
}

public class CeitecCoreFacilities
{
    [Render(Title = "CEITEC - Biomolecular Interaction and Crystallization")]
    public CfBic CfBic { get; set; }
    [Render(Title = "CEITEC - X-ray Diffracttion and Bio-SAXS Core Facility")]
    public CfXray CfXray { get; set; }
    [Render(Title = "CEITEC - Cryo-electron Microscopy and Tomography")]
    public CfCryoEM CfCryoEM { get; set; }
    [Render(Title = "CEITEC - Nanobiotechnology")]
    public CfNanoBio CfNanoBio { get; set; }
    [Render(Title = "CEITEC - Proteomics")]
    public CfProteo CfProteo { get; set; }
    [Render(Title = "CEITEC - Josef Dadok National NMR Centre")]
    public CFNMR CFNMR { get; set; }
}

public class BiocevCoreFacilities
{
    [Render(Title = "BIOCEV - Biophysical techniques")]
    public CfBiBiotech CfBiBiotech { get; set; }
    [Render(Title = "BIOCEV - Crystallization of proteins and nucleic acids")]
    public CfBiCryst CfBiCryst { get; set; }
    [Render(Title = "BIOCEV - Diffraction techniques")]
    public CfBiDiff CfBiDiff { get; set; }
    [Render(Title = "BIOCEV - Structural mass spectrometry")]
    public CfBiSpec CfBiSpec { get; set; }
    [Render(Title = "BIOCEV - Protein production")]
    public CfBiProt CfBiProt { get; set; }

    // public BiocevSamplesContainer BiocevSamples { get; set; } = new BiocevSamplesContainer();
    [Render(AsNotOption = true), Required]
    public List<BiocevSample> BiocevSamples { get; set; } = new() { new BiocevSample() };

}

// Ceitec



public class CfNanoBio
{
    [Selection(
        "Cells - mechanical properties",
        "Cells - imaging",
        "Biomolecules - imaging",
        "Nano - objects - imaging",
        "Raman - AFM combined microscopy",
        "Raman microscopy",
        "Electrochemical measurements",
        "Nanodeposition system",
        "Fluorescence scanner",
        "SPR biosensor",
        "Multi - electrode array(MEA)"
    )]
    [Required]
    public List<string> TypeOfAnalysis { get; set; }

    [Render(Tip = "Origin, size, history, compatibility, safety...")]
    public string SamplesDescription { get; set; }

    [Selection(
        "Full service",
        "Measurement only",
        "Data processing only"
    )]
    [Required]
    public string RequiredService { get; set; }
}

public class CfProteo
{
    [Required]
    public List<ProteoSample> SamplesInformation { get; set; } = new() { new ProteoSample() };

    [Required]
    [Selection(
        "Protein fractionation / separation(1D, 2D, GE, IEF, LC)",
        "Intact mass analysis",
        "Protein identification",
        "Characterisation of protein modifications",
        "Absolute and relative protein quantification"
    )]
    public List<string> RequiredServices { get; set; }
}

public class CFNMR
{
    [Required]
    public List<NMRSample> SamplesInformation { get; set; } = new() { new NMRSample() };
    [Render(Title = "Type of NMR experiments", Tip = "E.g proton 1D, 13C with proton decoupling, DEPT, COSY, HMBC etc.")]
    public string TypeOfExperiments { get; set; }
    [Render(Tip = "Specify magnetic field, probehead, pulseprogram, etc., if necessary.")]
    public string EquipmentRequired { get; set; }
    [Render(Title = "Time needed to prepare and deliver samples")]
    [Required]
    public string TimeNeededDeliver { get; set; }
    [Render(Title = "Estimated measureing time required (days)")]
    [Required]
    public string TimeNeededMeasurement { get; set; }

    [Render(Title = "Previous NMR measurements on the sample")]
    [Selection(
        "none",
        "1D 1H",
        "2D Homonuclear",
        "2D Heteronuclear",
        "Heteronuclear 3D NMR",
        "other" // TODO - other text box
    )]
    public List<string> PreviousMeasurements { get; set; }

    [Selection(
        "Full service",
        "Full measurement and assistance with analysis",
        "Full measurement without analysis",
        "Assistance with measurement and analysis",
        "Assistance with masurement",
        "No assistance"
    )]
    [Required]
    public List<string> RequiredServices { get; set; }
}




public class CfBiCryst
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    public string TypeOfAccess { get; set; }

    [Render(Title = "Estimation of how many hours, days, or individual measurements are planned")]
    [Required]
    public string MeasurementDetails { get; set; }

    [Selection(
        "Robotic setup of 96 - well crystalliziation plates",
        "Manual setup of crystallization plates",
        "Manual setup of crystallization plates under an inert atmosphere",
        "Automated monitoring of crystallization in the crystallization hotel",
        "Crystal handling and preparation for diffraction experiments",
        "Crystal handling and preparation for diffraction experiments in oxygen free conditions"
    )]
    [Required]
    public List<string> RequiredServices { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string OtherSpecialRequests { get; set; }
}

public class CfBiSpec
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    public string TypeOfAccess { get; set; }

    [Selection(
         "Precise molecular weight determination",
         "Mass spec data interpretation",
         "MALDI analysis",
         "ESI electrospray analysis",
         "MS sample preparation and handling",
         "Hydrogen deuterium exchange with MS",
         "Chemical cross - linking with MS",
         "Native electrospray",
         "Fast photochemical oxidation of proteins",
         "Single protein identification from gel/ solution",
         "Proteins identification / quantification"
    )]
    [Required]
    public List<string> RequiredServices { get; set; }

    [Render(Sizing = Sizing.Medium)]
    public string SpecialRequests { get; set; }
}

public class CfBiProt
{
    [Selection(
        "Cloning", 
        "Protein expression (large-scale)", 
        "Protein purification", 
        "Small-scale expression and solubility test",
        "Method development"
    )]
    [Required]
    public List<string> RequiredServices { get; set; }
}


// ------------- Samples -------------------


public class BiocevSample
{
    [Required]
    public string SampleName { get; set; }
    [Required]
    [Render(Sizing = Sizing.Medium)]
    public string SampleDescription { get; set; }
    [Render(Title = "Do the sample present any risk to human health and/or environment?")]
    public bool Risk { get; set; }
    [Required]
    [Binded("Risk", "True")]
    [Selection("1", "2", "3")]
    public string ClassOfRisk { get; set; }
    [Required]
    public string SourceOfOrigin { get; set; }
    [Required]
    public bool IsRecombinant { get; set; }
    [Binded("IsRecombinant", "True")]
    public bool ExpressionHost { get; set; }
    [Selection("active virus", "virulence factor", "toxin", "prion protein")]
    public string TheSampleIs { get; set; }
    [Render(Sizing = Sizing.Medium)]
    public string OtherSpecifications { get; set; }
}

public class BiocevSamplesContainer
{
    public List<BiocevSample> BiocevSamples { get; set; } = new() { new BiocevSample() };
}

public class NMRSample
{
    [Render(Tip = "solid/liquid")]
    [Required]
    public string AggregationState { get; set; }
    [Required]
    public string MolecularWeight { get; set; }
    [Render(Title = "solvent/buffer/pH")]
    public string SolventBufferPH { get; set; }
    [Render(Tip = "typical/max allowed")]
    [Required]
    public string Temperature { get; set; }
    [Required]
    public string Concentration { get; set; }
    [Required]
    public string Stability { get; set; }
    [Required]
    [Selection(
        "natural isotopic abundance",
        "15N",
        "13C",
        "2H",
        "other"
     )] // TODO - other text field
    public List<string> IsoLabeled { get; set; }
    [Required]
    [Render(Sizing = Sizing.Medium)]
    public string Comments { get; set; }
}

public class ProteoSample
{
    [Required]
    public string Organism { get; set; }

    [Render(Tip = "Solution, gel etc; incl. solvent/buffer/salts/detergent information.", Sizing = Sizing.Small)]
    public string SampleOrigin { get; set; }
    [Render(Tip = "Description of sample prep procedures.", Sizing = Sizing.Small)]
    public string SampleHistory { get; set; }
    public ProteoGelElectro GelElectrophoresis { get; set; } = new();
}

public class ProteoGelElectro
{
    [Selection("1D", "2D")]
    [Render(NoTitle = true)]
    public string GelElType { get; set; }
    [Render(Title = "pl range", Unit = "%")]
    public string PlRange { get; set; }
    public string T { get; set; }
    public string Staining { get; set; }
}

#endregion


#region BiBiotech

public class CfBiBiotech
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    public string TypeOfAccess { get; set; }
    [Selection]
    [Required]
    public CfBiBiotechServices RequiredServices { get; set; } = new();
    [Render(Sizing = Sizing.Small)]
    public string OtherSpecialRequests { get; set; }
}

public class CfBiBiotechServices
{
    [Render(Title = "Surface plasmon resonance (SPR)")]
    public BiotechSurfacePlasmonResonance SurfacePlasmonResonance { get; set; }
    [Render(Title = "NanoDSF assay")]
    public BiotechNanoDSFAssay NanoDsfAssay { get; set; }
    public BiotechMicroscaleThermo LabeledMicroscaleThermophoresis { get; set; }
    public bool LabelFreeMicroscaleThermophoresis { get; set; }
    public bool IsothermalTitrationCalorimetry { get; set; }
    public bool DifferentialScanningCalorimetry { get; set; }
    [Render(Title = "UV/visible spectrometry")]
    public bool UVVisibleSpectrometry { get; set; }

    public BiotechCircularDichroism CircularDichroism { get; set; }
}

public class BiotechCircularDichroism
{
    [Required]
    public string WhichBufferIsUsed { get; set; }
}

public class BiotechMicroscaleThermo
{
    [Required]
    public string WhichDyeIsNeededForProteinLabelling { get; set; }
}

public class BiotechNanoDSFAssay
{
    [Required]
    public string AreTrpOrTyrAminoAcidsPresent { get; set; }
}

public class BiotechSurfacePlasmonResonance
{
    [Required]
    public string WhatKindOfSensorChipIsNeeded { get; set; }
}

#endregion

#region BiDiff

public class CfBiDiff
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    public string TypeOfAccess { get; set; }

    [Render(NoTitle = true)]
    [Required]
    [Selection]
    public BiXrayServices RequiredServices { get; set; } = new();
    [Render(Sizing = Sizing.Small)]
    public string OtherSpecialRequrests { get; set; }
}

public class BiXrayServices
{
    
    [Render(Title = "Small angle X-ray scattering (SAXS) measurement", GroupStart = "Small angle X-ray scattering")]
    public BiXraySmallAngle SmallAngle { get; set; }
    [Render(Title = "SAXS data processing")]
    public bool SaxsDataProcessing { get; set; }
    [Render(Title = "Assistance with SAXS data interpretation")]
    public bool AssistanceWithSaxsData { get; set; }
    [Render(Title = "Measurement of SAXS data at synchrotron radiation sources")]
    public BiXrayMeasurementOfDiffraction Synchroton { get; set; }

    [Render(Title = "In-situ (in the crystallisation plates) testing of crystal diffraction", GroupStart = "Single crystal X-ray diffraction")]
    public bool InSitu { get; set; }
    public BiXrayMeasurementOfDiffraction MeasurementOfSingleCrystalDiffractionData { get; set; }
    public BiXrayCrystalDehumidifyingExperiment CrystalDehumidifyingExperiment { get; set; }
    public bool DiffractionDataProcessing { get; set; }
    [Render(Title = "Assistance to solve a 3D structure")]
    public bool Assistance3D { get; set; }
    [Render(Title = "Measurement of X-ray diffraction data sets at synchrotron radiation sources")]
    public BiXrayMeasurementDiffSynchroton DiffSynchroton { get; set; }
}

// Detailed services

public class BiXraySmallAngle
{
    [Required]
    public string EstimatedNumberOfSamples { get; set; }
}

public class BiXrayMeasurementSynchroton
{
    [Required]
    public string EstimatedNumberOfSamples { get; set; }
}

public class BiXrayMeasurementOfDiffraction // TODO - group complete datase measurement
{
    [Render(Title = "Testing of diffraction quality (estimate number of crystals)")]
    public string TestingDiffractionQuality { get; set; }
    public string EstimatedNumberOfCrystals { get; set; }
    [Render(Tip = "options: MR, MIR, SAD, other")]
    public string ExpectedPhasingMethod { get; set; }
}

public class BiXrayCrystalDehumidifyingExperiment
{
    [Required]
    public string EstimatedNumberOfDays { get; set; }
}

public class BiXrayMeasurementDiffSynchroton
{
    [Required]
    public string EstimatedNumberOfCrystals { get; set; }
}

#endregion

#region BIC

public class CfBic
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    public string TypeOfAccess { get; set; }

    [Required]
    public List<BicSample> SamplesInformation { get; set; } = new() { new BicSample() };

    public BicGeneralSeviceInfo GeneralServiceInformation { get; set; } = new();
    [Selection]
    [Required]
    public BicServices ServicesInformation { get; set; } = new();
}

public class BicSample
{

    [Required]
    public string SampleName { get; set; }
    [Required]
    [Render(Tip = "Sample and buffer description - concentration, molecular weight, pH, theoretical pl, usage of His-Tag.", Sizing = Sizing.Small)]
    public string SampleDescription { get; set; }
    [Render(Title = "Do the samples present any risk to human health and/or environment?")]
    [Selection]
    public bool DoSamplesPresentRisk { get; set; }
    [Selection("1", "2", "3")]
    [Binded("DoSamplesPresentRisk", "True")]
    [Required]
    public string ClassOfRisk { get; set; }
    [Render(Title = "Is the sample recombinant?", NoteAfter = "If yes, please specify details in the Other specification field")]
    public bool Recombinant { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string SourceOfOrigin { get; set; }
    [Selection("active virus", "virulence factor", "toxin", "prion protein")]
    public List<string> TheSampleIs { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string OtherSpecifications { get; set; }


}

public class BicGeneralSeviceInfo
{
    [Render(Title = "Are you interested in data evaluation service (if relevant)?", NoteAfter = "If yes, please specify in which method")]
    [Required]
    [Selection]
    public bool DataEvalService { get; set; }
    [Required]
    [Render(NoTitle = true, Sizing = Sizing.Small)]
    [Binded("DataEvalService", "True")]
    public string SpecDataEvalService { get; set; }


    [Render(Title = "Are you interested in training in data processing (if relevant)?", NoteAfter = "If yes, please specify in which method")]
    [Required]
    [Selection]
    public bool TrainingInData { get; set; }
    [Required]
    [Render(NoTitle = true, Sizing = Sizing.Small)]
    [Binded("TrainingInData", "True")]
    public string SpecTrainingInData { get; set; }

    [Render(Title = "Are you interested in expert consulting assistance?", NoteAfter = "If yes, please specify in which method")]
    [Required]
    [Selection]
    public bool ExpertConsulting { get; set; }
    [Required]
    [Render(NoTitle = true, Sizing = Sizing.Small)]
    [Binded("ExpertConsulting", "True")]
    public string SpecExpertConsulting { get; set; }
}

public class BicServices
{
    public const string SPEC_TRAINING = "This type of experiment is supposed to be managed by the user itself after special training.";

    public BicCalorimetricTitrationService CalorimetricTitration { get; set; }
    [Render(Tip = SPEC_TRAINING)]
    public BicDifferentialCaloService DifferentialScanningCalorimetry { get; set; }
    [Render(Tip = SPEC_TRAINING)]
    public BicMicroscaleThermoService MicroscaleThermophoresis { get; set; }
    [Render(Title = "Differential scanning fluorimetry (Prometheus NT.48)", Tip = SPEC_TRAINING)]
    public BicDifferentialFluorService DifferentialScanningFluorimetry { get; set; }
    [Render(Title = "Surface plasmon resonance (Biacore T200)", Tip = SPEC_TRAINING)]
    public BicPlasmonBiaService SurfacePlasmonBia { get; set; }
    [Render(Title = "Biolayer interferometry (Octet RED96e)", Tip = SPEC_TRAINING)]
    public BicBiolayerService BiolayerInterferometry { get; set; }
    [Render(Title = "Surface plasmon resonance (SFR Imaging multichannel system)", Tip = SPEC_TRAINING)]
    public BicPlasmonBiaService BicSurfacePlasmonBia { get; set; }
    public BicUltracentrifugationService AnalyticalUltracentrifugation { get; set; }
    [Render(Tip = SPEC_TRAINING)]
    public BicDynamicLightScatteringService DynamicLightScattering { get; set; }
    [Render(Title = "CD / fluorescence measurement", Tip = SPEC_TRAINING)]
    public BicCdService CdService { get; set; }
    public BicProteinCrystallService ProteinCrystallization { get; set; }

}

public class BicCalorimetricTitrationService
{
    [Selection]
    [Required]
    public CaloTitEquipment EquipmentRequested { get; set; } = new();

    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }

}

public class CaloTitEquipment
{
    [Render(Title = "VP-iTC", Tip = "This type of equipment is supposed to be managed by the user itself after special training.")]
    public CaloTitVpITC VPITC { get; set; }
    [Render(Title = "AutoITC200", Tip = "This type of equipment is supposed to be managed by the technican of CF.")]
    public CaloTitAutoITC AutoITC { get; set; }
}

public class CaloTitVpITC
{
    [Render(Unit = "days", NoteAfter = "If you plan to use other liquids than water-based buffers, please specify in \"Other information\" field.")]
    [Required]
    public string TimeOfMeasurementRequired { get; set; }
}

public class CaloTitAutoITC
{
    [Required]
    public string NumberOfSamples { get; set; }
    public string NumberOfReplicatesPerSample { get; set; }
    [Selection]
    public bool BlankMeasurementRequired { get; set; }
    public string TotalMeasurementsExpected { get; set; }
    [Render(Title = "Do you request measurement at different temperatures?", NoteAfter = "If Yes, please specify, experiments are routinely performed at 25°C")]
    public bool MeasurementsAtDiffTemperatures { get; set; }
    [Render(NoTitle = true, Sizing = Sizing.Small)]
    public bool SpecMeasurementsAtDiffTemperatures { get; set; }
    [Selection("Standard titration experiments", "Competitive titration experiments for low or ultrahight affinity interactions", "Single titration injection (continuous titration)")]
    [Required]
    public List<string> TypeOfExperiments { get; set; }
    [Selection(
        "Automated evaluation using in-build software fitting (one independent binding site model and/or one independent binding site model with fixed stoichiometry)",
        "Expert evaluation (one or two independent binding site model, cooperativity, competitive binding, kinetics)"
    )]
    [Required]
    public List<string> MethodOfEvaluation { get; set; } = new();
}

public class BicDifferentialCaloService
{
    [Render(Unit = "days")]
    [Required]
    public string TimeOfMeasurementRequired { get; set; }
    [Render(NoteAfter = "If you plan to use other liquids than water / water-based buffers, please specifiy in \"Other information\" field.")]
    public string TheTemperatureRangeOfTheExperiment { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form")]
    public string OtherInformation { get; set; }

}

public class BicMicroscaleThermoService
{
    [Selection("Monolith NT.115", "Monolith NT.115 Pico")]
    [Required]
    public List<string> UserInstrumentForMeasurement { get; set; } = new();
    [Required]
    [Render(Tip = "1 run = 1 binding curve of 16 points for Microscale Thermophoresis")]
    public string NumberOfRunsRequired { get; set; }
    [Render(Title = "Used label (for Monolith NT.115)")]
    [Binded("UserInstrumentForMeasurement", "Monolith NT.115")]
    public string UsedLabel { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicDifferentialFluorService
{
    [Required]
    public string NumberOfRunsRequired { get; set; }
    [Render(Title = "Extinction coef. at 280 nm (for Prometheus NT.48)")]
    [Required]
    public string ExtinctionCoef { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicPlasmonBiaService
{
    [Render(Unit = "days")]
    [Required]
    public string TimeOfMeasurementRequired { get; set; }
    [Selection("I will use my own chip", "I need a chip provided by CF")]
    [Required]
    public string SensorChipsForMeasurement { get; set; }
    [Render(Title = "Chip type and number of chips requested (if provided by CF)", Sizing = Sizing.Small)]
    public string ChipTypeNumber { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicBiolayerService
{
    [Render(Unit = "days")]
    [Required]
    public string TimeOfMeasurementRequired { get; set; }

    [Selection("I will use my own biosensors", "I need biosensors provided by CF")]
    [Render(NoteAfter = "<ul></li>Amine Reactive Second-Generation (AR2G) Biosensors</li><li>Protein A(ProA) Biosensors</li><li>Streptavidin(SA) Biosensors</li><li>Anti - HIS(HIS2) Biosensors</li></ul>")]
    [Required]
    public string BiosensorsForMeasurement { get; set; }
    [Render(Title = "Type and number of sensors requested (if provided by CF)", Sizing = Sizing.Small)]
    public string SensorsTypeNumber { get; set; }

    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }

    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicPlasmonSfrService
{
    [Render(Unit = "days")]
    [Required]
    public string TimeOfMeasurementRequired { get; set; }
    [Selection("I will use my own chip", "I need a chip provided by CF")]
    [Render(NoteAfter = "Note: the Core Facility provides the unmodified sensor chips with gold layer only. If you require the chip coating services, please specify in \"Other information\" field.")]
    [Required]
    public string SensorChipsForMeasurement { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperimentsPlanned { get; set; }

    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }

}

public class BicUltracentrifugationService
{
    [Required]
    public string NumberOfSamples { get; set; }
    public string NumberOfTotalMeasurementsExpected { get; set; }
    [Selection("Sedimentation velocity (1 day/experiment)", "Sedimentation equilibrium (4 days/experiment)")]
    [Required]
    [Render(NoteAfter = "Note that max 3 samples may be measured in one experiment.")]
    public List<string> TypeOfExperiments { get; set; } = new();
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicDynamicLightScatteringService
{
    [Selection("DelsaMax (cuvette measurements)", "Spectrolight 600 (plate reader measurements)")]
    [Required]
    public string UsedInstrumentForMeasurement { get; set; }

    [Required]
    [Render(Unit = "hours")]
    public string TimeOfMeasurementsRequired { get; set; }
    [Required]
    public string TemperatureOfExperiments { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperiments { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicCdService
{
    [Required]
    [Render(Unit = "hours")]
    public string TimeOfMeasurementsRequired { get; set; }
    [Selection(
        "1mm path", "2mm path", "5mm path",
        "10mm path", "5mm fluorescence compatible",
        "10mm fluorescence compatible", "Other - please specify in Other infomation below"
    )]
    [Required]
    public List<string> CuvetteTypesRequired { get; set; }

    [Render(NoteAfter = "Please specify in the “Other information” field below. This requires technical assistence. (Standard configuration includes: peltier, standard cuvette, CD detector, fluorescence monochromator).")]
    public bool NeedToChangeTheTypicalConfiguration { get; set; }

    [Render(Sizing = Sizing.Small)]
    public string TypeOfExperiments { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicProteinCrystallService
{
    [Required]
    public string NumberOfSamples { get; set; }

    public BicProteinTechniques RequestedTechniques { get; set; }
    [Render(Title = "Other information, or if more description is needed than is covered by the form", Sizing = Sizing.Small)]
    public string OtherInformation { get; set; }
}

public class BicProteinTechniques
{
    [Render(Title = "Standard screnn set-up", Tip = "Specify screens required in \"Other information\" field.")]
    public BicTechniqueStandard Standard { get; set; }

    public BicTechniquePlate PlateStorageAndInspection { get; set; }
    public BicOptimization OptimizationOfCrystallization { get; set; }
    [Render(Tip = "Specify in \"Other information\" field.")]
    public string AdvancedCrystallizationTechniques { get; set; }
}

public class BicTechniqueStandard
{
    public string  NumberOfScreenPlatesPerSampleRequired { get; set; }
}

public class BicTechniquePlate
{
    [Render(Title = "Prolonged inspection period: 4 + ", Unit = "weeks")]
    public string ProlongedInspectionPeriod { get; set; }
}

public class BicOptimization
{
    public string NumberOfScreenPlatesPerSampleRequired { get; set; }
}

#endregion

#region CRYO

public class CfCryoEM
{
    [Required]
    public List<CfCryoSample> SamplesInformation { get; set; } = new() { new CfCryoSample() };

    [Required]
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    public string TypeOfAccess { get; set; }

    [Selection]
    [Required]
    public CfCryoRequiredServicesSelection RequiredServices { get; set; }
}

public class CfCryoRequiredServicesSelection
{
    [Render(Tip = "Available only for users who passed the training for microscope operation at CEMCOF.")]
    public bool AccessToMicroscopes { get; set; }
    [Render(Title = "TEM imaging")]
    public bool TemImaging { get; set; }
    [Render(Title = "SEM imaging")]
    public bool SemImaging { get; set; }
    public bool SampleVitrification { get; set; }
    public bool ResingEmbedding { get; set; }
    [Render(Title = "FIB-SEM tomography")]
    public bool FibSemTomography { get; set; }
    [Render(Tip = "Sample preparation and imaging by negative staining EM to support sample suitability for structural characterization by electron microscopy.")]
    public bool InitialStructuralScreening { get; set; }
    [Render(Title = "Initial cryo-EM screening", Tip = "Optimization of sample vitrification conditions and sample imaging to provide sufficient data to prove sample quality for high-end data collection.")]
    public bool InitialCryoScreening { get; set; }
    [Render(Title = "Cryo-FIB lamella preparation", Tip = "Only available for life-science specimen.")]
    public bool CryoFibLamellaPreparation { get; set; }
    public bool SingleParticleAnalysisDataCollection { get; set; }
    [Render(Title = "Cryo-electron tomography data collection")]
    public bool CryoElectronTomography { get; set; }
    public bool ElectronDiffractionTomography { get; set; }
    [Render(Tip = "Available in limited number, prolonged service duration possible.")]
    public bool DataAnalysis { get; set; }
}
public class CfCryoSample
{
    [Required]
    public string SampleName { get; set; }
    [Required]
    [Render(Tip = "CEMCOF facility can handle BSL 1 and BSL 2 type of samples. In case of BSL 2 samples, attach appropriate data sheets comprising information on sample handling and decontamination.")]
    public string BiosafetyLevel { get; set; }

    [Render(Title = "Number of aliquotes/grids.")]
    public string NumberOfAliquotesAndGrids { get; set; }
    [Render(Tip = "When sample aliquotes are provided.")]
    public string ConcentrationOfSampleStock { get; set; }
    [Render(Tip = "Provide information on molecular size of the assembly to be studied or expected size of the objects to be imaged.")]
    public string TotalMolecularMassOrSize { get; set; }
    [Required]
    public string BufferOrSolventComposition { get; set; }
    [Selection("RT", "4C", "-20C", "-80C", "LN2")]
    public string DeliveryConditions { get; set; }
    [Selection("RT", "4C", "-20C", "-80C", "LN2")]
    public string StorageConditions { get; set; }
    [Render(Tip = "Origin, oligomeric state, stability, purity, decontamination procedure.")]
    public string AdditionalSampleInformation { get; set; }
}

#endregion

#region XRAY

public class CfXray
{
    [Selection(ProposalCommonStrings.SPA, ProposalCommonStrings.BAG)]
    [Required]
    [Render(NoteAfter = "For Single project access, please also provide sample information.")]
    public string TypeOfAccess { get; set; }

    [Required]
    public XrayServices RequiredServices { get; set; } = new();

    public List<XrayBioSample> BiologicalSamplesInformation { get; set; } = new();
    [Render(Title = "Non-biological sample(s) information")]
    public List<XrayNonbioSample> NonbioSamplesInformation { get; set; } = new();

}

public class XrayServices
{
    [Render(Title = "Non-biological SAXS service", Tip = "SAXS characterization of non-biological nanostructures is provided only as a measurement, without any advanced analysis of resulting SAXS data.")]
    public XrayNonBioService NonBioService { get; set; }

    [Render(Title = "Biological SAXS service")]
    public XrayBioService BioService { get; set; }
    public XrayDiffService DiffractionService { get; set; }
    [Render(Title = "X-ray service")]
    public XrayStandardService StandardService { get; set; }
}

public class XrayNonBioService
{
    [Render(Title = "Estimated number of samples for non-biological SAXS characterization for a selected period of time", GroupStart = "Numerical quantification of requirement for non-biological SAXS characterization")]
    [Required]
    public string EstimatedNumberOfSamples { get; set; }

    [Selection("week", "month", "year")]
    [Required]
    public string TimePeriod { get; set; }
}

public class XrayBioService
{
    [Selection("Basic characterization of biological macromolecules by SAXS",
               "Determination of a 3 - D shape of macromolecules by SAXS")]
    [Required]
    public List<string> ServicesRequired { get; set; }

    [Selection("Full service (measurement, data processing and data analysis provided by CF)",
               "Measurement without analysis of SAXS data")]
    [Required]
    public string ModeOfService { get; set; }

    [Render(Title = "Estimated number of samples for biological SAXS characterization for a selected period of time", GroupStart = "Numerical quantification of requirement for biological SAXS characterization")]
    [Required]
    public string EstimatedNumberOfSamples { get; set; }

    [Selection("week", "month", "year")]
    [Required]
    public string TimePeriod { get; set; }
}

public class XrayDiffService
{
    [Required]
    [Selection("Data collection and solving of the crystal structures with non - biological single crystals",
               "Collection of diffraction data with non - biological single crystals")]
    public List<string> DiffractionServiceRequired { get; set; }

    [Selection("Full service (measurement, data processing and data analysis provided by CF)",
               "Measurement without analysis of SAXS data")]
    [Required]
    public string ModeOfService { get; set; }

    [Render(Title = "Estimated number of samples for non-biological X-ray diffraction experiments for a selected period of time", GroupStart = "Numerical quantification of requirement for non-biological single crystal diffraction")]
    [Required]
    public string EstimatedNumberOfSamples { get; set; }

    [Selection("week", "month", "year")]
    [Required]
    public string TimePeriod { get; set; }
}

public class XrayStandardService
{
    [Required]
    [Selection("Preliminary non robotized X - ray tests(e.g test of cryoprotectant, test protein / salt, test of diffraction quality of single crystal) with biological sample",
               "Robotized test of diffraction quality of protein crystals",
               "Collection of diffraction data with crystals of biological macromolecules"
    )]
    public List<string> DiffractionServiceRequired { get; set; }

    [Selection("Full service (measurement, data processing and data analysis provided by CF)",
               "Measurement without analysis of SAXS data")]
    [Required]
    public string ModeOfService { get; set; }

    [Render(Title = "Estimated/required number of hours of X-ray time for a selected period of time", GroupStart = "Numerical quantification of requirements for biological single crystal diffraction")]
    [Required]
    public string EstimatedNumberOfSamples { get; set; }

    [Selection("week", "month", "year")]
    [Required]
    public string TimePeriod { get; set; }
}

public class XrayBioSample
{
    [Required]
    public string SampleName { get; set; }

    [Required]
    [Render(Sizing = Sizing.Small)]
    public string SampleDescription { get; set; }

    [Selection]
    [Required]
    [Render(Title = "Is the sample an active?")]
    public bool IsTheSampleAnActive { get; set; }

    [Selection]
    [Required]
    [Render(Title = "Does the sample present any risk to human health and/or environment?", NoteAfter = "If yes, please specify in the Other information field")]
    public bool SampleRisk { get; set; }

    [Selection("1", "2", "3", "4")]
    public string TheBiosafetyLevel { get; set; }

    [Selection]
    [Required]
    [Render(Title = "GMO?")]
    public bool IsGMO { get; set; }

    [Render(Sizing = Sizing.Small)]
    public string TheSourceOfOrigin { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string PrecautionsForSafeHandling { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string PersonalProtectiveEquipment { get; set; }
    [Render(Sizing = Sizing.Small)]
    public string OtherSpecifications { get; set; }

}

public class XrayNonbioSample
{
    [Required]
    public string SampleName { get; set; }

    [Required]
    [Render(Sizing = Sizing.Small)]
    public string SampleDescription { get; set; }

    [Render(Title = "Does the sample present any risk to human health and/or environment?", NoteAfter = "If yes, please specify in the Other information field")]
    [Selection]
    [Required]
    public bool RiskToEnv { get; set; }

    [Selection(
        "Explosive", "Flammable",
        "Health hazard(1)", "Health hazard(2)", "Acute toxicity",
        "Oxidizing", "Corrosive", "Environmental hazard"
    )]
    public string TheSampleIsExpectedToBe { get; set; }

    public string OtherHazards { get; set; }

    [Selection("Danger", "Warning", "None")]
    public string SignalWord { get; set; }

    public string DescriptionOfFirstAidMeasures { get; set; }
    public string PrecautionsForSafeHandling { get; set; }

    [Render(Title = "Conditions for safe storage, including any incompatibilities")]
    public string ConditionsForSafeStorage { get; set; }

    public string PersonalProtectiveEquipment { get; set; }
    public string Reactivity { get; set; }
    public string ChemicalStability { get; set; }
    public string PossiblityOfHazardousReactionWith { get; set; }
    public string IncompatibleWithMaterials { get; set; }
}

#endregion