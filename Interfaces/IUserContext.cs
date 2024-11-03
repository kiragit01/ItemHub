namespace ItemHub.Interfaces;

public interface IUserContext
{
    string Login { get; }
    string Email { get; }
    string Name { get; }
}