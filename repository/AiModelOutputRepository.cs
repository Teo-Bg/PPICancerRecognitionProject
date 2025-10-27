using Microsoft.Data.SqlClient;
using PPICancerRecognitionProject.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.repository
{
    public class AIModelOutputRepository
    {
        private readonly string _connectionString;
        private readonly string _modelOutputsBasePath;

        public AIModelOutputRepository(string connectionString, string modelOutputsBasePath)
        {
            _connectionString = connectionString;
            _modelOutputsBasePath = modelOutputsBasePath;
        }

        public int Add(AIModelOutput output, byte[] imageBytes)
        {
            // Step 1️⃣: Get the scan’s FileCode to know which folder to use
            string scanFileCode = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT FileCode FROM CTScans WHERE ScanID = @ScanID;";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScanID", output.ScanID);
                    connection.Open();
                    scanFileCode = command.ExecuteScalar() as string;
                }
            }

            if (string.IsNullOrEmpty(scanFileCode))
                throw new Exception($"No CT scan found with ID {output.ScanID}.");

            // Step 2️⃣: Create folder if it doesn’t exist
            string scanFolder = Path.Combine(_modelOutputsBasePath, scanFileCode);
            if (!Directory.Exists(scanFolder))
                Directory.CreateDirectory(scanFolder);

            // Step 3️⃣: Determine file path
            // Add a .png extension automatically (or adjust as needed)
            string fileName = output.FileCode.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                ? output.FileCode
                : output.FileCode + ".png";

            string fullFilePath = Path.Combine(scanFolder, fileName);
            File.WriteAllBytes(fullFilePath, imageBytes);

            // Step 4️⃣: Store relative path
            output.RelativePath = Path.Combine(scanFileCode, fileName);

            // Step 5️⃣: Insert into DB
            using (var connection = new SqlConnection(_connectionString))
            {
                string insertQuery = @"
            INSERT INTO AI_Model_Outputs (ScanID, FileCode, RelativePath, GenerationDate)
            VALUES (@ScanID, @FileCode, @RelativePath, @GenerationDate);
            SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@ScanID", output.ScanID);
                    command.Parameters.AddWithValue("@FileCode", output.FileCode);
                    command.Parameters.AddWithValue("@RelativePath", output.RelativePath);
                    command.Parameters.AddWithValue("@GenerationDate", output.GenerationDate);

                    connection.Open();
                    output.OutputID = Convert.ToInt32(command.ExecuteScalar());
                    return output.OutputID;
                }
            }
        }

        public AIModelOutput GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT OutputID, ScanID, FileCode, RelativePath, GenerationDate FROM AI_Model_Outputs WHERE OutputID = @OutputID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OutputID", id);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AIModelOutput
                            {
                                OutputID = (int)reader["OutputID"],
                                ScanID = (int)reader["ScanID"],
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                GenerationDate = (DateTime)reader["GenerationDate"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public AIModelOutput GetByFileCode(string fileCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT OutputID, ScanID, FileCode, RelativePath, GenerationDate FROM AI_Model_Outputs WHERE FileCode = @FileCode";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileCode", fileCode);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AIModelOutput
                            {
                                OutputID = (int)reader["OutputID"],
                                ScanID = (int)reader["ScanID"],
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                GenerationDate = (DateTime)reader["GenerationDate"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<AIModelOutput> GetByScanId(int scanId)
        {
            var outputs = new List<AIModelOutput>();
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT OutputID, ScanID, FileCode, RelativePath, GenerationDate FROM AI_Model_Outputs WHERE ScanID = @ScanID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ScanID", scanId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            outputs.Add(new AIModelOutput
                            {
                                OutputID = (int)reader["OutputID"],
                                ScanID = (int)reader["ScanID"],
                                FileCode = (string)reader["FileCode"],
                                RelativePath = (string)reader["RelativePath"],
                                GenerationDate = (DateTime)reader["GenerationDate"]
                            });
                        }
                    }
                }
            }
            return outputs;
        }
    }
}
