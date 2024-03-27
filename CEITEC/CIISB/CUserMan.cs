using sip.Projects;
using sip.Userman;

namespace sip.CEITEC.CIISB;

// Application roles.
// These are the relations between user-organization, not user-project-organization
public class ProjectAdminRole : RoleDefinition;
public class CfStaffRole : RoleDefinition;
public class CfHeadRole : RoleDefinition;
public class PeerReviewerRole : RoleDefinition;
public class AdminObserverRole : RoleDefinition;


public class ApplicantMember : MemberRef;
public class PrincipalMember : MemberRef;
public class AdditionalMember : MemberRef;
public class PeerReviewerMember : MemberRef;
public class ResponsibleMember : MemberRef;

