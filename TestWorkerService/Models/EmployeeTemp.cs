using OfficeOpenXml.FormulaParsing.Excel.Operators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestWorkerService.Models
{
    public partial class EmployeeTemp
    {
        [Key]
        public int DOCIMO { get; set; }
        public string? 船主名 { get; set; }
        public string? DOCName { get; set; }
        public string? OriginalOperatorName { get; set; }
        public string? BeneficialOwnerName { get; set; }
        public string? RegisteredOwnerName { get; set; }
        public string? OriginalOwner { get; set; }
        public string? MFDJA { get; set; }
    }

    public partial class StoredProc_Result {
    public int RowAffected { get; set; }
      public int NewEmployees { get; set; }
    }

    }
