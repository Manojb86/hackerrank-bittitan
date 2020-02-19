using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = GetRecordsByAgeGroup(25, 30, 63);

            foreach (var item in result)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }

        public static List<int> GetRecordsByAgeGroup(int ageStart, int ageEnd, int bpDiff)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri("https://jsonmock.hackerrank.com/")
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            List<int> response = new List<int>();
            if (ageStart > 0 && ageEnd > 0 && (bpDiff > 0 && bpDiff <= 100))
            {
                for (int i = 1; i <= 10; i++)
                {
                    string url = string.Concat("api/medical_records?&page=", i);
                    HttpResponseMessage httpResponseMessage = client.GetAsync(url).GetAwaiter().GetResult();
                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var medicalRecordsStr = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var patientData = JsonConvert.DeserializeObject<MedicalRecords>(medicalRecordsStr);

                        if (patientData != null && patientData.Data.Any())
                        {
                            ManageMedicalData(ageStart, ageEnd, bpDiff, response, patientData);
                        }
                    }
                }
            }

            return response;
        }

        private static void ManageMedicalData(
            int ageStart, 
            int ageEnd, 
            int bpDiff, 
            List<int> response, 
            MedicalRecords patientData)
        {
            if (ageStart > 0 && ageEnd > 0 && patientData != null && patientData.Data.Any())
            {
                foreach (var item in patientData.Data)
                {
                    if (item != null
                        && item.Vitals != null
                        && !string.IsNullOrEmpty(item.Timestamp)
                        && !string.IsNullOrEmpty(item.UserDob))
                    {
                        double age = GetAge(item);

                        int bpDiffCalculated = item.Vitals.BloodPressureDiastole - item.Vitals.BloodPressureSystole;

                        if (bpDiffCalculated > bpDiff && age > ageStart && age < ageEnd)
                        {
                            response.Add(item.Id);
                        }

                    }
                } 
            }
        }

        private static double GetAge(PatientData item)
        {
            double age = 0;

            if (item != null && !string.IsNullOrEmpty(item.Timestamp) && !string.IsNullOrEmpty(item.UserDob))
            {
                double timestamp = Convert.ToDouble(item.Timestamp);
                DateTime createdDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(timestamp / 1000d)).ToLocalTime();
                DateTime userDob = DateTime.ParseExact(item.UserDob, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                age = (createdDate - userDob).TotalDays / 365; 
            }

            return age;
        }
    }
    
    class MedicalRecords
    {
        public List<PatientData> Data { get; set; }
    }

    class PatientData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Timestamp { get; set; }
        public string UserName { get; set; }
        public string UserDob { get; set; }
        public Vitals Vitals { get; set; }
    }

    class Vitals
    {
        public int BloodPressureDiastole { get; set; }
        public int BloodPressureSystole { get; set; }
    }

}
