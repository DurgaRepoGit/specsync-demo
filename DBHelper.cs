using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System;
using System.Data;

namespace specsync_demo.tests.Helpers
{
    public static class DBHelper
    {
        private static readonly string ConnectionString = "Server=127.0.0.1\\BERNOULLI;Database=Bernoulli;User Id=sa;Password=cpc;TrustServerCertificate=True;";
        public static void DischargePatientByMRN(String mrn)
        {
            Console.WriteLine("about to discharge patient with mrn: " + mrn);
            if (mrn.Length == 0)
                return;
            RunQuery(String.Format("EXEC sp_HL7Discharge @patientmrn = '{0}', @DevicesRemoved = 1", mrn));
        }
        public static void AdmitPatientToRoom(string roomName, string famName, string givenName, int mrn, char gender, int birthMonth, int birthDay, int birthYear)
        {
            DateTime dob = new DateTime(birthYear, birthMonth, birthDay);

            RunQuery($"insert into PatientList (FamilyName, GivenName, MedicalRecNo, Gender, DateOfBirth) values ('{famName}', '{givenName}', {mrn}, '{gender}', '{dob:yyyy-MM-dd}')");

            var tempTable = RunReturnQuery($"select * from PatientList where MedicalRecNo='{mrn}'");
            var patientId = tempTable.Rows[0]["PatientId"];

            RunQuery($"update RoomList SET PatientId = {patientId} where RoomId='{roomName}'");
        }

        public static int GetPatientIDByMRN(string mrn)
        {
            string query = $"SELECT PatientID FROM PatientList WHERE MedicalRecNo = '{mrn}'";
            return ReturnCountQuery(query);
        }

        // Underlying database execution wrappers
        private static void RunQuery(string query)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

        private static DataTable RunReturnQuery(string query)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var adapter = new SqlDataAdapter(query, connection);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }
        public static int ReturnCountQuery(string countQuery)
        {
            try
            {
                //input a sql query in the form "select count..." and return the value as an integer
                DataTable dt = RunReturnQuery(countQuery);
                String returnedValue = dt.Rows[0][0].ToString();
                Console.WriteLine("Count: " + returnedValue + " for query: " + countQuery);
                int value = int.Parse(returnedValue);
                return value;
            }
            catch (Exception ex)
            {
                //TestLogger.LogError(ex);
                Console.WriteLine("Error with the query " + countQuery + ": returning -1");
                return -1;
            }
        }
    }
}
