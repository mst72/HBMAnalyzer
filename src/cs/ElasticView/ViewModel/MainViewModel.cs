using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ElasticView.Classes;
using ElasticView.Model;

namespace ElasticView.ViewModel
{
    public class MainViewModel: ViewModelBase
    {
        private string _status = "Idle";

        public MainViewModel()
        {
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged( () => Status ); }
        }

        /// <summary>
        ///     Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        private void OnRequestClose()
        {
            EventHandler handler = RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(param => OnRequestClose())); }
        }

        private RelayCommand _connectCommand;
        private bool _isConnected = false;

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommand(param => OnConnect(), x => !_isConnected));
            }
        }

        private MainModel _model;
        public MainModel Model
        {
            get { return _model ?? (_model = new MainModel()); }
        }

        private RelayCommand _executeCommand;
        public ICommand ExecuteCommand
        {
            get { return _executeCommand ?? (_executeCommand = new RelayCommand(param => OnExecute())); }
        }

        private void OnExecute()
        {
            if (!_isConnected)
            {
                OnConnect();
            }
            Status = "Busy";
            Task.Factory.StartNew(() =>
            {
                Status = "Busy";
                return 0;
            }).ContinueWith(x => Model.ExecuteSomething()).ContinueWith(x => Status = "Online");
        }

        private void OnConnect()
        {
            _isConnected = Model.Connect();
            if (_isConnected)
            {
                Status = "Online";
            }
        }
    }
}
