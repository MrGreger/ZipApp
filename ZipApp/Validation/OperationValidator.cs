using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public class OperationValidator : ArgumentsValidator
    {
        public OperationValidator(ArgumentsValidator childValidator = null) : base(childValidator)
        {
        }

        public override ValidationResult Validate(string[] args)
        {
            if (string.IsNullOrWhiteSpace(args[0]))
            {
                return new ValidationResult(false, "Operation argument is empty.");
            }

            if (!args[0].Equals("compress",StringComparison.CurrentCultureIgnoreCase) && !args[0].Equals("decompress", StringComparison.CurrentCultureIgnoreCase))
            {
                return new ValidationResult(false, "Invalid operation. You can only compress or decompress file.");
            }

            if (_childValidator == null)
            {
                return new ValidationResult(true, "");
            }

            return _childValidator.Validate(args);
        }
    }
}
