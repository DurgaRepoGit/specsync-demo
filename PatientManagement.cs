using Reqnroll;
using specsync_demo.tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace specsync_demo.tests.StepDefinitions
{
    [Binding]
    public class PatientManagement
    {
        [Given(@"a patient with MRN ""(.*)"" is admitted to room ""(.*)"" as ""(.*)"" ""(.*)""")]
        public void GivenAPatientIsAdmittedToRoom(string mrn, string roomName, string firstName, string lastName)
        {
            int parsedMrn = int.Parse(mrn);
            // Reusing your database helper method cleanly!
            DBHelper.AdmitPatientToRoom(
                roomName: roomName,
                famName: lastName,
                givenName: firstName,
                mrn: parsedMrn,
                gender: 'M',
                birthMonth: 5,
                birthDay: 12,
                birthYear: 1985
            );
        }

        [Given(@"the patient with MRN ""(.*)"" is discharged from the system")]
        public void GivenThePatientIsDischargedFromTheSystem(string mrn)
        {
            DBHelper.GetPatientIDByMRN(mrn);
            DBHelper.DischargePatientByMRN(mrn);
        }
    }
}
