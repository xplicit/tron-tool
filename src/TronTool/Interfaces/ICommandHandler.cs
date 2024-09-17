namespace Interfaces;

interface ICommandHandler
{
    Task HandleCommands(string [] args);
}