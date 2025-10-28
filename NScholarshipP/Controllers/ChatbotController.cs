using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace NScholarshipP.Controllers
{
    public class ChatbotController : Controller
    {
        private static readonly Dictionary<string, string> _responses = new()
        {
            {"hi","How can I help you Sir/Mam?" },
            {"hello","How can I help you Sir/Mam?" },
            {"what is nsp", "The National Scholarship Portal (NSP) is a government initiative to provide scholarships to eligible students through a single platform." },
            {"apply scholarship", "You can apply by logging in as a student and clicking 'Apply' on your dashboard." },
            {"contact", "You can reach support at support@nsp.gov.in." },
            {"how can i register as a new student on the portal?","To register, visit the NSP homepage and click on 'New Registration'. Fill in your details and verify through your registered email ID." },
            {"what documents are required during registration?","You will need your Aadhaar number, bank account details, income certificate, and caste certificate for registration." },
            {"i forgot my password. how can i reset it?","Click on 'Forgot Password' on the login page and follow the steps to reset your password." },
            {"my account is locked. what should i do?","Kindly contact the team. Go to Contact Us and you can find the details." },
            {"how do i apply for a scholarship?","Login as a student and click on 'Apply for Scholarship' under the 'Application Form' section." },
            {"can i edit my application after submission?","You can edit your application only before final submission. Once submitted, no changes can be made." },
            {"what should i do if my application is marked as defective?","If your application is marked 'Defective', correct the mentioned errors and resubmit it before the deadline." },
            {"can i apply for more than one scholarship?","No, a student can apply for only one scholarship under NSP for a particular academic year." },
            {"what is the last date to submit the scholarship form?","The scholarship application deadline is usually announced on the 'Announcements' page of the portal." },
            {"how can i check my scholarship application status?","You can check your status by logging in and selecting 'Track Application Status'." },
            {"what does pending at institute mean","It means your application is under verification by your institute. Please contact your institute for updates." },
            {"my application was rejected - what can i do?","If your application is rejected, check the remarks provided and reapply in the next session after corrections." },
            {"how long does it take for the application to be verified?","It usually takes a few weeks depending on the number of applications received." },
            {"what does approved by ministry mean?","It means your application has been approved for disbursement. The amount will be credited soon." },
            {"how can my institute register on the portal?","Institutes can register under NSP by clicking on 'Institute Login' and then choosing 'Register as New Institute'." },
            {"who verifies the application at the institute level?","Your application is first verified by the State then Ministry Nodal Officer." },
            {"what is the role of the state nodal officer?","The State Nodal Officer is responsible for verifying and approving institute-level applications within the state." },
            {"my institute is not listed - what should i do?","If your institute is not listed, contact your institute administration to complete their NSP registration." },
            {"what is the difference between institute verification and state verification?","Institute verification checks student details, while state verification confirms eligibility and forwards to the ministry." },
            {"when will i receive my scholarship amount?","Once your application is approved by the ministry, the amount is directly credited to your Aadhaar-linked bank account." },
            {"how is the scholarship amount credited?","Scholarship amounts are credited through the Direct Benefit Transfer (DBT) system." },
            {"my scholarship payment failed - what should i do?","If your payment failed, ensure your Aadhaar and bank account are correctly linked and contact your bank." },
            {"how can i update my bank account details?","Bank details can only be updated before final submission or by contacting the nodal officer." },
            {"how do i check if my payment has been processed?","You can check your payment status on the 'Know Your Payment' link available on the NSP homepage." },
            {"the portal is not opening. what should i do?","Try clearing your browser cache or use a different browser. The portal may also be under maintenance." },
            {"i'm unable to upload my documents.","Ensure your file is below 1 MB and in PDF or JPEG format. Try again after refreshing the page." },
            {"how do i contact the helpdesk for technical issues?","Kindly go and check the Contact Us page and you will find the details of our team." },
            {"which browser should i use for better performance?","Use the latest version of Chrome or Microsoft Edge for best performance." },
            {"how can i contact the nsp support team?","You can contact the NSP support team at Contact Us page." },
            {"what is the official email address for queries?","Our official email ID is support@nsp.gov.in." },
            {"where is the nsp office located?","NSP Office: Block No. 12, 2nd Floor, Kendriya Sadan, Vijayawada – 520002, Andhra Pradesh." },
            {"is there a helpline number for students?","Helpline: +91 86884 33453 (Mon–Fri, 10 AM – 5 PM)." },
            {"what are the working hours for support?","Our support team is available Monday to Friday, 10 AM to 5 PM." },
            {"which scholarships are available on nsp?","You can view available scholarships by selecting your state and ministry under the 'Schemes' section on the homepage." },
            {"can students from any state apply?","Yes, students from all states can apply, provided they meet the eligibility criteria of the respective scheme." },
            {"are there scholarships for minority students?","Yes, scholarships for minority students are available under the Ministry of Minority Affairs." },
            {"what should i do if i find incorrect information on my profile?","If your profile has incorrect information, contact your institute or state nodal officer for correction." }
        };
        [HttpPost]
        public IActionResult GetResponse(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return Json(new { response = "Please ask something." });

            var msg = userMessage.ToLower();

            var reply = _responses.FirstOrDefault(x => msg.Contains(x.Key)).Value
                ?? _responses.FirstOrDefault(x => x.Key.Split(' ').Any(k => msg.Contains(k))).Value
                ?? "Sorry, I didn’t understand that.";
            return Json(new { response = reply });
        }
    }
}