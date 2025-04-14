using System.ComponentModel.DataAnnotations;

namespace SIMS.Validations
{
    public class AllowTypeFile : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowTypeFile(string[] extensions)
        {
            _extensions = extensions;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult(GettErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        private string GettErrorMessage()
        {
            return $"This file extension is not allowed";
        }
    }
}
