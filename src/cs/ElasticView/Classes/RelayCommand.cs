using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using log4net;

namespace ElasticView.Classes
{
    /// <summary>
    /// A command whose sole purpose is to 
    /// relay its functionality to other
    /// objects by invoking delegates. The
    /// default return value for the CanExecute
    /// method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand, IDisposable
    {
        private readonly object _lock = new object();

        private Action<object> _execute;
        private Predicate<object> _canExecute;

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand( Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~RelayCommand()
        {
            Dispose();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            lock (_lock)
            {
                return _canExecute == null || _canExecute(parameter);
            }
        }

        /// <summary>
        /// Event that occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            var can = CanExecute(parameter);
            lock (_lock)
            {
                if (_execute != null && can)
                {
                    try
                    {
                        _execute(parameter);
                    }
                    catch (Exception ex)
                    {
                        ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                        logger.ErrorFormat("Failed to execute command: {0}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Requery command
        /// </summary>
        public void Requery()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            lock (_lock)
            {
                _execute = null;
                _canExecute = null;
            }
        }
    }
}