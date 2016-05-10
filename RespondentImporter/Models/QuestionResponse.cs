using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.Models
{
    public class QuestionResponse
    {
        public QuestionResponse()
        {
            Responses = new List<Response>();
        }
        public int QuestionId { get; set; }
        public IList<Response> Responses { get; set; }
    }

    public class Response {
        public int ResponseId { get; set; }
        public string Value { get; set; }
    }
}
