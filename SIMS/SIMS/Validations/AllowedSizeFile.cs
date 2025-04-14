using System.ComponentModel.DataAnnotations;

namespace SIMS.Validations
{
    public class AllowedSizeFile : ValidationAttribute
    {
        private readonly int _maxSizeFile;
        public AllowedSizeFile(int sizeFile)
        {
            _maxSizeFile = sizeFile;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var file = value as IFormFile; 
            if (file != null)
            {
                if (file.Length > _maxSizeFile)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }
        private string GetErrorMessage()
        {
            return $"Maximum a allowes file size is {_maxSizeFile}";
        }
    }
}
