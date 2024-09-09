namespace LogBook_API.Contracts
{

        public class ApiResponse<T>
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public T? Data { get; set; }
            public bool Status { get; set; }
            public IEnumerable<string> Errors { get; set; }

            public static ApiResponse<T> PartialSuccess(short statusCode, T? data, string message, IEnumerable<string> errors)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = statusCode,
                    Data = data,
                    Message = message,
                    Status = true,
                    Errors = errors
                };
            }
            public static ApiResponse<T> Success(short statusCode, T? data, string? message)
            {
                return new ApiResponse<T>()
                {
                    StatusCode = statusCode,
                    Status = true,
                    Data = data,
                    Message = message
                };
            }
            public static ApiResponse<T> Success(short statusCode, string message)
            {
                return new ApiResponse<T>()
                {
                    Status = true,
                    StatusCode = statusCode,
                    Message = message
                };
            }
            public static ApiResponse<T> Failure(short statusCode, T? data, string message)
            {
                return new ApiResponse<T>()
                {
                    Status = false,
                    StatusCode = statusCode,
                    Data = data,
                    Message = message

                };
            }
            public static ApiResponse<T> Failure(short statusCode, string message)
            {
                return new ApiResponse<T>()
                {
                    Status = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
        public static ApiResponse<T> Failure(short statusCode, T? data, string message, IEnumerable<string> errors )
        {
            return new ApiResponse<T>()
            {
                Status = false,
                StatusCode = statusCode,
                Data = data,
                Message = message,
                Errors = errors
                
            };
        }

    }
}
