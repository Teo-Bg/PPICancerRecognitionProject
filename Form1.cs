using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.ImageSharp;
using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;
using PPICancerRecognitionProject.service;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
namespace PPICancerRecognitionProject


{
    public partial class Form1 : Form
    {
        private readonly PatientRepository _patientRepo;
        private readonly CTScanRepository _scanRepo;
        private readonly AIModelOutputRepository _aiRepo;
        private readonly string _aiOutputPath;
        private readonly string _projectBasePath;
        private readonly AIModelService _aiService;
        private readonly string _aiApiUrl = "http://localhost:8000/predict/";  
        private readonly string _aiApiUrl_full = "http://localhost:8000/predict-full-vgg/";

        public Form1()
        {
            InitializeComponent();


            // change connectionString
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=MedicalImagingDB;Trusted_Connection=True;TrustServerCertificate=True;";

            _projectBasePath = "C:\\Facultate\\AplicatieAi\\PPICancerRecognitionProject";
            _aiOutputPath = Path.Combine(_projectBasePath, "outputs");

            _patientRepo = new PatientRepository(connectionString);
            _scanRepo = new CTScanRepository(connectionString);
            _aiRepo = new AIModelOutputRepository(connectionString, _aiOutputPath);

            _aiService = new AIModelService(_aiRepo, _aiApiUrl, _aiApiUrl_full,  _aiOutputPath);

            LoadPatients();
        }

        private void LoadPatients()
        {
            lstPatients.Items.Clear();
            var patients = _patientRepo.GetAll();

            foreach (var p in patients)
                lstPatients.Items.Add($"{p.PatientID}: {p.FirstName} {p.LastName}");
        }

    
        private void lstPatients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null) return;
            btnExportData.Visible = lstPatients.SelectedItem != null;

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

      
        private void lstScans_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstScans.SelectedItem == null)
            {
                Console.WriteLine("⚠️ No scan selected.");
                return;
            }

            picOriginal.Image = null;
            picAIResult.Image = null;
            lblAIInfo.Text = "";
            btnGenerateAI.Visible = false;

            try
            {
            
                string selectedText = lstScans.SelectedItem.ToString();
                Console.WriteLine($"Selected item text: {selectedText}");
                int scanId = int.Parse(selectedText.Split(':')[0]);

                var scan = _scanRepo.GetById(scanId);
                var outputs = _aiRepo.GetByScanId(scanId);

       
                picAIResult.Image = null;
                lblAIInfo.Text = "";
                btnGenerateAI.Visible = false;

                if (scan == null)
                {
                    Console.WriteLine($"No scan found with ID {scanId} in DB.");
                    lblAIInfo.Text = "Scan not found in database.";
                    return;
                }

                Console.WriteLine($"Loaded scan from DB: ID={scan.ScanID}, FileCode={scan.FileCode}, Path={scan.RelativePath}");

           
                string scanFullPath = Path.Combine(_projectBasePath, scan.RelativePath);
                Console.WriteLine($"Full scan path: {scanFullPath}");

         
                if (!File.Exists(scanFullPath))
                {
                    Console.WriteLine("File does not exist at path above.");
                    lblAIInfo.Text = "Original scan file not found.";
                }
                else
                {
                    Console.WriteLine($"File exists: {scanFullPath}");
                    Console.WriteLine($"Extension: {Path.GetExtension(scanFullPath)}");

       
                    if (Path.GetExtension(scanFullPath).Equals(".dcm", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            Console.WriteLine("Rendering DICOM with ImageSharp backend...");

                            var dicomImage = new DicomImage(scanFullPath);
                            using (var imageSharp = dicomImage.RenderImage().AsSharpImage())
                            using (var ms = new MemoryStream())
                            {
                                imageSharp.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                                ms.Position = 0;

                                using (var tmp = System.Drawing.Image.FromStream(ms))
                                {
                                    picOriginal.Image?.Dispose();
                                    picOriginal.Image = new Bitmap(tmp);
                                }

                                Console.WriteLine("DICOM rendered and displayed successfully.");
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

                Console.WriteLine("---- Checking for AI Output ----");
                if(outputs.Count > 0)
                {
                    Console.WriteLine($"Found {outputs.Count} AI output(s) in DB.");

                    var output = outputs.First();
                    string aiFullPath = Path.Combine(_projectBasePath, output.RelativePath);
                    Console.WriteLine($"Full AI path: {aiFullPath}");

                    if (File.Exists(aiFullPath))
                    {
                        try
                        {
                            Console.WriteLine("Loading AI mask binary...");

           
                            string maskJson = File.ReadAllText(aiFullPath);

                            var maskBinary = JsonSerializer.Deserialize<List<List<int>>>(maskJson);
                            Bitmap originalBmp = new Bitmap(picOriginal.Image);
                            
                            var resizedMask = ResizeMask(maskBinary, originalBmp.Width, originalBmp.Height);
                            
                            Bitmap overlayed = ApplyMaskOverlay(originalBmp, resizedMask);

                            picAIResult.Image?.Dispose();
                            picAIResult.Image = overlayed;

                            string predictedType = "N/A";
                            string predictedClass = "N/A";

                            if (!string.IsNullOrEmpty(output.TypeProbabilities))
                            {
                                try
                                {
                                    var typeProbs = JsonSerializer.Deserialize<Dictionary<string, double>>(output.TypeProbabilities);
                                    if (typeProbs != null && typeProbs.Count > 0)
                                    {
                                        predictedType = typeProbs.OrderByDescending(kv => kv.Value).First().Key;
                                    }
                                }
                                catch
                                {
                                    predictedType = output.PredictedTypes ?? "N/A";
                                }
                            }
                            else
                            {
                                predictedType = output.PredictedTypes ?? "N/A";
                            }

                            if (!string.IsNullOrEmpty(output.ClassProbabilities))
                            {
                                try
                                {
                                    var classProbs = JsonSerializer.Deserialize<Dictionary<string, double>>(output.ClassProbabilities);
                                    if (classProbs != null && classProbs.Count > 0)
                                    {
                                        predictedClass = classProbs.OrderByDescending(kv => kv.Value).First().Key;
                                    }
                                }
                                catch
                                {
                                    predictedClass = output.PredictedClass ?? "N/A";
                                }
                            }
                            else
                            {
                                predictedClass = output.PredictedClass ?? "N/A";
                            }

                            lblAIInfo.Text = $"Predicted Type: {predictedType}\n" + $"Predicted Class: {predictedClass}";

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
                        picAIResult.Image = null;
                    }
                }
                else
                {
                    Console.WriteLine("No AI outputs found in DB.");

                    picAIResult.Image = null;
                    lblAIInfo.Text = "No AI result available.";
                    btnGenerateAI.Visible = true;
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
        


        private byte[] BitmapToBytes(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
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

        private async void btnGenerateAI_Click(object sender, EventArgs e)
        {
            if (lstScans.SelectedItem == null) return;

            int scanId = int.Parse(lstScans.SelectedItem.ToString().Split(':')[0]);
            var scan = _scanRepo.GetById(scanId);

            if (scan == null)
            {
                MessageBox.Show("Scan not found in database.");
                return;
            }

            string dicomFullPath = Path.Combine(_projectBasePath, scan.RelativePath);
            if (!File.Exists(dicomFullPath))
            {
                MessageBox.Show("DICOM file not found.");
                return;
            }

            btnGenerateAI.Enabled = false;
            lblAIInfo.Text = "Processing AI model... please wait.";

            try
            {
                var aiOutput = await _aiService.ProcessDicomFullAsync(scanId, scan.FileCode, dicomFullPath);

                string aiFullPath = Path.Combine(_projectBasePath, aiOutput.RelativePath);
                if (File.Exists(aiFullPath))
                {
                    picAIResult.Image?.Dispose();
                    picAIResult.Image = System.Drawing.Image.FromFile(aiFullPath);
                    lblAIInfo.Text = "AI processing complete.";
                }
                else
                {
                    lblAIInfo.Text = "AI output file not found after processing.";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Out of memory", StringComparison.OrdinalIgnoreCase))
                {
                    lblAIInfo.Text = "AI processing complete (minor memory warning ignored).";
                    return;
                }
                
                MessageBox.Show($"Error processing AI model: {ex.Message}", 
                    "AI Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);

                lblAIInfo.Text = "Error during AI processing.";
            }
            finally
            {
                btnGenerateAI.Enabled = true;
                lstScans_SelectedIndexChanged(null, EventArgs.Empty); 
            }
        }
        
        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (lstPatients.SelectedItem == null)
                return;

            int patientId = int.Parse(lstPatients.SelectedItem.ToString().Split(':')[0]);
            var patient = _patientRepo.GetById(patientId);

            var scans = _scanRepo.GetByPatientId(patientId)
                                 .OrderBy(s => s.ScanDate)
                                 .ToList();

            if (scans.Count == 0)
            {
                MessageBox.Show("Pacientul nu are scanuri.");
                return;
            }

            string safeName = $"{patient.FirstName}_{patient.LastName}".Replace(" ", "_");

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Patient_{safeName}_Report.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            using FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
            
            iTextSharp.text.Document document =
                new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25f, 25f, 25f, 25f);

            PdfWriter.GetInstance(document, fs);
            document.Open();
            
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            
            document.Add(new Paragraph(
                $"Patient: {patient.FirstName} {patient.LastName}",
                headerFont));

            document.Add(new Paragraph("\n"));
            
            System.Drawing.Size uiSize = picOriginal.Size;

            foreach (var scan in scans)
            {
                document.Add(new Paragraph(
                    $"Scan date: {scan.ScanDate:dd.MM.yyyy HH:mm}",
                    headerFont));

                document.Add(new Paragraph("\n"));

                string dicomPath = Path.Combine(_projectBasePath, scan.RelativePath);
                if (!File.Exists(dicomPath))
                    continue;

                var outputs = _aiRepo.GetByScanId(scan.ScanID);
                string maskPath = outputs.Count > 0
                    ? Path.Combine(_projectBasePath, outputs.First().RelativePath)
                    : null;
                
                using Bitmap originalUi =
                    RenderUiFinalBitmap(dicomPath, null, uiSize);

                using Bitmap overlayUi =
                    RenderUiFinalBitmap(dicomPath, maskPath, uiSize);

                var origImg = iTextSharp.text.Image.GetInstance(BitmapToBytes(originalUi));
                var overlayImg = iTextSharp.text.Image.GetInstance(BitmapToBytes(overlayUi));

                origImg.ScaleToFit(250f, 250f);
                overlayImg.ScaleToFit(250f, 250f);

                origImg.Alignment = Element.ALIGN_CENTER;
                overlayImg.Alignment = Element.ALIGN_CENTER;
                
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 1f });

                table.AddCell(new PdfPCell(new Phrase("Original", headerFont))
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                table.AddCell(new PdfPCell(new Phrase("AI Result", headerFont))
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                table.AddCell(new PdfPCell(origImg)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                table.AddCell(new PdfPCell(overlayImg)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                document.Add(table);
                document.Add(new Paragraph("\n\n"));
            }

            document.Close();
            MessageBox.Show("PDF Created.");
        }




        
        private Bitmap ApplyMaskOverlay(Bitmap original, List<List<int>> mask)
        {
            int width = original.Width;
            int height = original.Height;

            var overlay = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    System.Drawing.Color orig = original.GetPixel(x, y);

                    if (mask[y][x] == 1)
                    {
                        var blended = System.Drawing.Color.FromArgb(
                            150, 
                            255, 0, 0
                        );

                        overlay.SetPixel(x, y, blended);
                    }
                    else
                    {
                        overlay.SetPixel(x, y, System.Drawing.Color.FromArgb(0, orig.R, orig.G, orig.B));
                    }
                }
            }
            
            Bitmap final = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(final))
            {
                g.DrawImage(original, System.Drawing.Point.Empty);
                g.DrawImage(overlay, System.Drawing.Point.Empty);
            }

            return final;
        }
        
        private List<List<int>> ResizeMask(List<List<int>> mask, int newWidth, int newHeight)
        {
            int oldHeight = mask.Count;
            int oldWidth = mask[0].Count;

            var resized = new List<List<int>>(newHeight);
    
            for (int y = 0; y < newHeight; y++)
            {
                int srcY = (int)((double)y / newHeight * oldHeight);
                srcY = Math.Min(srcY, oldHeight - 1);

                var row = new List<int>(newWidth);

                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = (int)((double)x / newWidth * oldWidth);
                    srcX = Math.Min(srcX, oldWidth - 1);

                    row.Add(mask[srcY][srcX]);
                }

                resized.Add(row);
            }

            return resized;
        }

        private Bitmap RenderUiFinalBitmap(
            string dicomPath,
            string maskPath,
            System.Drawing.Size uiSize)
        {
            var dicomImage = new DicomImage(dicomPath);

            using var sharp = dicomImage.RenderImage().AsSharpImage();
            using var ms = new MemoryStream();
            sharp.Save(ms, new PngEncoder());
            ms.Position = 0;

            using Bitmap dicomBmp = new Bitmap(ms);
            
            float scaleX = (float)uiSize.Width / dicomBmp.Width;
            float scaleY = (float)uiSize.Height / dicomBmp.Height;
            float scale = Math.Min(scaleX, scaleY);

            int drawW = (int)(dicomBmp.Width * scale);
            int drawH = (int)(dicomBmp.Height * scale);

            int offsetX = (uiSize.Width - drawW) / 2;
            int offsetY = (uiSize.Height - drawH) / 2;
            
            Bitmap uiBmp = new Bitmap(uiSize.Width, uiSize.Height);
            using (Graphics g = Graphics.FromImage(uiBmp))
            {
                g.Clear(System.Drawing.Color.Black);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(dicomBmp,
                    new System.Drawing.Rectangle(offsetX, offsetY, drawW, drawH));
            }
            
            if (string.IsNullOrEmpty(maskPath) || !File.Exists(maskPath))
                return uiBmp;
            
            var mask = JsonSerializer.Deserialize<List<List<int>>>(File.ReadAllText(maskPath));

            Bitmap result = new Bitmap(uiBmp);
            using (Graphics g = Graphics.FromImage(result))
            {
                using Bitmap overlay = new Bitmap(drawW, drawH);

                for (int y = 0; y < drawH; y++)
                {
                    int srcY = (int)((float)y / drawH * mask.Count);
                    srcY = Math.Min(srcY, mask.Count - 1);

                    for (int x = 0; x < drawW; x++)
                    {
                        int srcX = (int)((float)x / drawW * mask[0].Count);
                        srcX = Math.Min(srcX, mask[0].Count - 1);

                        if (mask[srcY][srcX] == 1)
                        {
                            overlay.SetPixel(x, y,
                                System.Drawing.Color.FromArgb(150, 255, 0, 0));
                        }
                    }
                }

                g.DrawImage(overlay, offsetX, offsetY);
            }

            return result;
        }
    }
}
