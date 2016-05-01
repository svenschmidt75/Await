/*
 * Name  : MainWindowViewModel.cs
 * Path  :
 * Use   : 
 * Author: Sven Schmidt
 * Date  : 04/30/2016
 */

using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Await.Annotations;

namespace Await
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _url;
        private string _urlLabel;
        private int _progressBarValue;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            AddWebSiteCommand = new CommandHandler(OnAddWebSite, true);
            Url = "http://cnn.com";
            ProgressBarValue = 0;
            AsyncChecked = true;
        }

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }

        public string UrlLabel
        {
            get
            {
                return _urlLabel;
            }

            set
            {
                _urlLabel = value;
                OnPropertyChanged(nameof(UrlLabel));
            }
        }

        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        public bool AsyncChecked { get; set; }

        private async void OnAddWebSite()
        {
            ProgressBarValue = 0;
            string html;
            Mouse.OverrideCursor = Cursors.Wait;
            if (AsyncChecked)
            {
                html = await DownloadUrlAsync();
            }
            else
            {
                html = DownloadUrl();
            }
            UrlLabel = html;
            ProgressBarValue = 0;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private string DownloadUrl()
        {
            string html;
            var request = WebRequest.Create(Url);
            using (var response = request.GetResponse())
            {
                int chunkLength = 1024;
                long totalLength = response.ContentLength;
                int nChunks = (int)(totalLength / chunkLength + 1);
                float step = (float)100 / (nChunks + 1);
                float currentStep = 0;

                var buffer = new byte[chunkLength];
                var temp = new MemoryStream();
                using (var stream = response.GetResponseStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        temp.Write(buffer, 0, count);
                        currentStep += step;
                        ProgressBarValue = (int)currentStep;
                        Task.Delay(100);
                    } while (count > 0);
                    temp.Seek(0, SeekOrigin.Begin);
                    html = new StreamReader(temp).ReadToEnd();
                }
            }
            return html;
        }

        private async Task<string> DownloadUrlAsync()
        {
            string html;
            var request = WebRequest.Create(Url);
            using (var response = await request.GetResponseAsync())
            {
                int chunkLength = 1024;
                long totalLength = response.ContentLength;
                int nChunks = (int) (totalLength/chunkLength + 1);
                float step = (float)100 / (nChunks + 1);
                float currentStep = 0;

                var buffer = new byte[chunkLength];
                var temp = new MemoryStream();
                using (var stream = response.GetResponseStream())
                {
                    int count;
                    do
                    {
                        count = await stream.ReadAsync(buffer, 0, buffer.Length);
                        temp.Write(buffer, 0, count);
                        currentStep += step;
                        ProgressBarValue = (int)currentStep;
                        await Task.Delay(100);
                    } while (count > 0);
                    temp.Seek(0, SeekOrigin.Begin);
                    html = new StreamReader(temp).ReadToEnd();
                }
            }
            return html;
        }

        public ICommand AddWebSiteCommand { get; }

    }
}
