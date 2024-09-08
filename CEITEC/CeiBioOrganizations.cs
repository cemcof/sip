namespace sip.CEITEC;

// ======== Organization types ==========
public class ResearchInfrastructure(string id) : Organization(id);
public class ResearchCenter(string id) : Organization(id);
public class ResearchFacility(string id) : Organization(id);
public class Company(string id) : Organization(id);


public class InfrastructureOrg : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        
        opts.OrganizationDetails = new ResearchInfrastructure(Name)
        {
            Abbreviation = "CEITEC/BIOCEV",
            Description = "CEITEC and BIOCEV root organization",
            Name = "CEITEC and BIOCEV"
        };
    }
}

public class Ceitec : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        opts.OrganizationDetails = new ResearchCenter(Name)
        {
            Name = "Ceitec",
            Url = "https://www.ceitec.cz/",
            Abbreviation = "CEITEC",
        };
        
        opts.SetParent<InfrastructureOrg>();
    }
    
    public class CfBic : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Biomolecular Interaction and Crystallization",
                Url = "http://bic.ceitec.cz/",
                Abbreviation = "CF BIC"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfCryo : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Cryo-Electron Microscopy and Tomography",
                Url = "https://www.ceitec.eu/cryo-electron-microscopy-and-tomography-core-facility/cf94",
                Abbreviation = "CF CryoEM",
                LinkId = "cryoem"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfNano : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Nanobiotechnology",
                Url = "https://www.ceitec.eu/nanobiotechnology-core-facility/cf104",
                Abbreviation = "CF Nanobio"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfXray : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "X-ray Diffraction and Bio-SAXS Core Facility",
                Url = "https://www.ceitec.eu/cryo-electron-microscopy-and-tomography-core-facility/cf94",
                Abbreviation = "CF X-ray"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfProt : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Proteomics",
                Url = "https://www.ceitec.eu/proteomics-core-facility/cf95",
                Abbreviation = "CF Prot"
            };
            
            opts.SetParent<Ceitec>();
        }
    }

    public class CfNmr : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Josef Dadok National NMR Centre",
                Url = "http://nmr.ceitec.cz/",
                Abbreviation = "CF NMR"
            };
            
            opts.SetParent<Ceitec>();
        }
    }
}

public class Biocev : OrganizationDefinition
{
    public override void Setup(OrganizationOptions opts)
    {
        opts.OrganizationDetails = new ResearchCenter(Name)
        {
            Name = "Biocev",
            Url = "https://www.biocev.eu/en",
            Abbreviation = "BIOCEV",
        };
        
        opts.SetParent<InfrastructureOrg>();
    }

    public class BTech : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Biophysical techniques",
                Url = "",
                Abbreviation = "CF BioTech"
            };
            
            opts.SetParent<Biocev>();
        }
    }
    
    public class BCryst : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Crystallization of proteins and nucleic acids",
                Url = "",
                Abbreviation = "CF Cryst"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BDiff : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Diffraction techniques",
                Url = "",
                Abbreviation = "Cf Diff"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BSpec : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Structural mass spectrometry",
                Url = "",
                Abbreviation = "CF Spec"
            };
            
            opts.SetParent<Biocev>();
        }
    }

    public class BProtProd : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new ResearchFacility(Name)
            {
                Name = "Protein production",
                Url = "",
                Abbreviation = "Cf ProtProd"
            };
            
            opts.SetParent<Biocev>();
        }
    }
    
    public class Eyen : OrganizationDefinition
    {
        public override void Setup(OrganizationOptions opts)
        {
            opts.OrganizationDetails = new Company(Name)
            {
                Name = "EYEN",
                Url = "",
                LinkId = "eyen",
                Abbreviation = "EYEN"
            };
        }
    }
}


