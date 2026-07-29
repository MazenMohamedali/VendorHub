using System.Text.Json.Serialization;
using VendorHub.Models;

namespace VendorHub.DTOs.sharedDto
{
    public class GeneralResponse
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IEnumerable<ValidationError>? Errors { get; init; }

        [JsonIgnore]
        public ResultStatus Status { get; init; }

        [JsonConstructor]
        public GeneralResponse(bool success, string? message, ResultStatus status = ResultStatus.Success, IEnumerable<ValidationError>? errors = null)
        {
            Success = success;
            Message = message;
            Status = status;
            Errors = errors;
        }

        public static GeneralResponse Succeeded(string? message = null)
            => new(true, message, ResultStatus.Success);

        public static GeneralResponse Created(string? message = null)
            => new(true, message, ResultStatus.Created);

        public static GeneralResponse InvalidInput(string message, IEnumerable<ValidationError>? errors = null)
            => new(false, message, ResultStatus.InvalidInput, errors);

        public static GeneralResponse Unauthenticated(string message)
            => new(false, message, ResultStatus.Unauthenticated);

        public static GeneralResponse Forbidden(string message)
            => new(false, message, ResultStatus.Forbidden);

        public static GeneralResponse NotFound(string message)
            => new(false, message, ResultStatus.NotFound);

        public static GeneralResponse Error(string message)
            => new(false, message, ResultStatus.Error);
    }

    public class GeneralResponse<T> : GeneralResponse
    {
        public T? Data { get; init; }

        [JsonConstructor]
        public GeneralResponse(bool success, T? data, string? message, ResultStatus status = ResultStatus.Success, IEnumerable<ValidationError>? errors = null)
            : base(success, message, status, errors)
        {
            Data = data;
        }

        public static GeneralResponse<T> Succeeded(T data, string? message = null)
            => new(true, data, message, ResultStatus.Success);

        public static GeneralResponse<T> Created(T data, string? message) => new(true, data, message, ResultStatus.Created);

        public new static GeneralResponse<T> InvalidInput(string message, IEnumerable<ValidationError>? errors = null)
            => new(false, default, message, ResultStatus.InvalidInput, errors);

        public new static GeneralResponse<T> Unauthenticated(string message)
            => new(false, default, message, ResultStatus.Unauthenticated);

        public new static GeneralResponse<T> Forbidden(string message)
            => new(false, default, message, ResultStatus.Forbidden);

        public new static GeneralResponse<T> NotFound(string message)
            => new(false, default, message, ResultStatus.NotFound);

        public new static GeneralResponse<T> Error(string message)
            => new(false, default, message, ResultStatus.Error);
    }
}
