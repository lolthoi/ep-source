namespace Kloon.EmployeePerformance.Models.Common
{
    public class ResultModel<T>
    {
        public ResultModel(T data)
        {
            Data = data;
        }
        public ResultModel(ErrorModel error)
        {
            Error = error;
        }
        public ResultModel(ErrorType type, string errorMessage)
        {
            Error = new ErrorModel(type, errorMessage);
        }
        public T Data { get; private set; }
        public ErrorModel Error { get; private set; }
    }

    public class ErrorModel
    {
        public ErrorModel(ErrorType type, string errorMessage)
        {
            Type = type;
            Message = errorMessage;
        }
        public ErrorType Type { get; private set; }
        public string Message { get; private set; }
    }

    public enum ErrorType
    {
        NOT_AUTHORIZED,
        NO_ROLE,
        NO_DATA_ROLE,
        BAD_REQUEST,
        NOT_EXIST,
        DUPLICATED,
        CONFLICTED,
        INTERNAL_ERROR,
        CONFLICTED_ROLE_CHANGE
    }
}
