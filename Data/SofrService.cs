using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;


namespace TG_Fitz.Data
{
    public class SofrService
    {
        private readonly HttpClient _httpClient;
        public SofrService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<string> GetLatestSofrAsync()
        {
            string FreadApiJson = await File.ReadAllTextAsync("fredApi.json");
            using var doc = JsonDocument.Parse(FreadApiJson);
            string apiKey = doc.RootElement.GetProperty("FRED_API_KEY").GetString()!;

            var url = $"https://api.stlouisfed.org/fred/series/observations?series_id=SOFR&api_key={apiKey}&file_type=json&sort_order=desc&limit=1";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(content);
            using var jsonDoc = JsonDocument.Parse(content);

            var observation = jsonDoc.RootElement.GetProperty("observations")[0];

            string date = observation.GetProperty("date").GetString()!;
            string value = observation.GetProperty("value").GetString()!;

            return $"Sofr on {date}: {value}%";
        }
    }
}
