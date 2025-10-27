using Microsoft.Data.SqlClient;
using PPICancerRecognitionProject.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.repository
{
    public class PatientRepository
    {
        private readonly string _connectionString;

        public PatientRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(Patient patient)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
            INSERT INTO Patients (FirstName, LastName, DateOfBirth)
            VALUES (@FirstName, @LastName, @DateOfBirth);
            SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", patient.FirstName);
                    command.Parameters.AddWithValue("@LastName", patient.LastName);
                    command.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);

                    connection.Open();
                    patient.PatientID = Convert.ToInt32(command.ExecuteScalar());
                    return patient.PatientID;
                }
            }
        }

        public Patient GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT PatientID, FirstName, LastName, DateOfBirth FROM Patients WHERE PatientID = @PatientID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientID", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Patient
                {
                    PatientID = (int)reader["PatientID"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    DateOfBirth = (DateTime)reader["DateOfBirth"]
                };
            }

            return null;
        }

        public List<Patient> GetAll()
        {
            var patients = new List<Patient>();
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT PatientID, FirstName, LastName, DateOfBirth FROM Patients";

            using var command = new SqlCommand(query, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                patients.Add(new Patient
                {
                    PatientID = (int)reader["PatientID"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    DateOfBirth = (DateTime)reader["DateOfBirth"]
                });
            }

            return patients;
        }
    }
}
