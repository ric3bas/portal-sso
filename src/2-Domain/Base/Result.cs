namespace Portal.Domain.Base
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public static class Result
    {
        public static Result<T> OkResult<T>(T data) => Result<T>.Ok(data);
        public static Result<T> ValidationResult<T>(string error) => Result<T>.Validation(error);
        public static Result<T> ValidationResult<T>(IEnumerable<string> errors) => Result<T>.Validation(errors);
        public static Result<T> BusinessResult<T>(string message) => Result<T>.Business(message);
        public static Result<T> NotFoundResult<T>(string message) => Result<T>.NotFound(message);
    }

    public class Result<T>
    {
        public bool Success => Type == ResultType.Success;
        public ResultType Type { get; private set; }
        public T? Data { get; private set; }
        public CustomProblemDetails? Problem { get; private set; }

        private Result(ResultType type, T? data, CustomProblemDetails? problem)
        {
            Type = type;
            Data = data;
            Problem = problem;
        }

        public static Result<T> Ok(T data) =>
            new Result<T>(ResultType.Success, data, null);


        public static Result<T> Validation(string error) =>
            new Result<T>(ResultType.Validation, default, new CustomProblemDetails
            {
                Errors = [error]
            });

        public static Result<T> Validation(IEnumerable<string> errors) =>
            new Result<T>(ResultType.Validation, default, new CustomProblemDetails
            {
                Errors = errors.ToArray()
            });

        public static Result<T> Business(string message) =>
            new Result<T>(ResultType.Business, default, new CustomProblemDetails
            {
                Errors = [message]
            });

        public static Result<T> Business(IEnumerable<string> errors) =>
            new Result<T>(ResultType.Business, default, new CustomProblemDetails
         {
             Errors = errors.ToArray()
         });

        public static Result<T> NotFound(string message) =>
            new Result<T>(ResultType.NotFound, default, new CustomProblemDetails
            {
                Errors = [message]
            });
    }

  
}
