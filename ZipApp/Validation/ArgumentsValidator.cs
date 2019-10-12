using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipApp.Validation
{
    public abstract class ArgumentsValidator
    {
        protected ArgumentsValidator _childValidator;

        protected ArgumentsValidator(ArgumentsValidator childValidator = null)
        {
            _childValidator = childValidator;
        }

        public void SetChildValidator(ArgumentsValidator childValidator)
        {
            _childValidator = childValidator;
        }

        public abstract ValidationResult Validate(string[] args);
    }
}
