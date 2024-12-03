using System;
using RtspClientSharp;

namespace Wpf_DualCam_SpDriving.GUI.Models
{
    interface IMainWindowModel
    {
        event EventHandler<string> StatusChanged;
        IVideoSource VideoSource { get; }
        void Start(ConnectionParameters connectionParameters);
        void Stop();
    }
}