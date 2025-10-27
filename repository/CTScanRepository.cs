using Microsoft.Data.SqlClient;
using PPICancerRecognitionProject.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.repository
{
    public class CTScanRepository
    {
        private readonly string _connectionString;

        public CTScanRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(CTScan scan)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
            INSERT INTO CTScans (PatientID, FileCode, RelativePath, ScanDate)
            VALUES (@PatientID, @FileCode, @RelativePath, @ScanDate);
            SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", (object)scan.PatientID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FileCode", scan.FileCode);
                    command.Parameters.AddWithValue("@RelativePath", scan.RelativePath);
                    command.Parameters.AddWithValue("@ScanDate", scan.ScanDate);

                    connection.Open();
                    scan.ScanID = Convert.ToInt32(command.ExecuteScalar());
                    return scan.ScanID;
                }
            }
        }

        public CTScan GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT ScanID, PatientID, FileCode, RelativePath, ScanDate FROM CTScans WHERE ScanID = @ScanID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScanID", id);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CTScan
                            {
                                ScanID = (int)reader["ScanID"],
                                PatientID = reader["PatientID"] as int?,
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                ScanDate = (DateTime)reader["ScanDate"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public CTScan GetByFileCode(string fileCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT ScanID, PatientID, FileCode, RelativePath, ScanDate FROM CTScans WHERE FileCode = @FileCode";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileCode", fileCode);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CTScan
                            {
                                ScanID = (int)reader["ScanID"],
                                PatientID = reader["PatientID"] as int?,
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                ScanDate = (DateTime)reader["ScanDate"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<CTScan> GetByPatientId(int patientId)
        {
            var scans = new List<CTScan>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT ScanID, PatientID, FileCode, RelativePath, ScanDate FROM CTScans WHERE PatientID = @PatientID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PatientID", patientId);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            scans.Add(new CTScan
                            {
                                ScanID = (int)reader["ScanID"],
                                PatientID = reader["PatientID"] as int?,
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                ScanDate = (DateTime)reader["ScanDate"]
                            });
                        }
                    }
                }
            }
            return scans;
        }
    }
}
