using System;
using System.Threading.Tasks;

namespace Common
{
	public class Result<TSuccess, TFail>
	{
		public bool Success { get; private set; }

		private readonly TSuccess _value;
		private readonly TFail _error;

		private Result(TSuccess value)
		{
			Success = true;
			_value = value;
			_error = default(TFail);
		}

		private Result(TFail error)
		{
			Success = false;
			_value = default(TSuccess);
			_error = error;
		}

		public T Match<T>(Func<TSuccess, T> onSuccess, Func<TFail, T> onFail)
			=> Success ? onSuccess(_value) : onFail(_error);

		public void Apply(Action<TSuccess> onSuccess, Action<TFail> onFail)
		{
			if (Success)
				onSuccess(_value);
			else
				onFail(_error);
		}

		public static Result<TSuccess, TFail> Succeed(TSuccess value)
			=> new Result<TSuccess, TFail>(value);

		public static Result<TSuccess, TFail> Fail(TFail error)
			=> new Result<TSuccess, TFail>(error);
	}

	public static class ResultExtensions
	{
		public static T Match<T, TSuccess>(this Result<TSuccess, T> result, Func<TSuccess, T> onSuccess)
			=> result.Match(onSuccess, error => error);

		public static Task<T> MatchAsync<T, TSuccess>(this Result<TSuccess, T> result, Func<TSuccess, Task<T>> onSuccess)
			=> result.Match(onSuccess, error => Task.FromResult(error));

		public static async Task<T> MatchAsync<T, TSuccess, TFail>(this Task<Result<TSuccess, TFail>> result, Func<TSuccess, Task<T>> onSuccess, Func<TFail, Task<T>> onFail)
			=> await (await result).Match(value => onSuccess(value), error => onFail(error));

		public static async Task<T> MatchAsync<T, TSuccess>(this Task<Result<TSuccess, T>> result, Func<TSuccess, Task<T>> onSuccess)
			=> await (await result).Match(value => onSuccess(value), error => Task.FromResult(error));
	}
}
