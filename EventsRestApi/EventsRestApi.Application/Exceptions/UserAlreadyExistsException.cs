namespace EventsRestApi.Application.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public string Login { get; }

        public UserAlreadyExistsException(string login)
            : base($"Пользователь с логином {login} уже существует.")
        {
            Login = login;
        }

        public UserAlreadyExistsException(string login, Exception innerException)
            : base($"Пользователь с логином '{login}' уже существует.", innerException)
        {
            Login = login;
        }
    }
}
