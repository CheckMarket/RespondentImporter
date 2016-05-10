using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMarket.RespondentImporter.Models
{
    public class ImportResult
    {
        public List<RowResult> RowResults { get; set; }

        public int RowsInFile {
            get {
                return RowResults.Count;
            }
        }
        public int RowsImported {
            get {
                return RowResults.Count(r => r.Imported = true);
            }
        }
        public int RowsFailed {
            get {
                return RowResults.Count(r => r.Imported = false);
            }
        }
    }

    public class RowResult {
        public RowResult(int RowNumber)
        {
            this.RowNumber = RowNumber;

        }
        public int RowNumber { get; set; }
        public bool Imported { get; set; }
    }
}
