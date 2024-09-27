using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AppliedIntegration.Interfaces;

// This interface was created for running asynchronous commands
public interface IAsyncCommand : ICommand
{
    IEnumerable<Task> RunningTasks { get; }
    bool CanExecute();
    Task ExecuteAsync();
}