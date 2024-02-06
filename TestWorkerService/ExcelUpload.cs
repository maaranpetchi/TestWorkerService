using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWorkerService.Data;
using TestWorkerService.Models;

namespace TestWorkerService
{
    internal class ExcelUpload:IDisposable
    {
        private bool disposedValue;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        public ExcelUpload(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        public async Task<int> ExcelImport()
        {
            try {
                using (var _db = new DevTestingContext())
                {

                    var excelDataList = new List<EmployeeTemp>();

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    var filepath = _configuration.GetSection("Report").GetValue<string>("FileLocation");
                    using (var package = new ExcelPackage(new FileInfo(filepath)))
                    {
                        //   data = package.Workbook.Worksheets.Count;
                        if (package.Workbook.Worksheets.Count > 0)
                        {
                            //var worksheet = package.Workbook.Worksheets[worksheetIndex];
                            // Use the worksheet here
                        }
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            DateTime date;
                            var employee = new EmployeeTemp
                            {
                                船主名 = worksheet.Cells[row, 1].Value?.ToString(),

                                DOCIMO = Convert.ToInt32(worksheet.Cells[row, 2].Value?.ToString()),
                                DOCName = worksheet.Cells[row, 3].Value?.ToString(),
                                OriginalOperatorName = worksheet.Cells[row, 4].Value?.ToString(),
                                BeneficialOwnerName = worksheet.Cells[row, 5].Value?.ToString(),
                                RegisteredOwnerName = worksheet.Cells[row, 6].Value?.ToString(),
                                OriginalOwner = worksheet.Cells[row,7].Value?.ToString(),
                                MFDJA = worksheet.Cells[row,8].Value?.ToString(),
                              //  DateTime.TryParseExact(worksheet.Cells[row, 6].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                             //   Date = DateTime.TryParseExact(worksheet.Cells[row, 6].Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)?date:DateTime.MinValue,
                            };
                            excelDataList.Add(employee);
                        }
                    }

                    await _db.EmployeeTemps.AddRangeAsync(excelDataList);
                    _logger.LogWarning("{0} rows read from the excel", excelDataList.Count());
                    return await _db.SaveChangesAsync();
                }
                
            }
            catch(Exception ex)
            {
                _logger.LogWarning("An error occured while sending the mail : {0}", ex.Message);
                // throw ex;
                 return 0;
            }

      
            //  return 0;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ExcelUpload()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
