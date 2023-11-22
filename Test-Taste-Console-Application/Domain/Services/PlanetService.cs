using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            var allPlanetsWithTheirMoons = new Collection<Planet>();

            try
            {
                Console.WriteLine("Loading data..."); // Inform users about loading data

                var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

                //If the status code isn't 200-299, then the function returns an empty collection.
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: Failed to fetch planet data. Status code: {response.StatusCode}");
                    Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                    return allPlanetsWithTheirMoons;
                }

                var content = response.Content.ReadAsStringAsync().Result;

                //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
                var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

                // The JSON converter can return a null object.
                if (results == null)
                {
                    Console.WriteLine("Error: Failed to deserialize planet data.");
                    return allPlanetsWithTheirMoons;
                }

                //If the planet doesn't have any moons, then it isn't added to the collection.
                foreach (var planet in results.Bodies)
                {
                    if (planet.Moons != null)
                    {
                        var newMoonsCollection = new Collection<MoonDto>();
                        foreach (var moon in planet.Moons)
                        {
                            var moonResponse = _httpClientService.Client
                                .GetAsync(UriPath.GetMoonByIdQueryParameters + moon.URLId)
                                .Result;

                            // Check if the request for moon data was successful
                            if (!moonResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Error: Failed to fetch moon data for {planet.Id}. Status code: {moonResponse.StatusCode}");
                                continue; // Continue to the next moon
                            }

                            var moonContent = moonResponse.Content.ReadAsStringAsync().Result;
                            newMoonsCollection.Add(JsonConvert.DeserializeObject<MoonDto>(moonContent));
                        }
                        planet.Moons = newMoonsCollection;

                    }
                    allPlanetsWithTheirMoons.Add(new Planet(planet));
                }

                Console.WriteLine("Data loaded successfully."); // Inform users about successful data loading
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            return allPlanetsWithTheirMoons;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}
