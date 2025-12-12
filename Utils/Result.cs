using System.Text.Json.Serialization;

namespace LmsApi.Utils
{
    public class Result<T>
    {
        public bool Success { get; private set; }
        public string? Error { get; private set; }
        public T? Value { get; private set; }
        public bool IsFailure => !Success;

        private Result() { } // for serializers (if needed)

        public static Result<T> Ok(T value) => new() { Success = true, Value = value };
        public static Result<T> Fail(string error) => new() { Success = false, Error = error };

        // Prevent accidental JSON serialization in most cases
        [JsonIgnore] public bool ShouldSerializeValue => Success;
        [JsonIgnore] public bool ShouldSerializeError => !Success;
    }
}
