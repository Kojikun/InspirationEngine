using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InspirationEngine.WPF.ValidationRules
{
    /// <summary>
    /// Markup Extension for PathRule
    /// </summary>
    public class PathValidationRule : BaseValidationRule<PathRule> { }

    /// <summary>
    /// ValidationRule that checks if a path is valid
    /// </summary>
    public class PathRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            /// the reason we don't use <see cref="Utilities.Utilities.TryGetFullPath(string, out string)"/> here is because we want the exception message
            try
            {
                Path.GetFullPath(value as string);
                return ValidationResult.ValidResult;
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }
}
