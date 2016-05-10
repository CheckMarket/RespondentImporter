using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.Models
{
    public class ConfigureModel
    {
        public int SurveyId { get; set; }
        public string FileName {get; set;}

        public IList<string> Columns { get; set; }
    }
}
