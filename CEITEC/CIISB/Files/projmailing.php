<?php 
namespace emcf\projectman;

use emcf\mailing\Mail;
use emcf\mailing\MailOutputGate;
use emcf\mailing\ThreadManager;
use emcf\mailing\templates\TemplateRenderer;
use emcf\Config;
use emcf\utils\WKPDF;
use emcf\projectman\facilities\Facility;
use emcf\utils\GeneralUtils;
use emcf\utils\Person;

class ProjMailing 
{
    public static function proposalSubmitUser(Project $project, $cfList = NULL, $notifyUser = true)
    {
        $mo         = new MailOutputGate();
        $pid        = $project->getID();
        $projtitle  = $project["project_title"]->getPrettyValue();
        $name       = $project->applicant->getName();
        $email      = $project->applicant->getEmail();
        $acronym    = $project["acronym"]->getPrettyValue();
        $dur_till   = CIISBProlong::computeExpirationDate()->format("d.m.Y");

        // Prepare PDF of the project proposal
        $proposalURL    = $project->getWkUrl();       
        $attachfile     = WKPDF::render_to_tmp_file($proposalURL);

        // Send email to the user -------------------------------------------------
        if ($notifyUser)
        {
            $specText = ($project->hasPeerReview()) ? "Your project will be now undergo two step evaluation process which comprises technical feasibility evaluation done by facility manager(s) and peer-review evaluation performed by a member
                of our review committee." : "Your project will be now undergo one step evaluation process which comprises technical feasibility evaluation done by facility manager(s).";
            $subjectUser = "[CIISB #$pid] $projtitle";
            $textUser = "<p>
                Dear $name, <br><br>
                thank you for submitting your project to CIISB. Your project has been assigned project identification number (PID) $pid. 
                Please always use the PID in the subject of your email for any communication related to this project.
                <br>
                $specText We will contact you back once we obtain the results of the project evaluation.
                <br>
                In case of a successful evaluation of your project proposal, the maximal duration of the project is till $dur_till.
                <br>
                With kind regards, <br>
                Your CIISB team";
    
            $mail               = new Mail();
            $mail->setRelatedProject($project);
            $mail->Subject      = $subjectUser;
            $mail->Body         = $textUser;
            $mail->addAddress($email, $name);
            $mail->addAttachment($attachfile, "proposal_$pid.pdf", Mail::ENCODING_BASE64, "application/pdf");
    
            $mo->send_ciisb_systemmail($mail);
        }
        // ------------------------------------------------------------------------

        // Send emails to all facility heads. -------------------------------------
        $mail               = new Mail();
        $mail->setRelatedProject($project);
        $pmanlink           = GeneralUtils::linkify(ProjectManager::getProjectManagementLink());
        $techevallink       = GeneralUtils::linkify($project->getTechEvalFormUrl());

        $subj_part          = (!$notifyUser) ? "Project reminder" : "New project"; // Not notifiying user means sending the reminder
        $subjectHead        = "[CIISB #$pid] $subj_part - $acronym";
        $textHead = "New project $projtitle has been submitted to your facility by user $name. <br>
            The project has been assigned PID $pid. <br>
            The project is now waiting for technical feasibility evaluation. You can directly perform the technical evaluation by clicking the link below <br>
            $techevallink <br>
            You can also assign a person responsible for the project $pmanlink.";
        $mail->Subject  = $subjectHead;
        $mail->Body     = $textHead;
        
        $facTargets = ($cfList === NULL) ? $project->getFacilityKeys() : $cfList;
        $contacts = Person::selectFacHeadsMailNames($facTargets);
        foreach ($contacts as $key_mail => $val_name) 
        {
            $mail->addAddress($key_mail, $val_name);
        }

        // Send this to project admins as well
        $contacts_admins = $project->getProjAdmins();
        foreach ($contacts_admins as $key_mail => $value_name) 
        {
            $mail->addCC($key_mail, $value_name);
        }

        $mail->addAttachment($attachfile, "proposal_$pid.pdf", Mail::ENCODING_BASE64, "application/pdf");
        $mo->send_ciisb_systemmail($mail);
    }

    public static function projectAcceptation($project)
    {
        $mo             = new MailOutputGate();
        $pid            = $project->getID();
        $proposalTitle  = $project["project_title"]->getPrettyValue();
        $email_app      = $project->applicant->getEmail();
        $name_app       = $project->applicant->getName();
        $email_pri      = $project->principle->getEmail();
        $name_pri       = $project->principle->getName();

        // Mail will be sent to principle, with copy to applicant and blind copies to employers
        $mail               = new Mail();
        $mail->setRelatedProject($project);
        $mail->Subject      = "[CIISB #$pid] Project accepted";
        $body_has_terms     = "
            Dear $name_pri, <br><br>
            we are happy to inform you that proposal <i>$proposalTitle</i> passed the revision. Please see the general terms and conditions document and rates attached to the next email which apply when accessing our laboratory through CIISB research infrastructure.  <br>
            Your project will be under service after we obtain your confirmation of the terms.
            <br>
            Kind regards, <br>
            Your CIISB team
        ";

        $body_no_terms      = "
            Dear $name_pri, <br><br>
            we are happy to inform you that your proposal <i>$proposalTitle</i> passed the revision. See attached document/s for details about technical feasibility evaluation.<br>
            Your project is now under service.<br>

            Kind regards, <br>
            Your CIISB team
        ";

        $mail->Body = ($project->hasTerms()) ? $body_has_terms : $body_no_terms;

        // Attach technical feasibilities
        foreach ($project->tech_evals as $teval) 
        {
            $evalUrl = $teval->getWkUrl($pid);
            $evalPdf = WKPDF::render_to_tmp_file($evalUrl);
            $fac = Facility::FAC_SHORTCUT_MAP[$teval["eval_facility"]->getValue()];
            $fac = \str_replace(" ", "_", $fac);
            $mail->addAttachment($evalPdf, "evaluation_tech_${fac}_${pid}.pdf", Mail::ENCODING_BASE64, "application/pdf");
        }

        // Recipients ---------------------------------------------------------------
        $mail->addAddress($email_pri, $name_pri); // Principle investigator
        $mail->addCC($email_app, $name_app); // Copy to applicant
        $contacts_bcc = $project->getProjAdmins();
        $contacts_bcc = \array_merge($contacts_bcc, $project->getProjFacHeads());
        foreach ($contacts_bcc as $key_mail => $val_name)        // Facility heads
            $mail->addBCC($key_mail, $val_name);
        // --------------------------------------------------------------------------

        $mo->send_ciisb_systemmail($mail);
        
        self::send_terms($project);
    }

    public static function send_terms($project, $org = null)
    {
        if (!$project->hasTerms())
            return;

        $send_biocev = ($org === null || $org === "biocev") && $project->hasTerms("biocev");
        $send_ceitec = ($org === null || $org === "ceitec") && $project->hasTerms("ceitec");

        $mo = new MailOutputGate();
        $thm = new ThreadManager();
        
        $pid = $project->getID();
        $pi_name = $project->principle->getName();
        $pi_email = $project->principle->getEmail();
        $terms = new CIISBTerms($project);
        $cfnamesbi = implode(", ", $project->getFacilityShortcuts("biocev"));
        $cfnamesce = implode(", ", $project->getFacilityShortcuts("ceitec"));
        $padm_ceitec = Person::mailNamesByRole("r_project_admin_ceitec");
        $padm_biocev = Person::mailNamesByRole("r_project_admin_biocev");
       
        if ($send_biocev)
        {
            $mail = new Mail();
            $mail->addAttachment($terms->generate_biocev());
            $mail->setRelatedProject($project);
            $mail->addAddress($pi_email, $pi_name); // Principle investigator
            $mail->addCC($project->applicant->getEmail(), $project->applicant->getName()); // Applicant to copy

            foreach ($padm_biocev as $mailaddr => $name) {
                $mail->addCC($mailaddr, $name); // Copy to biocev admin
            }

            $mail->Subject = "[CIISB #$pid] " . CIISBTerms::BIOCEV_THREAD;
            $mail->Body = 
"Dear $pi_name, 

Please see the attached Agreement on usage of the research infrastructure which applies when accessing our laboratories through the CIISB research infrastructure. Please fill out the Agreement in the attachment, sign it, and send back a scanned copy. 
Your project will be under service once we obtain the signed Agreement.

Kind regards,

Your CIISB team";

            // Attach pricelists
            foreach ($project->facilities as $fac) {
                if ($fac->getOrg() != "biocev") continue;
                $paths = $fac->getRateFilePaths();
                foreach ($paths as $path) {
                    if (\file_exists($path))
                        $mail->addAttachment($path);
                }
            }          

            $mo->send_ciisb_systemmail($mail, false, ", reply to this email and attach a signed scan of the Agreement. Make sure to send the reply from this address: $pi_email", true);
            $thread = $thm->ensure_thread($mail, array_merge($padm_biocev, [$pi_email => $pi_name]));
            $mail->thread_id = $thread->getID();
            $mail->insert(false, true);
        }

        if ($send_ceitec)
        {
            $mail = new Mail();
            $mail->addAttachment($terms->generate_ceitec());
            $mail->setRelatedProject($project);
            $mail->addAddress($pi_email, $pi_name); // Principle investigator
            $mail->addCC($project->applicant->getEmail(), $project->applicant->getName()); // Applicant to copy

            foreach ($padm_ceitec as $mailaddr => $name) {
                $mail->addCC($mailaddr, $name); // Copy to ceitec admin
            }

            $mail->Subject = "[CIISB #$pid] " . CIISBTerms::CEITEC_THREAD;
            $mail->Body = 
"Dear $pi_name, 

Please see the attached Agreement on usage of the research infrastructure which applies when accessing our laboratories through the CIISB research infrastructure. 
The rates attached to this email apply to utilization of instrumentation and services at $cfnamesce.
By replying to this email, you accept the rates for utilization of our equipment for your project and general terms and conditions for access to CIISB research infrastructure. 
No further action on your project will be taken until we receive your confirmation.

Kind regards,

Your CIISB team";

            // Attach pricelists
            foreach ($project->facilities as $fac) {
                if ($fac->getOrg() != "ceitec") continue;
                $paths = $fac->getRateFilePaths();
                foreach ($paths as $path) {
                    if (\file_exists($path))
                        $mail->addAttachment($path);
                }
            }          

            $mo->send_ciisb_systemmail($mail, false, ", reply to this email to accept the conditions of access to the CIISB research infrastructure. Make sure to send the reply from this address: $pi_email", true);
            $thread = $thm->ensure_thread($mail, array_merge($padm_ceitec, [$pi_email => $pi_name]));
            $mail->thread_id = $thread->getID();
            $mail->insert(false, true);
        }      
    }

    public static function projectTermination($project, $evaluation)
    {
        $mo             = new MailOutputGate();
        $upon           = ($evaluation->getOutcome() == Evaluation::EVAL_ACCEPTED_UPON); 
        $type           = ($evaluation->getType());
        $pid            = $project->getID();
        $projtitle      = $project["project_title"]->getPrettyValue();
        $userName       = $project->applicant->getName();
        $userEmail      = $project->applicant->getEmail();
        $proposalLink   = GeneralUtils::linkify($project->getProposalFormUrl());
        $facKey         = ($type == Evaluation::EVAL_TYPE_PEER) ? $project["cf_peer_req"]->getValue() : $evaluation["eval_facility"]->getValue();
        $facManagers    = $project->getProjFacHeads([$facKey]);
        $facManager     = array_keys($facManagers)[0];
        $facManagerName = $facManagers[$facManager];
        $eval_comments  = $evaluation["eval_comments"]->getPrettyValue();

        $techSubject = "[CIISB #$pid] Project terminated at technical feasibility level";
        $techMessage = "
            Dear $userName, <br><br>
            we regret to inform you that the goals of your project are not feasible using instrumentation and expertise available. Please see the technical feasibility report in the attachment for more information. The project $pid is now terminated. If you want to re-submit your project, please submit a new proposal using the link below. <br>
            $proposalLink <br>
            
            Kind regards, <br>
            Your CIISB team"; 

        $peerSubject = "[CIISB #$pid] Project terminated at peer-review level";
        $peerMessage = "
            Dear $userName, <br><br>
            we regret to inform your proposal has been rejected during the peer-review process. Please see the peer-review report below for more information. The project $pid is now terminated. If you want to re-submit your project, please submit a new proposal which reflects reviewer’s concerns using the link below.<br>
            $proposalLink <br>
            
            <br>
            -------- Peer review -------- <br>
            $eval_comments
            <br><br>

            Kind regards, <br>
            Your CIISB team"; 

        $uponSubject = "[CIISB #$pid] Additional information required for project approval";
        $uponMessage = "
            Dear $userName, <br><br>
            evaluation of your proposal revealed that additional information is required to accept your project $projtitle. Please see the attached document for more details. The project $pid is now terminated. If you want to re-submit your project, please submit a new proposal which reflects reviewer's concerns using the link below.<br>
            $proposalLink <br>
            
            Kind regards,<br>
            Your CIISB team";

        $mail = new Mail();
        $mail->setRelatedProject($project);

        // Recipients ---------------------------------------------------------------
        $mail->addAddress($userEmail);                          // User
        $contacts_bcc = $project->getProjAdmins();
        $contacts_bcc = \array_merge($contacts_bcc, $project->getProjFacHeads());
        foreach ($contacts_bcc as $key_mail => $val_name)        // Facility heads
            $mail->addBCC($key_mail, $val_name); 
        // --------------------------------------------------------------------------

        // Prepare PDF of the project proposal
        //$proposalURL     = $project->getWkUrl();       
        //$proposalPdf     = WKPDF::render_to_tmp_file($proposalURL);
        // Prepare PDF of evaluation
        
        // Setup subjects, bodies, attachments
        if ($upon)
        {
            $mail->Subject  = $uponSubject; 
            $mail->Body     = $uponMessage;
        }
        else 
        {
            $mail->Subject  = ($evaluation->isTech()) ? $techSubject : $peerSubject;
            $mail->Body     = ($evaluation->isTech()) ? $techMessage : $peerMessage;
        }
        
        if ($type == Evaluation::EVAL_TYPE_TECH)
        {
            $evalUrl         = $evaluation->getWkUrl($pid);
            $evalPdf         = WKPDF::render_to_tmp_file($evalUrl);
            $mail->addAttachment($evalPdf, "evaluation_{$type}_{$pid}.pdf", Mail::ENCODING_BASE64, "application/pdf");
        }

        $mo->send_ciisb_systemmail($mail);
    }
            
    public static function peerReviewReminder(Project $project)
    {
        $mo                 = new MailOutputGate();
        $pid                = $project->getID();
        $projtitle          = $project["project_title"]->getPrettyValue();
        $userName           = $project->applicant->getName();
        $facHeads           = $project->getProjFacHeads([$project["cf_peer_req"]->getValue()]);
        $facName            = $facHeads[array_keys($facHeads)[0]];
        $revMailname        = $project->getPeerReviewerMailName();
        $reviewerName       = $revMailname["name"];
        $reviewerAddr       = $revMailname["email"];
        $proposalLink       = GeneralUtils::linkify($project->getProposalURL());
        $reviewLink         = GeneralUtils::linkify($project->getPeerRevFormUrl());

        $mail               = new Mail();
        $mail->setRelatedProject($project);
        $mail->Subject      = "[CIISB #$pid] Project revision";
        $mail->Body         = "Dear $reviewerName, <br><br>
            we would like to kindly ask you to revise the project entitled $projtitle submitted to CIISB infrastructure by $userName. You can find the project proposal in the attachment or you can access the proposal using the link below. <br>
            $proposalLink <br>
            
            Please submit your review into the form available on the link below within coming four weeks. <br>
            $reviewLink <br>
            
            Thank you very much for your help. <br>
            
            Kind regards, <br>
            $facName";

        $mail->addAddress($reviewerAddr);

        // Prepare PDF of the project proposal
        $proposalURL    = $project->getWkUrl();       
        $attachfile     = WKPDF::render_to_tmp_file($proposalURL);
        $mail->addAttachment($attachfile, "proposal_$pid.pdf", Mail::ENCODING_BASE64, "application/pdf");

        $mo->send_ciisb_systemmail($mail);
    }

    public static function techFeasAssign(Project $project, $facKey, $person)
    {
        $mo             = new MailOutputGate();
        $pid            = $project->getID();
        $projtitle      = $project["project_title"]->getPrettyValue();
        $address        = $person->getEmail();
        $proposalLink   = GeneralUtils::linkify($project->getProposalURL());
        $reviewLink     = GeneralUtils::linkify($project->getTechEvalFormUrl());

        $mail               = new Mail();
        $mail->setRelatedProject($project);

        $mail->Subject      = "[CIISB #$pid] Project responsibility";
        $mail->Body         = "Facility head assigned you revision of technical feasibility of project \"$projtitle\".<br>
            More details about the project can be found using the link below.<br>
            $proposalLink <br>
            You can directly perform the technical evaluation by clicking the link below <br>
            $reviewLink";

        $mail->addAddress($address);

        $mo->send_ciisb_systemmail($mail);
    }

    public static function serviceFinishMail(Project $project, $body_template, $facKey = "")
    {
        $mo = new MailOutputGate();
        $tr = new TemplateRenderer($project);
        $thm = new ThreadManager();
        extract($tr->getDefaultSubstitutes());
        

        $mail = new Mail();
        $mail->setRelatedProject($project);

        $mail->Subject  = "[CIISB #$pid] Servis finished";
        $mail->Body     = $tr->render_html($body_template, ["cf_shortcut" => $project->getFacilityShortcut($facKey)]);

        $mail->addAddress($app_email, $app_name);
        $mail->addAddress($pi_email, $pi_name);
        $ccs = array_merge($proj_admins, $cf_heads);
        foreach ($ccs as $em => $nam) 
        {
            $mail->addCC($em, $nam);
        }
        
        $thread = $thm->ensure_thread($mail, array_merge($proj_admins, [$pi_email => $pi_name, $app_email => $app_name]));
        $mo->send_ciisb_systemmail($mail, false, " Respond to the email in order to submit the publication(s) resulting from the utilization of CIISB core facility/ies.");
        $mail->thread_id = $thread->getID();
        $mail->insert(false, true);
    }

    public static function serviceStartMail(Project $project)
    {
        $mo = new MailOutputGate();
        $tr = new TemplateRenderer($project);
        extract($tr->getDefaultSubstitutes());
        $mail = new Mail();
        $mail->setRelatedProject($project);

        $contact_list = $project->getProjFacHeads();
        $contact_list_text = "";
        foreach ($contact_list as $key => $value) 
        {
            $contact_list_text .= "$value - $key\n";
        }

        $mail->Subject  = "[CIISB #$pid] Servis started";
        $mail->Body = $tr->render_html("service_start", ["cf_contact_list" => $contact_list_text]);

        $mail->addAddress($app_email, $app_name);
        $mail->addCC($pi_email, $pi_name);
        foreach ($contact_list as $key => $value) 
        {
            $mail->addCC($key, $value);
        }

        $mo->send_ciisb_systemmail($mail);
    }

    public static function sendServiceCloseToTerm(Project $project)
    {
        $mo = new MailOutputGate();
        $tr = new TemplateRenderer($project);
        $prolong = new CIISBProlong($project);
        extract($tr->getDefaultSubstitutes());
        $mail = new Mail();
        $mail->setRelatedProject($project);
        $form_link = GeneralUtils::linkify($prolong->generateProlongLink());

        $mail->addAddress($app_email, $app_name);
        $mail->addAddress($pi_email, $pi_name);
        $mail->addCCS($proj_admins);
        $mail->addCCS($cf_heads);

        $mail->Subject = "[CIISB #$pid] Service close to termination";
        $mail->Body = $tr->render_html("service_close_term", ["form_link" => $form_link]);

        $mo->send_ciisb_systemmail($mail);
    }

    public static function sendProlongTechRequest(Project $project_previous, Project $project_new)
    {
        $mo         = new MailOutputGate();
        $tr         = new TemplateRenderer($project_previous);
        $prolong    = new CIISBProlong($project_previous);
        $mail       = new Mail();
        $mail->setRelatedProject($project_previous);
        extract($tr->getDefaultSubstitutes());
        $pmanlink  = GeneralUtils::linkify(ProjectManager::getProjectManagementLink());

        $pid_new = $project_new->getID();
        $pid_prev = $project_previous->getID();
        

        $mail->addRecps($proj_admins);
        $mail->addRecps($cf_heads);

        $mail->Subject = "[CIISB #$pid_new] Request for $pid_prev project extension";
        $mail->Body = $tr->render_html("extension_tech_review", ["pman_link" => $pmanlink, "justification" => $project_previous["prolong_justification"]->getValueEsc()]);

        $mo->send_ciisb_systemmail($mail);
    }

    public static function sendProlongAcceptation(Project $project)
    {
        $pid        = $project->getID();
        $old_pid    = $project["prolonged_pid"]->getValue();
        $mo         = new MailOutputGate();
        $tr         = new TemplateRenderer($project);
        $prolong    = new CIISBProlong($project);
        $expiration_date = $prolong->compExpirationDate()->format("d.m.Y");
        $mail       = new Mail();
        $mail->setRelatedProject($project);
        
        extract($tr->getDefaultSubstitutes());

        // Attach technical feasibilities
        foreach ($project->tech_evals as $teval) 
        {
            $evalUrl = $teval->getWkUrl($pid);
            $evalPdf = WKPDF::render_to_tmp_file($evalUrl);
            $fac = Facility::FAC_SHORTCUT_MAP[$teval["eval_facility"]->getValue()];
            $fac = \str_replace(" ", "_", $fac);
            $mail->addAttachment($evalPdf, "evaluation_tech_${fac}_${pid}.pdf", Mail::ENCODING_BASE64, "application/pdf");
        }
        

        // Recipients ---------------------------------------------------------------
        $mail->addAddress($app_email, $app_name); // To applicant
        $mail->addCC($pi_email, $pi_name); // Copy to principle investigator
        $mail->addBCCs($proj_admins);
        $mail->addBCCs($cf_heads);
        // --------------------------------------------------------------------------

        $mail->Subject = "[CIISB #$pid] Extension of the project $old_pid accepted";
        $mail->Body = $tr->render_html("extension_acceptation", ["new_pid" => $pid, "expiration_date" => $expiration_date]);
        $mo->send_ciisb_systemmail($mail);
    }
    
    public static function sendProlongRejection(Project $project, Evaluation $bad_eval)
    {
        $mo         = new MailOutputGate();
        $tr         = new TemplateRenderer($project);
        $prolong    = new CIISBProlong($project);
        $mail       = new Mail();
        $mail->setRelatedProject($project);
        $fac_key    = $bad_eval["eval_facility"]->getValue();
        extract($tr->getDefaultSubstitutes());
        $proposal_link = GeneralUtils::linkify($project->getProposalFormUrl());

        $mail->addAddress($app_email, $app_name);
        $mail->addAddress($pi_email, $pi_name);
        $mail->addCCS($proj_admins);
        $mail->addCCS($cf_heads);

        $old_pid = $project["prolonged_pid"]->getValue();

        $mail->addBCCs($proj_admins);
        $mail->addBCCs($cf_heads);

        $mail->Subject = "[CIISB #$pid] Extension of the project $old_pid rejected";
        $mail->Body = $tr->render_html("extension_rejection", ["proposal_link" => $proposal_link]);

        $evalUrl         = $bad_eval->getWkUrl($pid);
        $evalPdf         = WKPDF::render_to_tmp_file($evalUrl);
        $mail->addAttachment($evalPdf, "evaluation_tech_{$pid}.pdf", Mail::ENCODING_BASE64, "application/pdf");

        $mo->send_ciisb_systemmail($mail);
    }
}