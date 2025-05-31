using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;


namespace DebloaterPro.Services
{
    public class NotificationService
    {
        private static NotificationService _instance;
        private InfoBar _infoBar;

        public static NotificationService Instance => _instance ??= new NotificationService();

        public void Initialize(InfoBar infoBar)
        {
            _infoBar = infoBar;
        }

        public void ShowSuccess(string message, string title = "Success", bool autoHide = true)
        {
            ShowInfoBar(message, title, InfoBarSeverity.Success, autoHide);
        }

        public void ShowWarning(string message, string title = "Warning", bool autoHide = true)
        {
            ShowInfoBar(message, title, InfoBarSeverity.Warning, autoHide);
        }

        public void ShowError(string message, string title = "Error", bool autoHide = true)
        {
            ShowInfoBar(message, title, InfoBarSeverity.Error, autoHide);
        }

        public void Hide()
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                _infoBar.IsOpen = false;
            });
        }

        private async void ShowInfoBar(string message, string title, InfoBarSeverity severity, bool autoHide)
        {
            if (_infoBar == null) return;

            _infoBar.DispatcherQueue.TryEnqueue(() =>
            {
                _infoBar.Title = title;
                _infoBar.Message = message;
                _infoBar.Severity = severity;
                _infoBar.IsOpen = true;
            });

            if (autoHide)
            {
                await Task.Delay(3000);
                Hide();
            }
        }
    }
}