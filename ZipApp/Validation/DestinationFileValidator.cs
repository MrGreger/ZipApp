using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public class DestinationFileValidator : ArgumentsValidator
    {
        public DestinationFileValidator(ArgumentsValidator childValidator = null) : base(childValidator)
        {
        }

        public override ValidationResult Validate(string[] args)
        {
            if (string.IsNullOrWhiteSpace(args[2]))
            {
                return new ValidationResult(false, "Destination file path is empty.");
            }

            var fileInfo = new FileInfo(args[2]);

            if (fileInfo.Exists == true)
            {
                return new ValidationResult(false, "Destination file alredy exists.");
            }

            if (_childValidator == null)
            {
                return new ValidationResult(true, "");
            }

            return _childValidator.Validate(args);
        }
    }
}
