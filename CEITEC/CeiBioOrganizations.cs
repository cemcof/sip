namespace sip.CEITEC;

// ======== Organization types ==========
public class ResearchInfrastructure(string id, string linkId, string name, string abbreviation)
    : Organization(id, linkId, name, abbreviation);
public class ResearchCenter(string id, string linkId, string name, string abbreviation) 
    : Organization(id, linkId, name, abbreviation);
public class ResearchFacility(string id, string linkId, string name, string abbreviation) 
    : Organization(id, linkId, name, abbreviation);
public class Company(string id, string linkId, string name, string abbreviation) 
    : Organization(id, linkId, name, abbreviation);


public class InfrastructureOrg : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        opts.OrganizationDetails = new ResearchInfrastructure(
            id: Name, 
            linkId: "ceibio", 
            name: "CEITEC/BIOCEV", 
            abbreviation: "CEITEC/BIOCEV")
        {
            Description = "CEITEC and BIOCEV root organization",
            DisplayName = "CEITEC and BIOCEV"
        };
    }
}
public class Ceitec : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        opts.OrganizationDetails = new ResearchCenter(
            id: Name, 
            linkId: "ceitec", 
            name: "CEITEC", 
            abbreviation: "CEITEC")
        {
            Url = "https://www.ceitec.cz/",
            Description = "CEITEC Research Center",
            DisplayName = "Central European Institute of Technology"
        };
        
        opts.SetParent<InfrastructureOrg>();
    }

    public class CfBic : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-bic", 
                name: "Biomolecular Interaction and Crystallization", 
                abbreviation: "CF BIC")
            {
                Url = "http://bic.ceitec.cz/",
                Description = "Core Facility for BIC",
                DisplayName = "Biomolecular Interaction and Crystallization"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfCryo : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cryoem", 
                name: "INSTRUCT-CZ_CIISB_CEMCOF", 
                abbreviation: "CF CryoEM")
            {
                Url = "https://www.ceitec.eu/cryo-electron-microscopy-and-tomography-core-facility/cf94",
                DisplayName = "Cryo-Electron Microscopy and Tomography",
                Description = "Core Facility for Cryo-EM"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfNano : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-nanobio", 
                name: "Nanobiotechnology", 
                abbreviation: "CF Nanobio")
            {
                Url = "https://www.ceitec.eu/nanobiotechnology-core-facility/cf104",
                Description = "Core Facility for Nanobiotechnology",
                DisplayName = "Nanobiotechnology"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfXray : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-xray", 
                name: "X-ray Diffraction and Bio-SAXS Core Facility", 
                abbreviation: "CF X-ray")
            {
                Url = "https://www.ceitec.eu/cryo-electron-microscopy-and-tomography-core-facility/cf94",
                Description = "Core Facility for X-ray Diffraction",
                DisplayName = "X-ray Diffraction and Bio-SAXS"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfProt : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-prot", 
                name: "Proteomics", 
                abbreviation: "CF Prot")
            {
                Url = "https://www.ceitec.eu/proteomics-core-facility/cf95",
                Description = "Core Facility for Proteomics",
                DisplayName = "Proteomics"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfNmr : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-nmr", 
                name: "Josef Dadok National NMR Centre", 
                abbreviation: "CF NMR")
            {
                Url = "http://nmr.ceitec.cz/",
                Description = "Core Facility for NMR",
                DisplayName = "Josef Dadok National NMR Centre"
            };
            
            opts.SetParent<Ceitec>();
        }
    }
}

public class Biocev : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        opts.OrganizationDetails = new ResearchCenter(
            id: Name, 
            linkId: "biocev", 
            name: "Biocev", 
            abbreviation: "BIOCEV")
        {
            Url = "https://www.biocev.eu/en",
            Description = "BIOCEV Research Center",
            DisplayName = "Biotechnology and Biomedicine Center"
        };
        
        opts.SetParent<InfrastructureOrg>();
    }

    public class BTech : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-biotech", 
                name: "Biophysical techniques", 
                abbreviation: "CF BioTech")
            {
                Url = "",
                Description = "Core Facility for Biophysical Techniques",
                DisplayName = "Biophysical Techniques"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BCryst : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-cryst", 
                name: "Crystallization of proteins and nucleic acids", 
                abbreviation: "CF Cryst")
            {
                Url = "",
                Description = "Core Facility for Crystallization",
                DisplayName = "Crystallization of Proteins and Nucleic Acids"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BDiff : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-diff", 
                name: "Diffraction techniques", 
                abbreviation: "Cf Diff")
            {
                Url = "",
                Description = "Core Facility for Diffraction",
                DisplayName = "Diffraction Techniques"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BSpec : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-spec", 
                name: "Structural mass spectrometry", 
                abbreviation: "CF Spec")
            {
                Url = "",
                Description = "Core Facility for Structural Mass Spectrometry",
                DisplayName = "Structural Mass Spectrometry"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BProtProd : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(
                id: Name, 
                linkId: "cf-protprod", 
                name: "Protein production", 
                abbreviation: "Cf ProtProd")
            {
                Url = "",
                Description = "Core Facility for Protein Production",
                DisplayName = "Protein Production"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class Eyen : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new Company(
                id: Name, 
                linkId: "eyen", 
                name: "EYEN", 
                abbreviation: "EYEN")
            {
                Url = "",
                Description = "EYEN Company",
                DisplayName = "EYEN"
            };
        }
    }
}
