using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public class ArgumentsCountValidator : ArgumentsValidator
    {
        public ArgumentsCountValidator(ArgumentsValidator childValidator = null) : base(childValidator)
        {
        }

        public override ValidationResult Validate(string[] args)
        {
            if (args.Length < 3 || args.Length > 3)
            {
                return new ValidationResult(false, "Invalid arguments count.");
            }          

            if (_childValidator == null)
            {
                return new ValidationResult(true, "");
            }

            return _childValidator.Validate(args);
        }
    }
}
