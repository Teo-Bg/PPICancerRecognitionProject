using PPICancerRecognitionProject.domain;
using PPICancerRecognitionProject.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PPICancerRecognitionProject.service
{
    public class AIModelService
    {
        private readonly AIModelOutputRepository _aiRepo;
        private readonly string _apiUrl;  // Ex: "http://localhost:8000/predict/"
        private readonly string _apiUrl_full;  // Ex: "http://localhost:8000/predict-full/"
        private readonly string _modelOutputsBasePath;

        public AIModelService(AIModelOutputRepository aiRepo, string apiUrl, string apiUrl_full, string modelOutputsBasePath)
        {
            _aiRepo = aiRepo;
            _apiUrl = apiUrl;
            _apiUrl_full = apiUrl_full;
            _modelOutputsBasePath = modelOutputsBasePath;
        }

        public async Task<AIModelOutput> ProcessDicomAsync(int scanId, string scanFileCode, string dicomFilePath)
        {
            if (!File.Exists(dicomFilePath))
                throw new FileNotFoundException("DICOM file not found.", dicomFilePath);

            using var httpClient = new HttpClient();

            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(dicomFilePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/dicom");
            form.Add(streamContent, "file", Path.GetFileName(dicomFilePath));

            var response = await httpClient.PostAsync(_apiUrl, form);

            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI API returned error: {response.StatusCode} - {err}");
            }

            // Assume API returns image bytes (PNG mask)
            var maskBytes = await response.Content.ReadAsByteArrayAsync();

            // Get scanFileCode from DB (to save in proper folder)
            //string scanFileCode = GetScanFileCode(scanId);
            //if (string.IsNullOrEmpty(scanFileCode))
            //    throw new Exception($"No CT scan found with ID {scanId}.");

            // Create output folder
            string scanFolder = Path.Combine(_modelOutputsBasePath, scanFileCode);
            if (!Directory.Exists(scanFolder))
                Directory.CreateDirectory(scanFolder);

            string outputFileName = $"{scanFileCode}_ai.png";
            string outputFullPath = Path.Combine(scanFolder, outputFileName);

            File.WriteAllBytes(outputFullPath, maskBytes);

            var output = new AIModelOutput
            {
                ScanID = scanId,
                FileCode = $"{scanFileCode}_ai",
                RelativePath = Path.Combine("outputs", scanFileCode, outputFileName),
                GenerationDate = DateTime.Now
            };

            // Save DB record
            _aiRepo.Add(output, maskBytes);

            return output;
        }

        public async Task<AIModelOutput> ProcessDicomFullAsync(int scanId, string scanFileCode, string dicomFilePath)
        {
            if (!File.Exists(dicomFilePath))
                throw new FileNotFoundException("DICOM file not found.", dicomFilePath);

            using var httpClient = new HttpClient();

            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(dicomFilePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/dicom");
            form.Add(streamContent, "file", Path.GetFileName(dicomFilePath));

            var response = await httpClient.PostAsync(_apiUrl_full, form);

            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI API returned error: {response.StatusCode} - {err}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            // Extragem masca binară ca array de array de int (0/1)
            var maskBinaryElement = root.GetProperty("mask_binary");

            // Convertim array JSON în byte[] (flatten și scriem 0/1 ca bytes)
            // Variante:
            // 1) Salvăm JSON-ul raw (ca text)
            // 2) Convertim la un format binar (flatten)
            // Pentru simplitate, salvăm ca JSON text

            string maskJsonString = maskBinaryElement.GetRawText();

            // Creare folder output
            string scanFolder = Path.Combine(_modelOutputsBasePath, scanFileCode);
            if (!Directory.Exists(scanFolder))
                Directory.CreateDirectory(scanFolder);

            // Salvăm masca ca .json în loc de .png
            string outputFileName = $"{scanFileCode}_ai_full_mask.json";
            string outputFullPath = Path.Combine(scanFolder, outputFileName);

            await File.WriteAllTextAsync(outputFullPath, maskJsonString);

            // Extragem clasificarea
            var classification = root.GetProperty("classification");
            string predictedTypes = string.Join(",", classification.GetProperty("predicted_types").EnumerateArray().Select(x => x.GetString()));
            string predictedClass = classification.GetProperty("predicted_class").GetString();

            string typeProbs = classification.GetProperty("type_probs").GetRawText();   // JSON string
            string classProbs = classification.GetProperty("class_probs").GetRawText(); // JSON string

            var output = new AIModelOutput
            {
                ScanID = scanId,
                FileCode = $"{scanFileCode}_ai_full_mask",
                RelativePath = Path.Combine("outputs", scanFileCode, outputFileName),
                GenerationDate = DateTime.Now,

                PredictedTypes = predictedTypes,
                PredictedClass = predictedClass,
                TypeProbabilities = typeProbs,
                ClassProbabilities = classProbs
            };

            // Salvăm în repo conținutul fișierului ca bytes (text -> bytes)
            var maskBytes = System.Text.Encoding.UTF8.GetBytes(maskJsonString);
            _aiRepo.Add(output, maskBytes);

            return output;
        }



    }
}
