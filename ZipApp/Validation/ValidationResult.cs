using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public class ValidationResult
    {
        public ValidationResult(bool succeeded, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }
        public string ErrorMessage { get; set; }
    }
}
