using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;
using Microsoft.Data.SqlClient;

namespace PPICancerRecognitionProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                                        AttachDbFilename=|DataDirectory|\Data\Database.mdf;
                                        Integrated Security=True;
                                        Connect Timeout=30";
            // ✅ 2️⃣ Define model outputs root (next to the database file)
            string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString() ?? AppDomain.CurrentDomain.BaseDirectory;
            string modelOutputsFolder = Path.Combine(dataDirectory, "model_outputs");

            if (!Directory.Exists(modelOutputsFolder))
                Directory.CreateDirectory(modelOutputsFolder);

            var patientRepo = new PatientRepository(connectionString);
            var scanRepo = new CTScanRepository(connectionString);
            var aiRepo = new AIModelOutputRepository(connectionString, modelOutputsFolder);

            try
            {
                var newPatient = new Patient
                {
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1980, 5, 23)
                };
                int patientId = patientRepo.Add(newPatient);
                Console.WriteLine($"✅ Added patient with ID: {patientId}");

                var newScan = new CTScan
                {
                    PatientID = patientId,
                    FileCode = "Scan_001",
                    RelativePath = @"RDBMC\Scan_001.dcm"
                };
                int scanId = scanRepo.Add(newScan);
                Console.WriteLine($"✅ Added CT scan with ID: {scanId}");

                
                byte[] fakeImageBytes = new byte[] { 0x42, 0x4D, 0x46, 0x00, 0x00, 0x00, 0x00, 0x00 }; // small dummy data

                var aiOutput = new AIModelOutput
                {
                    ScanID = scanId,
                    FileCode = "Output_001",
                    RelativePath = "", 
                    GenerationDate = DateTime.Now
                };

                int outputId = aiRepo.Add(aiOutput, fakeImageBytes);
                Console.WriteLine($"✅ Added AI output with ID: {outputId}");

                Console.WriteLine("🎉 Test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during test: {ex.Message}");
            }
            /*
            catch (SqlException sqlEx)
            {
                MessageBox.Show(
                    $"SQL ERROR:\n{sqlEx.Message}\n\nNumber: {sqlEx.Number}\nState: {sqlEx.State}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"GENERAL ERROR:\n{ex.Message}\n\n{ex.StackTrace}",
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            */
        }
    }
 }
