using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNet.Hosting;
using System.IO;
using Microsoft.Extensions.OptionsModel;
using System.Net.Http;
using System.Text;
using CheckMarket.RespondentImporter.Models;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CheckMarket.RespondentImporter.Controllers
{
    public class HomeController : RootController
    {
        readonly IHostingEnvironment _app;

        public HomeController(IHostingEnvironment app, IOptions<CheckMarket.RespondentImporter.Models.CheckMarketApiKeys> ApiKeys, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : base(ApiKeys, cache)
        {
            _app = app;
        }

        public ViewResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Configure() {
            int surveyId;
            if (int.TryParse(Request.Form["surveyid"], out surveyId))
            {
                IFormFile importFile = Request.Form.Files.GetFile("uploadFile");
                var saveFile = new lib.ImportFile(_app.WebRootPath);
                var fileName = await saveFile.Save(importFile);

                if (!string.IsNullOrEmpty(fileName))
                {
                    var model = new Models.ConfigureModel()
                    {
                        SurveyId = surveyId,
                        FileName = fileName,
                        Columns = new List<string>()
                    };

                    model.Columns = saveFile.ReadColumnHeaders();

                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Import() {
            var ApiController = new ApiAdapterController(apiKeys, cache);
            var rateLimiter = new lib.RateLimiter(4);
            List<RowResult> ilResults = new List<RowResult>();
            int RowIndex = 1;
            try
            {
                int surveyId;
                if (int.TryParse(Request.Form["surveyId"], out surveyId))
                {
                    var importFile = Request.Form["fileName"];
                    using (var importReader = new lib.ImportFile(_app.WebRootPath).ReadFile(importFile))
                    {
                        //Skip first row
                        importReader.ReadLine();
                        while (!importReader.EndOfStream)
                        {
                            RowResult rowResult = new RowResult(RowIndex);
                            var dataRow = await importReader.ReadLineAsync();
                            if (string.IsNullOrEmpty(dataRow)) continue;
                            var dataTable = dataRow.Split(';');
                            if (dataTable.Length == 0) continue;
                            List<QuestionResponse> ilQuestionResponses = new List<QuestionResponse>();
                            foreach (var column in Request.Form.Keys)
                            {
                                if (column.StartsWith("column", StringComparison.OrdinalIgnoreCase) && Request.Form[column] != "0")
                                {
                                    short columnIndex = Convert.ToInt16(column.Substring(column.IndexOf("_", StringComparison.OrdinalIgnoreCase) + 1));
                                    short questionId;
                                    if (dataTable.Length > columnIndex)
                                    {
                                        var columnData = dataTable[columnIndex].Trim('"');

                                        if (short.TryParse(Request.Form[column], out questionId))
                                        {
                                            var blnFound = true;
                                            var QuestionResponse = ilQuestionResponses.FirstOrDefault(qr => qr.QuestionId == questionId);
                                            var apiQuestionResult = (await ApiController.Question(surveyId, questionId)).Content;
                                            dynamic apiQuestion = JsonConvert.DeserializeObject<dynamic>(apiQuestionResult);
                                            if (QuestionResponse == null)
                                            {
                                                blnFound = false;
                                                QuestionResponse = new QuestionResponse { QuestionId = questionId };
                                            }
                                            int responseId;
                                            string value = null;
                                            //Should we define the response in the rows or is this column specific for one response
                                            if (int.TryParse(Request.Form[$"responses_{columnIndex}"], out responseId) && responseId > 0)
                                            {
                                                if (!string.IsNullOrEmpty(columnData))
                                                {
                                                    if (apiQuestion.Data.QuestionTypeId == "8")
                                                    {
                                                        value = columnData;
                                                    }
                                                    QuestionResponse.Responses.Add(new Models.Response { ResponseId = responseId, Value = value });
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(columnData))
                                            {
                                                if (apiQuestion.Data.QuestionTypeId == "7" || apiQuestion.Data.QuestionTypeId == "9")
                                                {
                                                    QuestionResponse.Responses.Add(new Models.Response { ResponseId = 1, Value = columnData });
                                                }
                                                else if (int.TryParse(columnData, out responseId))
                                                {
                                                    QuestionResponse.Responses.Add(new Models.Response { ResponseId = responseId });
                                                }
                                            }
                                            if (!blnFound && QuestionResponse.Responses.Count > 0)
                                                ilQuestionResponses.Add(QuestionResponse);
                                        }
                                    }
                                }
                            }
                            if (ilQuestionResponses.Count > 0)
                            {
                                rateLimiter.WaitForRate();
                                var result = await ApiController.AddRespondent(surveyId, ilQuestionResponses);
                                rowResult.Imported = result.Imported;
                            }
                            ilResults.Add(rowResult);
                        }
                        importReader.Close();
                    }

                    ImportResult importResult = new ImportResult();
                    importResult.RowResults = ilResults;

                    return View(importResult);
                }
                return RedirectToAction("Index");
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
