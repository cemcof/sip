<?php

namespace emcf\projectman;
use emcf\utils\XRM;

class CIISBCrm 
{
    const RESG_ID_CZECH     = "e174bdca-c3db-e611-9f54-005056991551";
    const RESG_ID_FOREIGN   = "6d6abad0-c3db-e611-9f54-005056991551";

    /**
     * Creates new project, checks if applicant exists in contacts and 
     * if not, creates new contact, assigns group and project to him
     */
    public function newProject(Project $p)
    {
        if (\strstr($p["project_title"]->getValue(), "[!CRM]")) // Testing?
            return;

        $app        = $p->applicant;
        $app_mail   = $app->getEmail();

        // First of all, create project
        $pname = "CIISB_" . $p->getID() . "_" . $p["acronym"]->getValue();
        $proj_crm_id = XRM::r_post("psa_projectses", ["psa_name" => $pname]);

        // Query person from the CRM
        $filter   = "emailaddress1 eq '$app_mail' or emailaddress2 eq '$app_mail' or emailaddress3 eq '$app_mail'";
        $select   = "contactid,_ge_defaultprojectid_value";
        $response = XRM::r_get("contacts", ['$filter' => $filter, '$select' => $select]);
        $cont_res = (count($response["value"]) == 0) ? false : $response["value"][0];
        $contid   = ($cont_res) ? $cont_res["contactid"] : false;

        if (!$contid)
        {
            // We need to create contact since we did not find it
            $groupid    = $p->isNational() ? self::RESG_ID_CZECH : self::RESG_ID_FOREIGN;
            $contid     = XRM::r_post("contacts", [
                "emailaddress1"                 => $app_mail,
                "firstname"                     => $app["firstname"]->getValue(),
                "lastname"                      => $app["surname"]->getValue(),
                "ge_primaryrgid@odata.bind"         => "/ge_res_groups($groupid)",
                "ge_defaultprojectid@odata.bind"    => "/psa_projectses($proj_crm_id)"
            ]);
        }
        else 
        {
            // Contact was found, we need to patch it's default project id, but only if it matches our project name
            $their_default_pid = $cont_res["_ge_defaultprojectid_value"];
            if ($their_default_pid && \preg_match("/^CIISB_[0-9]{6}C_.{1,10}$/", $their_default_pid))
            {
                XRM::r_patch("contacts($contid)", [
                    "ge_defaultprojectid@odata.bind" => "/psa_projectses($proj_crm_id)"
                ]);
            }
        }
        
         // We need to bind contact with the project by creating another entity
         $psa_teammember_id = XRM::r_post("psa_teammembers", [
            "psa_name"  => $app["surname"]->getValue() . "_" . $pname,
            "psa_projectid@odata.bind" => "/psa_projectses($proj_crm_id)",
            "psa_contactid@odata.bind" => "/contacts($contid)",
            "psa_role"                 => 1 
        ]);
    }
}