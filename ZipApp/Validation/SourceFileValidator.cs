using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public class SourceFileValidator : ArgumentsValidator
    {
        public SourceFileValidator(ArgumentsValidator childValidator = null) : base(childValidator)
        {
        }

        public override ValidationResult Validate(string[] args)
        {
            if (string.IsNullOrWhiteSpace(args[1]))
            {
                return new ValidationResult(false, "Source file path is empty.");
            }

            var fileInfo = new FileInfo(args[1]);

            if(fileInfo.Exists == false)
            {
                return new ValidationResult(false, "Source file does not exists.");
            }

            if(_childValidator == null)
            {
                return new ValidationResult(true, "");
            }

            return _childValidator.Validate(args);
        }
    }
}
