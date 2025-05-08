namespace RefactorThis.Domain
{
    public class Result
    {
        private readonly bool _isSuccess;
        private readonly string _message;

        private Result(bool isSuccess, string message)
        {
            _isSuccess = isSuccess;
            _message = message;
        }

        public bool IsSuccess() => _isSuccess;
        public string GetMessage() => _message;
        public static Result Success(string message) => new Result(true, message);
        public static Result Failure(string errorMessage) => new Result(false, errorMessage);
    }
}