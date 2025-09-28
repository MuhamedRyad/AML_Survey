

namespace AMLSurvey.Core.Abstractions
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }

        //Result without value 
        public static Result Success() => new Result(true, Error.None);
        public static Result Failure(Error error) => new Result(false, error);

        //Result with value  
        public static Result<TValue> Success<TValue>(TValue value) => new Result<TValue>(value, true, Error.None);
        public static Result<TValue> Failure<TValue>(Error error) => new Result<TValue>(default, false, error);


    }

    public class Result<TValue> : Result
    {
        
        private readonly TValue? _value;

        
        public Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            _value = value;
        }


        // if IsSuccess =true return object 
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Failure results cannot have value");

    }
}
