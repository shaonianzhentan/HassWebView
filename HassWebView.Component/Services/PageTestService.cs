using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace HassWebView.Component.Services
{
    public class PageTestService
    {
        private readonly Shell _shell;
        private List<string> _pageRoutes;
        private int _currentIndex = 0;
        private bool _isTesting = false;
        private List<string> _failedPages = new List<string>();
        private List<string> _passedPages = new List<string>();
        private int _testIntervalMs = 2000;

        public event EventHandler<PageTestEventArgs> TestCompleted;
        public event EventHandler<PageTestEventArgs> PageTested;

        public PageTestService(Shell shell)
        {
            _shell = shell;
            DiscoverPages();
        }

        private void DiscoverPages()
        {
            _pageRoutes = new List<string>();
            
            foreach (var item in _shell.Items)
            {
                if (item is FlyoutItem flyoutItem)
                {
                    _pageRoutes.Add(flyoutItem.Title);
                }
            }
        }

        public async Task StartTestingAsync()
        {
            if (_isTesting) return;
            
            _isTesting = true;
            _currentIndex = 0;
            _failedPages.Clear();
            _passedPages.Clear();

            Console.WriteLine("\n=== 开始页面测试 ===");
            Console.WriteLine($"共发现 {_pageRoutes.Count} 个页面需要测试\n");

            while (_currentIndex < _pageRoutes.Count && _isTesting)
            {
                var route = _pageRoutes[_currentIndex];
                await TestPageAsync(route);
                _currentIndex++;
                
                await Task.Delay(_testIntervalMs);
            }

            _isTesting = false;
            OnTestCompleted(new PageTestEventArgs(
                _passedPages, 
                _failedPages, 
                DateTime.Now));
        }

        private async Task TestPageAsync(string route)
        {
            try
            {
                await _shell.GoToAsync($"//{route}");
                await Task.Delay(500);
                
                _passedPages.Add(route);
                OnPageTested(new PageTestEventArgs(route, true, null));
            }
            catch (Exception ex)
            {
                _failedPages.Add(route);
                OnPageTested(new PageTestEventArgs(route, false, ex.Message));
            }
        }

        public void StopTesting()
        {
            _isTesting = false;
        }

        public int TotalPages => _pageRoutes.Count;
        public int PassedCount => _passedPages.Count;
        public int FailedCount => _failedPages.Count;
        public List<string> FailedPages => _failedPages;
        public List<string> PassedPages => _passedPages;

        protected virtual void OnPageTested(PageTestEventArgs e)
        {
            PageTested?.Invoke(this, e);
        }

        protected virtual void OnTestCompleted(PageTestEventArgs e)
        {
            TestCompleted?.Invoke(this, e);
        }
    }

    public class PageTestEventArgs : EventArgs
    {
        public string PageName { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }
        public List<string> PassedPages { get; }
        public List<string> FailedPages { get; }
        public DateTime TestTime { get; }

        public PageTestEventArgs(string pageName, bool success, string errorMessage)
        {
            PageName = pageName;
            Success = success;
            ErrorMessage = errorMessage;
        }

        public PageTestEventArgs(List<string> passedPages, List<string> failedPages, DateTime testTime)
        {
            PassedPages = passedPages;
            FailedPages = failedPages;
            TestTime = testTime;
        }
    }
}