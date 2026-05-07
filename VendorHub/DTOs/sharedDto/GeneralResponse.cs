namespace VendorHub.DTOs.sharedDto
{
    public class GeneralResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<ValidationError>? Errors { get; set; }

        public GeneralResponse Succeeded(string message = "Operation successful")
        {
            Success = true;
            Message = message;
            return this;
        }

        public GeneralResponse Failed(string message, IEnumerable<ValidationError>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
            return this;
        }

        public GeneralResponse ValidationFailed(string message, IEnumerable<ValidationError> errors)
        {
            Success = false;
            Message = message;
            Errors = errors;
            return this;
        }
    }

    public class GeneralResponse<T> : GeneralResponse
    {
        public T? Data { get; set; }

        public GeneralResponse<T> Succeeded(T data, string message = "Operation successful")
        {
            Success = true;
            Data = data;
            Message = message;
            return this;
        }

        public new GeneralResponse<T> Failed(string message, IEnumerable<ValidationError>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
            return this;
        }

        public new GeneralResponse<T> ValidationFailed(string message, IEnumerable<ValidationError> errors)
        {
            Success = false;
            Message = message;
            Errors = errors;
            return this;
        }
    }
}
