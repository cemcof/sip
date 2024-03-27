# EMAN - technical documentation

EMAN (EM Management) is a web application used for facilitation and automation of workflow and administration
of scientific facilities.

Main goal of this guide is to firstly describe an architecture of the application, identify it's concepts
and areas.

When this is understood, we will go through the process of setting up, configuring and deploying the application at a facility.

Ultimately, we will elaborate on how to further develop and extend the application with features required by specific facility.

## Part 1 - Features, concepts and architecture

The system can be seen as consisting of three major areas:
- Project management (administrative)
- Facility overview (informative)
- Data management (executive, processing)

Some of the features shared by these areas are:

- User store. Authentication of internal application users via external providers (Saml2, Google, orcid)
- Organization hierarchy, project types, status types.
- Emails. Both incoming and outgoing. Automatic system emails triggered by the workflow. Storing emails in threads.
- Documents. Support for storing, generating and sending documents with variety of formats.
  Conversion of HTML documents to PDF, templating .docx files.
- Workflows. Configuring how each project type for each organization reacts to the events, such as email receive or daily check.

The system can be seen as separated to three major layers. The data layer contains data classes that are mapped to the relational database using ORM framework.
It is therefore responsible for data persistence. The application layer works with the data layer . The application layer essentially contains everything that is
not related to data persistence, data modelling and graphical user interface.
The presentation layer is responsible for displaying graphical user interface to the user.
To put it simply, data layer models and stores data of the system.
The application layer performs all operations on the data.
The presentation layer presents the data to the user and enables user to interact with the application layer using GUI.


![Screenshot](Doc/architecture.drawio.png)

### Organizations

The system supports multiple "organizations", each separately configured, but handled by a single deployment unit.
Organization in the system maps to real-life organization, such as research center, research facility, university, etc.
Definition of an organization in the system contains information about the organization, such as name, abbreviation, unique identifier
and optionally it's child organizations, which enables creating hierarchical organization structure.

A real example of defined Organization structure can bee seen in the following image:

![fff](Doc/org_tree.drawio.png)

#### Centers

All use cases of EMAN cannot be implemented by a single webserver. 
The research facility is expected to have it's own internal network and computers, 
where other modules of the system must be deployed to cover the features (e.g. data transfering from 
instruments, remote access to instruments...) of the facility. The webserver is then understood
as a central unit that offers internal API to the separate modules of the facility.

In the nomenclature of the system, the facility itself understood by the webserver is just an organization, 
but the whole internal facility infrastructure is called "center".

Each center than maps to its organization on the webserver, using the API and authenticating itself 
with a secret key.


The center is however optional, for example to cover features related to administrative project management,
it is enough to configure only organization on the webserver without actually deploying any other modules in the facility.  


### Users & Authentication

EMAN keeps internal store of the users. By privileged users, other users and their roles within the organizations are 
managed. The system supports multiple authentication providers, such as Google or ORCID, with a possibility to implement more.
However, these providers are just identities and must be mapped to the actual user within the system. That user is
then assigned roles and used for authorization process.
Using external identity for the first time therefore creates a new internal user with no permissions, which are then assigned 
later by a privileged user. 
However, attaching external logins to existing internal users is possible through email invitation links.

**Role** is an important entity that can be assigned to a user and gives the user certain meaning within the system.
Roles are assigned in pair with an organization, which enables one user to have different roles in different organizations.

The role/organization pairs are then used through the system for authorization and GUI rendering purposes.

The actual meanings of the roles and the application behaviour related to them is not generally implemented 
and their definition for the facility needs is up to the developer deploying the system.

### Projects, statuses, documents and emails

The system is designed to be used for administrative project management.
These entities are always needed in this area and therefore are implemented in the core of the system.

The most important thing is **organization-project-status** relation.

A **project** goes through a lifecycle and during this
process it alter's it's status, either automatically (e.g. recieving an email, deadline expiration...) or
or through user's or administrator's intervention.
The project can be in any predefined status for any predefined organization.

The projects are obviously also related to the users. While the 
relationship between user and organization is called a role, the relationship between user and project is called
a membership. Membership can be also optionally scoped to a specific organization.
Similarly to roles, membership types existing in the system are predefined by the developer. 

During lifecycle of the project
arbitrary **documents** can be attached to it. The system implements tooling related to documents, such as 
generating PDFs from HTML, templating .docx files, sending email attachments, etc.

**Document** is related to specific project and organization and contains it's primary file and attachments.

**Proposal** is a special type of document that extends it with information such as who
created the document, who should evaluate it, when it was evaluated, in which state it is, etc. 
Also, custom GUI components for rendering the proposal and it's evaluation form are attached to the proposals.

For example, **project proposal** can be a document that is submitted by a user through a custom web form and that 
triggers creation of new project. Afterward **technical feasibility proposals** are documents that are sent
to the facility heads for evaluation.


The email support is also important part of the system. 
The system can not only emit emails to the users, but receive emails, parse them and attach them to the project, which enables 
reacting to them for example by changing status of the project, which might be a required
feature for the custom project lifecycle.


Although base and abstract implementation of the projects and statuses is part ot the system,
specific required project types, their statuses and workflows are predefined by the developer.



## Part 2 - Installation and setup

EMAN webserver runs on the latest `.NET` environment from Microsoft. Since it is a web application, `ASP.NET Core framework`, which is part
of `.NET` ecosystem, is used.

- Install opensource `.NET 8 SDK`
- Install opensource `PostgreSQL` database.
- Set up a database user in `PostgreSQL` with enough permissions -do this by authenticating as `postgres` user, creating a new user and granting it permissions to create databases.
```sudo -u postgres psql -c "CREATE USER eman WITH PASSWORD '********';"```
```sudo -u postgres psql -c "ALTER USER eman WITH CREATEDB;"```
- Optional: If postgres database should accept connection from external network, edit `postgresql.conf` and set `listen_addresses = '*'`. Then edit `pg_hba.conf` and add `host all all
- Configure the database connection in [appsettings.Production.yml](lims/appsettings.Production.yml) under `Db->ConnectionString`, connection string 
should look like this: `Host=localhost; Database=eman; Username=eman; Password=********`
- Run `dotnet ef database update` in order to create the database and set up the tables. You will need to [install ef tools](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) before this.
- Configure server endpoints in [appsettings.Production.yml](lims/appsettings.Production.yml) under `Kestrel->Endpoints`. Here provide URLs and ports the server should listen on. Also specify path to SSL certificate. To allow `dotnet` to map to privileged ports (e.g. 80,443), use `sudo setcap CAP_NET_BIND_SERVICE=+eip <path to dotnet binary NOT A SYMLINK>`
- Check rest of [appsettings.Production.yml](lims/appsettings.Production.yml) and configure properties related to the environment, for example email SMTP server.
- Adjust and install `systemd` service file from template [lims.service](lims.service). Here specify path to dotnet binary, path to published application (working directory) and a user that will run the process.



## Part 3 - Adaptation, extension and development

Before deploying and using the system, developer must define and configure the concepts described above.
However, the one doing this must be familiar with several areas related to software development and web.

For configuring and deploying the system according to following manual, only basic knowledge of these areas will suffice.
However, for actually extending the system with new features, deeper understanding and more experience in these ares will be necessary.

This manual does not cover this, but includes links to relevant documentation and resources.

- .NET 8 and C# - syntax, basic concepts and usage 
- OOP (object oriented programming) - concepts like classes, interfaces, inheritance, polymorphism, methods, etc.
- YAML
- [ASP.NET Core framework](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- Python, if centers and lims nodes are required


### Define the organization tree

### Define index redirecting 

When user opens the webpage, redirection to a URL relevant to the user should happen. 
This typically depends on the role within the system. 
For example, project administrator will probably want to see the list of projects, whereas lab operator will be
be interested in data transfer from microscopes. Unauthenticated user should be redirected to the login page, etc.

This can be configured by implementing `IIndexRedirector` interface and registering it in the DI container.

### Define the user roles

### Define the project types and statuses



### Useful curl requests

To use these requests, set `$apikey` with the valid API key and `$expid` with the desired experiment ID.

```bash
# Request data expiration for an experiment
curl -X PATCH --location "https://condenser.ceitec.muni.cz/api/experiments/$expid" \
-H "Lims-Organization: $apikey" \
-H "Content-Type: application/jsonpatch+json" \
-d '{"op":"replace","path":"/Storage/State","value":"ExpirationRequested"}'
```

```bash
# Request data archivation for an experiment
curl -X PATCH --location "https://condenser.ceitec.muni.cz/api/experiments/$expid" \
-H "Lims-Organization: $apikey" \
-H "Content-Type: application/jsonpatch+json" \
-d '{"op":"replace","path":"/Storage/State","value":"ArchivationRequested"}'
```

```bash
# Get experiments
curl -X GET --location "https://condenser.ceitec.muni.cz/api/experiments" \
-H "Lims-Organization: $apikey"
```

```bash
```