using FellowOakDicom;
using FellowOakDicom.Imaging;
using Microsoft.Data.SqlClient;
using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;
using FellowOakDicom.Imaging.Codec;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.ImageSharp;
using Microsoft.Extensions.DependencyInjection;
// dotnet add package fo-dicom --version 5.0.2
// dotnet add package fo-dicom.Imaging.ImageSharp --version 5.0.2
//dotnet add package SixLabors.ImageSharp --version 2.1.3
//dotnet add package fo-dicom.Codecs --version 5.0.2


namespace PPICancerRecognitionProject
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            

           

            ApplicationConfiguration.Initialize();
            

            // change path
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=MedicalImagingDB;Trusted_Connection=True;TrustServerCertificate=True;";

            var patientRepo = new PatientRepository(connectionString);
            var scanRepo = new CTScanRepository(connectionString);
            var aiRepo = new AIModelOutputRepository(connectionString, "C:\\AI_Outputs_Test");

            
            var patient = patientRepo.GetById(1);
            Console.WriteLine($"Pacient: {patient.FirstName} {patient.LastName}, Născut: {patient.DateOfBirth:d}");
            
            var scans = scanRepo.GetByPatientId(patient.PatientID);
            foreach (var scan in scans)
            {
                Console.WriteLine($"  Scan: {scan.FileCode}  ({scan.RelativePath})");
                
                var outputs = aiRepo.GetByScanId(scan.ScanID);
                foreach (var output in outputs)
                {
                    Console.WriteLine($"    AI Output: {output.FileCode}  ({output.RelativePath})");
                }
            }

            Console.WriteLine("Test DB completat cu succes!");
            
            new DicomSetupBuilder() .RegisterServices(s => s.AddFellowOakDicom().AddImageManager<ImageSharpImageManager>()) .Build();

            Console.WriteLine("ImageSharp rendering backend registered.");

            
            ApplicationConfiguration.Initialize();
            // Continue to your main form
            Application.Run(new Form1());
        }
    }
}