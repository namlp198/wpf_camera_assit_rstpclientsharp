using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using RtspClientSharp;
using Wpf_DualCam_SpDriving.Commons;
using Wpf_DualCam_SpDriving.GUI.Models;

namespace Wpf_DualCam_SpDriving.GUI.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";

        private string _status = string.Empty;
        private string _status_2 = string.Empty;
        private readonly IMainWindowModel[] _mainWindowModel = new IMainWindowModel[Defines.MAX_IP_CAM];
        private bool _startButtonEnabled = true;
        private bool _stopButtonEnabled;

        //public string DeviceAddress { get; set; } = "rtsp://192.168.0.20:554/stream1";
        //public string DeviceAddress_2 { get; set; } = "rtsp://192.168.0.21:554/stream1";

        public string DeviceAddress { get; set; } = "rtsp://192.168.0.50:554/cam/realmonitor?channel=1&subtype=0&unicast=true&proto=Onvif";
        public string DeviceAddress_2 { get; set; } = "rtsp://192.168.0.21:554/stream1";

        public string Login { get; set; } = "viewer";
        public string Password { get; set; } = "Haiphuong12!";

        public string Login_2 { get; set; } = "viewer";
        public string Password_2 { get; set; } = "Haiphuong12!";

        public IVideoSource VideoSource
        {
            get
            {
                if (_mainWindowModel[0] == null)
                    return null;

                return _mainWindowModel[0].VideoSource;
            }
        }

        public IVideoSource VideoSource_2
        {
            get
            {
                if (_mainWindowModel[1] == null)
                    return null;

                return _mainWindowModel[1].VideoSource;
            }
        }

        public RelayCommand StartClickCommand { get; }
        public RelayCommand StopClickCommand { get; }
        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public string Status_2
        {
            get => _status_2;
            set
            {
                _status_2 = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(IMainWindowModel[] mainWindowModel)
        {
            _mainWindowModel = mainWindowModel ?? throw new ArgumentNullException(nameof(mainWindowModel));

            StartClickCommand = new RelayCommand(OnStartButtonClick, () => _startButtonEnabled);
            StopClickCommand = new RelayCommand(OnStopButtonClick, () => _stopButtonEnabled);
            ClosingCommand = new RelayCommand<CancelEventArgs>(OnClosing);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStartButtonClick()
        {
            string address = string.Empty;
            NetworkCredential credential = null;
            ConnectionParameters connectionParameters = null;

            for (int i = 0; i < Defines.MAX_IP_CAM; i++)
            {
                switch (i)
                {
                    case 0:
                        address = DeviceAddress;

                        if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                            address = RtspPrefix + address;

                        if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
                        {
                            MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        credential = new NetworkCredential(Login, Password);

                        connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) :
                            new ConnectionParameters(deviceUri, credential);

                        connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
                        connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

                        _mainWindowModel[i].Start(connectionParameters);
                        _mainWindowModel[i].StatusChanged += MainWindowModelOnStatusChanged;
                        break;

                    case 1:
                        address = DeviceAddress_2;

                        if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                            address = RtspPrefix + address;

                        if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri_2))
                        {
                            MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        credential = new NetworkCredential(Login_2, Password_2);

                        connectionParameters = !string.IsNullOrEmpty(deviceUri_2.UserInfo) ? new ConnectionParameters(deviceUri_2) :
                            new ConnectionParameters(deviceUri_2, credential);

                        connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
                        connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

                        _mainWindowModel[i].Start(connectionParameters);
                        _mainWindowModel[i].StatusChanged += MainWindowModelOnStatusChanged_2;
                        break;
                }
            }

            _startButtonEnabled = false;
            StartClickCommand.RaiseCanExecuteChanged();
            _stopButtonEnabled = true;
            StopClickCommand.RaiseCanExecuteChanged();
        }

        private void OnStopButtonClick()
        {
            for (int i = 0; i < Defines.MAX_IP_CAM; i++)
            {
                _mainWindowModel[i].Stop();
                _mainWindowModel[i].StatusChanged -= MainWindowModelOnStatusChanged;
                _mainWindowModel[i].StatusChanged -= MainWindowModelOnStatusChanged_2;

            }

            _stopButtonEnabled = false;
            StopClickCommand.RaiseCanExecuteChanged();
            _startButtonEnabled = true;
            StartClickCommand.RaiseCanExecuteChanged();
            Status = string.Empty;
            Status_2 = string.Empty;
        }

        private void MainWindowModelOnStatusChanged(object sender, string s)
        {
            Application.Current.Dispatcher.Invoke(() => Status = s);
        }
        private void MainWindowModelOnStatusChanged_2(object sender, string s)
        {
            Application.Current.Dispatcher.Invoke(() => Status_2 = s);
        }

        private void OnClosing(CancelEventArgs args)
        {
            for (int i = 0; i < Defines.MAX_IP_CAM; i++)
            {
                _mainWindowModel[i].Stop();
            }
        }
    }
}