using System;
using Wpf_DualCam_SpDriving.Commons;
using Wpf_DualCam_SpDriving.GUI.Models;
using Wpf_DualCam_SpDriving.GUI.ViewModels;

namespace Wpf_DualCam_SpDriving
{
    class ViewModelLocator
    {
        private readonly Lazy<MainWindowViewModel> _mainWindowViewModelLazy =
            new Lazy<MainWindowViewModel>(CreateMainWindowViewModel);

        public MainWindowViewModel MainWindowViewModel => _mainWindowViewModelLazy.Value;

        private static MainWindowViewModel CreateMainWindowViewModel()
        {
            MainWindowModel[] models = new MainWindowModel[Defines.MAX_IP_CAM];
            for (int i = 0; i < Defines.MAX_IP_CAM; i++)
            {
                models[i] = new MainWindowModel();
            }
           
            return new MainWindowViewModel(models);
        }
    }
}