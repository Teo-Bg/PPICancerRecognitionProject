using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using FellowOakDicom.Imaging.ImageSharp;
namespace PPICancerRecognitionProject

{
    public partial class Form1 : Form
    {
        private readonly PatientRepository _patientRepo;
        private readonly CTScanRepository _scanRepo;
        private readonly AIModelOutputRepository _aiRepo;
        private readonly string _aiOutputPath;
        private readonly string _projectBasePath;

        public Form1()
        {
            InitializeComponent();
            

            // change connectionString
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=MedicalImagingDB;Trusted_Connection=True;TrustServerCertificate=True;";
            // change path
            _projectBasePath = "C:\\Users\\Miha\\Desktop\\MPP\\PPICancerRecognitionProject";
            _aiOutputPath = Path.Combine(_projectBasePath, "outputs");

            _patientRepo = new PatientRepository(connectionString);
            _scanRepo = new CTScanRepository(connectionString);
            _aiRepo = new AIModelOutputRepository(connectionString, _aiOutputPath);

            LoadPatients();
        }
        
        private void LoadPatients()
        {
            lstPatients.Items.Clear();
            var patients = _patientRepo.GetAll();

            foreach (var p in patients)
                lstPatients.Items.Add($"{p.PatientID}: {p.FirstName} {p.LastName}");
        }

        // select patient - show scans
        private void lstPatients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null) return;

            lstScans.Items.Clear();
            picOriginal.Image = null;
            picAIResult.Image = null;
            lblAIInfo.Text = "";
            btnGenerateAI.Visible = false;

            int patientId = int.Parse(lstPatients.SelectedItem.ToString().Split(':')[0]);
            var scans = _scanRepo.GetByPatientId(patientId);

            foreach (var scan in scans)
            {
                var outputs = _aiRepo.GetByScanId(scan.ScanID);
                string label = outputs.Count > 0 ? $"{scan.ScanID}: {scan.FileCode} (AI ✅)" : $"{scan.ScanID}: {scan.FileCode}";
                lstScans.Items.Add(label);
            }
        }

        // select a scan - show image(s)
        private void lstScans_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstScans.SelectedItem == null)
            {
                Console.WriteLine("⚠️ No scan selected.");
                return;
            }

            // Reset UI
            picOriginal.Image = null;
            picAIResult.Image = null;
            lblAIInfo.Text = "";
            btnGenerateAI.Visible = false;

            try
            {
                // Parse scan ID from list item
                string selectedText = lstScans.SelectedItem.ToString();
                Console.WriteLine($"Selected item text: {selectedText}");
                int scanId = int.Parse(selectedText.Split(':')[0]);

                var scan = _scanRepo.GetById(scanId);
                var outputs = _aiRepo.GetByScanId(scanId);

                if (scan == null)
                {
                    Console.WriteLine($"No scan found with ID {scanId} in DB.");
                    lblAIInfo.Text = "Scan not found in database.";
                    return;
                }

                Console.WriteLine($"Loaded scan from DB: ID={scan.ScanID}, FileCode={scan.FileCode}, Path={scan.RelativePath}");

                // Full path to scan file
                string scanFullPath = Path.Combine(_projectBasePath, scan.RelativePath);
                Console.WriteLine($"Full scan path: {scanFullPath}");
                Console.WriteLine($"Project base path: {_projectBasePath}");

                // Check file existence
                if (!File.Exists(scanFullPath))
                {
                    Console.WriteLine("File does not exist at path above.");
                    lblAIInfo.Text = "Original scan file not found.";
                }
                else
                {
                    Console.WriteLine($"File exists: {scanFullPath}");
                    Console.WriteLine($"Extension: {Path.GetExtension(scanFullPath)}");

                    // Handle DICOM
                    if (Path.GetExtension(scanFullPath).Equals(".dcm", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Console.WriteLine("Rendering DICOM with ImageSharp backend...");

                            var dicomImage = new DicomImage(scanFullPath);
                            using (var imageSharp = dicomImage.RenderImage().AsSharpImage())
                            {
                                Console.WriteLine("Rendered ImageSharp DICOM image successfully.");
                                using (var ms = new MemoryStream())
                                {
                                    imageSharp.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                                    ms.Position = 0;
                                    using (var tmp = System.Drawing.Image.FromStream(ms))
                                    {
                                        picOriginal.Image?.Dispose();
                                        picOriginal.Image = new Bitmap(tmp);
                                    }
                                    Console.WriteLine("Converted ImageSharp image → Bitmap → displayed.");
                                }
                            }
                        }
                        catch (Exception dex)
                        {
                            Console.WriteLine("DICOM render failed:");
                            Console.WriteLine(dex.ToString());
                            lblAIInfo.Text = $"DICOM render error: {dex.Message}";
                        }
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine("Loading standard image format (PNG/JPG)...");
                            picOriginal.Image = System.Drawing.Image.FromFile(scanFullPath);
                            Console.WriteLine("Non-DICOM image displayed successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to load image file:");
                            Console.WriteLine(ex.ToString());
                            lblAIInfo.Text = $"Could not load image: {ex.Message}";
                        }
                    }
                }

                // ---------- AI OUTPUT SECTION ----------
                Console.WriteLine("---- Checking for AI Output ----");
                if (outputs.Count > 0)
                {
                    Console.WriteLine($"Found {outputs.Count} AI output(s) in DB.");
                    var output = outputs.First();
                    string aiFullPath = Path.Combine(_projectBasePath, output.RelativePath);
                    Console.WriteLine($"Full AI path: {aiFullPath}");

                    if (File.Exists(aiFullPath))
                    {
                        try
                        {
                            Console.WriteLine("Loading AI result image...");
                            picAIResult.Image = System.Drawing.Image.FromFile(aiFullPath);
                            lblAIInfo.Text = "Extracted features: textures ✓ keypoints ✓ shape ✓ intensity ✓ classification ✓";
                            Console.WriteLine("AI image loaded successfully.");
                        }
                        catch (Exception aiex)
                        {
                            Console.WriteLine("Failed to load AI image:");
                            Console.WriteLine(aiex.ToString());
                            lblAIInfo.Text = "Could not load AI result image.";
                        }
                    }
                    else
                    {
                        Console.WriteLine("AI result file not found on disk.");
                        lblAIInfo.Text = "AI result not found on disk.";
                    }
                }
                else
                {
                    Console.WriteLine("No AI outputs found in DB. Creating placeholder...");

                    string outputDir = Path.Combine(_projectBasePath, "outputs", scan.FileCode);
                    Console.WriteLine($"Output directory: {outputDir}");
                    Directory.CreateDirectory(outputDir);

                    string placeholderPath = Path.Combine(outputDir, $"{scan.FileCode}_ai.png");
                    Console.WriteLine($"Placeholder path: {placeholderPath}");

                    if (!File.Exists(placeholderPath))
                    {
                        try
                        {
                            Console.WriteLine("Generating new placeholder image...");
                            using (Bitmap bmp = new Bitmap(256, 256))
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.Clear(System.Drawing.Color.LightGreen);
                                g.DrawString("No AI result yet", new Font("Arial", 14), Brushes.Black, new System.Drawing.PointF(40, 110));
                                bmp.Save(placeholderPath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            Console.WriteLine("Placeholder image created successfully.");
                        }
                        catch (Exception pex)
                        {
                            Console.WriteLine("Failed to create placeholder image:");
                            Console.WriteLine(pex.ToString());
                        }
                    }

                    try
                    {
                        picAIResult.Image = System.Drawing.Image.FromFile(placeholderPath);
                        lblAIInfo.Text = "No AI result available — placeholder shown.";
                        btnGenerateAI.Visible = true;
                        Console.WriteLine("Placeholder displayed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to load placeholder image:");
                        Console.WriteLine(ex.ToString());
                    }
                }

                Console.WriteLine("lstScans_SelectedIndexChanged completed successfully.");
            }
            catch (Exception eMain)
            {
                Console.WriteLine("Unhandled exception in lstScans_SelectedIndexChanged:");
                Console.WriteLine(eMain.ToString());
                lblAIInfo.Text = $"Unexpected error: {eMain.Message}";
            }
        }
        
        private void btnAddPatient_Click(object sender, EventArgs e)
        {
            using (var dialog = new AddPatientDialog(_patientRepo))
            {
                dialog.ShowDialog();
            }

            LoadPatients();
        }
        
        private void btnDeletePatient_Click(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null)
            {
                MessageBox.Show("Select a patient to delete.");
                return;
            }

            int patientId = int.Parse(lstPatients.SelectedItem.ToString().Split(':')[0]);

            var confirm = MessageBox.Show("Are you sure you want to delete this patient and all their data?",
                                          "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(
                    "Server=localhost\\SQLEXPRESS;Database=MedicalImagingDB;Trusted_Connection=True;TrustServerCertificate=True;"
                );
                connection.Open();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand("DELETE FROM Patients WHERE PatientID = @id", connection);
                cmd.Parameters.AddWithValue("@id", patientId);
                cmd.ExecuteNonQuery();
            }

            LoadPatients();
        }
        
        private void btnUploadScan_Click(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null)
            {
                MessageBox.Show("Select a patient first.");
                return;
            }

            int patientId = int.Parse(lstPatients.SelectedItem.ToString().Split(':')[0]);

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "DICOM files (*.dcm)|*.dcm|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fileName = Path.GetFileName(ofd.FileName);
                    string newRelativePath = Path.Combine("scans", fileName);
                    Directory.CreateDirectory(Path.Combine(_projectBasePath, "scans"));
                    File.Copy(ofd.FileName, Path.Combine(_projectBasePath, newRelativePath), true);

                    var newScan = new CTScan
                    {
                        PatientID = patientId,
                        FileCode = Path.GetFileNameWithoutExtension(fileName),
                        RelativePath = newRelativePath,
                        ScanDate = DateTime.Now
                    };

                    _scanRepo.Add(newScan);
                    MessageBox.Show("Scan uploaded successfully.");
                    lstPatients_SelectedIndexChanged(null, EventArgs.Empty);
                }
            }
        }

        
        private void btnGenerateAI_Click(object sender, EventArgs e)
        {
            if (lstScans.SelectedItem == null) return;

            int scanId = int.Parse(lstScans.SelectedItem.ToString().Split(':')[0]);
            var scan = _scanRepo.GetById(scanId);

            // path to output folder
            string outputDir = Path.Combine(_projectBasePath, "outputs", scan.FileCode);
            Directory.CreateDirectory(outputDir);

            string placeholderPath = Path.Combine(outputDir, $"{scan.FileCode}_ai.png");
            
            if (!File.Exists(placeholderPath))
            {
                using (Bitmap bmp = new Bitmap(256, 256))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.LightBlue);
                    g.DrawString("AI result placeholder", new Font("Arial", 14), Brushes.Black, new System.Drawing.PointF(25, 110));
                    bmp.Save(placeholderPath, ImageFormat.Png);
                }
            }

            // TODO: integrate AI Model
            // Exemple:
            // var resultImage = MyAIModel.Process(scanFullPath);
            // resultImage.Save(outputPath, ImageFormat.Png);

            // add placeholder to db
            string relativeFromOutputs = Path.Combine("outputs", scan.FileCode, $"{scan.FileCode}_ai.png");

            var newOutput = new AIModelOutput
            {
                ScanID = scanId,
                FileCode = $"{scan.FileCode}_ai",
                RelativePath = relativeFromOutputs,
                GenerationDate = DateTime.Now
            };

            picAIResult.Image?.Dispose();
            picAIResult.Image = null;
            string fullPath = Path.Combine(_projectBasePath, relativeFromOutputs);
            _aiRepo.Add(newOutput, File.ReadAllBytes(fullPath));

            MessageBox.Show("Placeholder AI result created. (TODO: integrate real AI model here)");
            btnGenerateAI.Visible = false;
            lstScans_SelectedIndexChanged(null, EventArgs.Empty);
        }

    }
}
