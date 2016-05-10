using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.OptionsModel;
using System.Net.Http;
using System.Text;
using CheckMarket.RespondentImporter.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CheckMarket.RespondentImporter.Controllers
{
    public class ApiAdapterController : RootController
    {
        public ApiAdapterController(IOptions<CheckMarket.RespondentImporter.Models.CheckMarketApiKeys> ApiKeys, IMemoryCache cache) : base (ApiKeys, cache)
        {}
        
        [HttpGet("api/surveys")]
        public async Task<ContentResult> Surveys() {
            using (var httpClient = new HttpClient())
            {
                var result = await httpClient.SendAsync(GetApiRequest("3/surveys?select=Id,Title,Langs&filter=SurveyStatusId%20eq%202"));
                if (result.IsSuccessStatusCode)
                {
                    var response = await result.Content.ReadAsStringAsync();
                    return Content(response);
                }
                return Content(string.Empty);
            }
        }

        [HttpGet("api/surveys/{surveyid}/questions")]
        public async Task<ContentResult> Questions(int surveyid) {
            using (var httpClient = new HttpClient()) {
                var result = await httpClient.SendAsync(GetApiRequest($"3/surveys/{surveyid}/questions/"));
                if (result.IsSuccessStatusCode)
                {
                    var response = await result.Content.ReadAsStringAsync();
                    return Content(response);
                }
                return Content(string.Empty);
            }
        }
        [HttpGet("api/surveys/{surveyid}/questions/{questionid}")]
        public async Task<ContentResult> Question(int surveyid, int questionid) {
            var cacheKey = $"{surveyid}_q-{questionid}";
            var cacheResult = cache.Get(cacheKey);
            if (cacheResult != null) {
                return Content(cacheResult.ToString());
            } else {
                using (var httpClient = new HttpClient()) {
                    var result = await httpClient.SendAsync(GetApiRequest($"3/surveys/{surveyid}/questions/{questionid}"));
                    if (result.IsSuccessStatusCode) {
                        var response = await result.Content.ReadAsStringAsync();
                        this.cache.Set(cacheKey, response);
                        return Content(response);
                    }
                }
            }
            return Content(string.Empty);
        }

        public async Task<RowResult> AddRespondent(int surveyid, List<QuestionResponse> ilQuestionResponses)
        {
            using (var httpClient = new HttpClient())
            {
                var apiRequest = GetApiRequest($"3/surveys/{surveyid}/respondents");
                apiRequest.Method = HttpMethod.Post;

                var sQuestionResponses = new StringBuilder();
                foreach (var questionResponse in ilQuestionResponses)
                {
                    var sResponses = new StringBuilder();
                    foreach (var response in questionResponse.Responses)
                    {
                        sResponses.Append($"{{\"ResponseId\": { response.ResponseId }");
                        if (!string.IsNullOrEmpty(response.Value))
                        {
                            sResponses.Append($",\"Value\": \"{ response.Value }\"");
                        }
                        sResponses.Append("},");
                    }
                    sQuestionResponses.Append($"{{\"QuestionId\":{questionResponse.QuestionId}, \"Responses\":[{sResponses.ToString().Trim(',')}]}},");
                }

                apiRequest.Content = new StringContent(string.Concat("{ \"RespondentStatusId\":1, \"LanguageCode\":\"en\", \"QuestionResponses\":[", sQuestionResponses.ToString().Trim(','), "]}"), Encoding.UTF8, "application/json");

                var apiResponse = await httpClient.SendAsync(apiRequest);
                string result = await apiResponse.Content.ReadAsStringAsync();
                dynamic apiResult = JsonConvert.DeserializeObject<dynamic>(result);

                RowResult rowResult = new RowResult(0);
                rowResult.Imported = apiResult.Data.Succeeded;

                return rowResult;
            }
        }

        private new ContentResult Content(string response) {
            return Content(response, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        }
        internal HttpRequestMessage GetApiRequest(string query)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{apiKeys.Value.RootUrl}{query}"),
                Method = HttpMethod.Get
            };
            request.Headers.Add("X-Master-Key", apiKeys.Value.MasterKey);
            request.Headers.Add("X-Key", apiKeys.Value.Key);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }
    }
}
