using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ElasticView.Extensions;

namespace ElasticView.Classes
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private readonly IDictionary<string, RelayCommand> _commands = new Dictionary<string, RelayCommand>(StringComparer.Ordinal);

        protected ICommand GetCommand( Expression<Func<object>> commandName,  Action<object> execute, Predicate<object> canExecute = null)
        {
            if (commandName == null) throw new ArgumentNullException("commandName");
            if (execute == null) throw new ArgumentNullException("execute");

            lock (_commands)
            {
                return _commands.GetOrCreate(commandName.GetPropertyName(), () => new RelayCommand(execute, canExecute));
            }
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( Expression<Func<object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            HandlePropertyChanged(propertyExpression.GetPropertyName());
        }

        protected void HandlePropertyChanged( string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
            lock (_commands)
            {
                foreach (var command in _commands.Values)
                    command.Dispose();

                _commands.Clear();
            }
        }

        /// <summary>
        /// Для обработки "Ok"-нажатия
        /// </summary>
        public virtual void UpdateModel()
        {
        }

        /// <summary>
        /// Проверка правильности заполнения модели (моделей)
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateAll()
        {
            return true;
        }
        /// <summary>
        /// Проверка правильности заполнения модели (моделей)
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateAll(out string err)
        {
            err = string.Empty;
            return true;
        }

        /// <summary>
        /// Обычная перезагрузка (если потребуется)
        /// </summary>
        public virtual void Reload()
        {
        }

        protected Dispatcher Dispatcher
        {
            get
            {
                return Application.Current.Dispatcher;
            }
        }
    }
}