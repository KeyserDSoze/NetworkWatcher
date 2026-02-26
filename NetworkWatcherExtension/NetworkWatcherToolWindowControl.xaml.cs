using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetworkWatcherExtension
{
    /// <summary>
    /// Interaction logic for NetworkWatcherToolWindowControl.
    /// </summary>
    public partial class NetworkWatcherToolWindowControl : UserControl, IDisposable
    {
        private ProxyServer proxy;
        private ExplicitProxyEndPoint proxyEndpoint;
        private ObservableCollection<HttpTrafficEntry> allEntries = new ObservableCollection<HttpTrafficEntry>();
        private ObservableCollection<HttpTrafficEntry> filteredEntries = new ObservableCollection<HttpTrafficEntry>();
        private bool isProxyRunning = false;
        private bool disposed = false;

        // Search state for each detail box
        private List<int> requestHeadersSearchMatches = new List<int>();
        private List<int> requestBodySearchMatches = new List<int>();
        private List<int> responseHeadersSearchMatches = new List<int>();
        private List<int> responseBodySearchMatches = new List<int>();
        private int requestHeadersCurrentMatch = -1;
        private int requestBodyCurrentMatch = -1;
        private int responseHeadersCurrentMatch = -1;
        private int responseBodyCurrentMatch = -1;

        public NetworkWatcherToolWindowControl()
        {
            this.InitializeComponent();

            RequestListView.ItemsSource = filteredEntries;

            // Setup KeyDown handlers for search boxes to navigate with Enter
            RequestHeadersSearchBox.KeyDown += SearchBox_KeyDown;
            RequestBodySearchBox.KeyDown += SearchBox_KeyDown;
            ResponseHeadersSearchBox.KeyDown += SearchBox_KeyDown;
            ResponseBodySearchBox.KeyDown += SearchBox_KeyDown;

            // Ensure proxy is stopped when control is unloaded
            this.Unloaded += NetworkWatcherToolWindowControl_Unloaded;

            // Also handle when the application is closing
            Application.Current.Exit += Application_Exit;

            // Ensure UI reflects actual proxy state when control is loaded
            this.Loaded += NetworkWatcherToolWindowControl_Loaded;
        }

        private void NetworkWatcherToolWindowControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Synchronize UI with actual proxy state
            // This handles cases where the control is reloaded but proxy state is inconsistent
            // IMPORTANT: Don't reset if proxy is actually running - this prevents issues during debug cycles

            if (proxy != null && isProxyRunning)
            {
                // Proxy is running, ensure UI reflects this
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                StatusText.Text = "Running on port 8888 (System Proxy Active)";
            }
            else if (proxy == null && !isProxyRunning)
            {
                // Proxy is stopped, ensure UI reflects this
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                StatusText.Text = "Stopped";
            }
            else
            {
                // Inconsistent state - proxy object exists but flag says not running, or vice versa
                // This shouldn't happen, but if it does, trust the proxy object
                if (proxy != null)
                {
                    // Proxy exists, assume it's running
                    isProxyRunning = true;
                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = true;
                    StatusText.Text = "Running on port 8888 (System Proxy Active)";
                }
                else
                {
                    // No proxy, ensure it's marked as stopped
                    isProxyRunning = false;
                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                    StatusText.Text = "Stopped";
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Dispose();
        }

        private void NetworkWatcherToolWindowControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected Dispose method
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    try
                    {
                        // Unsubscribe from events to prevent memory leaks
                        this.Loaded -= NetworkWatcherToolWindowControl_Loaded;
                        this.Unloaded -= NetworkWatcherToolWindowControl_Unloaded;
                        if (Application.Current != null)
                        {
                            Application.Current.Exit -= Application_Exit;
                        }

                        // Stop proxy and cleanup
                        EnsureProxyStopped();
                    }
                    catch
                    {
                        // Ignore errors during disposal
                    }
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~NetworkWatcherToolWindowControl()
        {
            Dispose(false);
        }

        private void EnsureProxyStopped()
        {
            if (isProxyRunning || proxy != null)
            {
                try
                {
                    StopProxy();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await StartProxyAsync();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopProxy();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            allEntries.Clear();
            filteredEntries.Clear();
            ClearDetailsView();
        }

        private async Task StartProxyAsync()
        {
            try
            {
                if (isProxyRunning)
                {
                    MessageBox.Show("Proxy is already running!", "Network Watcher", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Make sure any previous proxy is completely stopped
                if (proxy != null)
                {
                    try
                    {
                        StopProxy();
                        // Wait a bit for the port to be released
                        await Task.Delay(500);
                    }
                    catch { }
                }

                proxy = new ProxyServer();

                // Setup certificate
                proxy.CertificateManager.CreateRootCertificate(false);
                proxy.CertificateManager.TrustRootCertificate(true);

                // Setup event handlers
                proxy.BeforeRequest += OnRequest;
                proxy.BeforeResponse += OnResponse;
                proxy.ServerCertificateValidationCallback += OnCertificateValidation;

                // Create and add endpoint
                proxyEndpoint = new ExplicitProxyEndPoint(
                    System.Net.IPAddress.Loopback,
                    8888,
                    true);

                proxy.AddEndPoint(proxyEndpoint);
                proxy.Start();

                // Wait a moment for the proxy to fully start
                await Task.Delay(200);

                // Set system proxy so all apps automatically use it
                ProxyHelper.SetSystemProxy("127.0.0.1", 8888);

                isProxyRunning = true;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                StatusText.Text = "Running on port 8888 (System Proxy Active)";
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                MessageBox.Show($"Failed to start proxy: {ex.Message}\n\nTry running Visual Studio as Administrator.", 
                    "Network Watcher", MessageBoxButton.OK, MessageBoxImage.Error);

                // Cleanup on error
                try
                {
                    ProxyHelper.UnsetSystemProxy();
                    if (proxyEndpoint != null && proxy != null)
                    {
                        proxy.RemoveEndPoint(proxyEndpoint);
                        proxyEndpoint = null;
                    }
                    proxy?.Stop();
                    proxy?.Dispose();
                    proxy = null;
                }
                catch { }

                isProxyRunning = false;
            }
        }

        private void StopProxy()
        {
            try
            {
                // Always try to unset system proxy first, even if proxy is null
                try
                {
                    ProxyHelper.UnsetSystemProxy();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error unsetting system proxy: {ex.Message}");
                }

                if (proxy != null)
                {
                    try
                    {
                        // Unsubscribe from events
                        proxy.BeforeRequest -= OnRequest;
                        proxy.BeforeResponse -= OnResponse;
                        proxy.ServerCertificateValidationCallback -= OnCertificateValidation;

                        // Remove endpoint explicitly before stopping
                        if (proxyEndpoint != null)
                        {
                            try
                            {
                                proxy.RemoveEndPoint(proxyEndpoint);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error removing endpoint: {ex.Message}");
                            }
                            proxyEndpoint = null;
                        }

                        // Stop the proxy server
                        proxy.Stop();

                        // Untrust the certificate to clean up
                        try
                        {
                            proxy.CertificateManager.TrustRootCertificate(false);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error untrusting certificate: {ex.Message}");
                        }

                        // Dispose the proxy
                        proxy.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping proxy: {ex.Message}");
                    }
                    finally
                    {
                        proxy = null;
                    }
                }

                isProxyRunning = false;

                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                StatusText.Text = "Stopped";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping proxy: {ex.Message}", "Network Watcher",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // Accept all certificates for debugging purposes
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;
            
            return Task.CompletedTask;
        }

        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            try
            {
                var request = e.HttpClient.Request;
                var entry = new HttpTrafficEntry
                {
                    Time = DateTime.Now.ToString("HH:mm:ss"),
                    Method = request.Method,
                    Url = request.Url,
                    Protocol = request.Url.StartsWith("https", StringComparison.OrdinalIgnoreCase) ? "HTTPS" : "HTTP"
                };

                var headers = new StringBuilder();
                headers.AppendLine($"{request.Method} {request.RequestUri.PathAndQuery} HTTP/{request.HttpVersion}");
                headers.AppendLine($"Host: {request.RequestUri.Host}");
                foreach (var h in request.Headers)
                {
                    headers.AppendLine($"{h.Name}: {h.Value}");
                }
                entry.RequestHeaders = headers.ToString();

                // Read request body if present
                if (request.HasBody)
                {
                    try
                    {
                        var bodyBytes = await e.GetRequestBody();
                        var bodyString = Encoding.UTF8.GetString(bodyBytes);
                        entry.RequestBody = TryFormatJson(bodyString, request.ContentType);

                        // Important: Set the body back so the request can continue
                        e.SetRequestBody(bodyBytes);
                    }
                    catch (Exception ex)
                    {
                        entry.RequestBody = $"[Error reading body: {ex.Message}]";
                    }
                }
                else
                {
                    entry.RequestBody = "[No body]";
                }

                // Update UI on background thread to avoid blocking
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                allEntries.Add(entry);

                // Use Dispatcher.BeginInvoke with low priority to avoid blocking the UI
                Dispatcher.BeginInvoke(new Action(() => ApplyFilters()), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                // Log error but don't break the proxy
                System.Diagnostics.Debug.WriteLine($"Error in OnRequest: {ex.Message}");
            }
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            try
            {
                var request = e.HttpClient.Request;
                var response = e.HttpClient.Response;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var entry = allEntries.FirstOrDefault(x => x.Url == request.Url && string.IsNullOrEmpty(x.Status));

                if (entry != null)
                {
                    entry.Status = ((int)response.StatusCode).ToString();

                    var headers = new StringBuilder();
                    headers.AppendLine($"HTTP/{response.HttpVersion} {(int)response.StatusCode} {response.StatusCode}");
                    foreach (var h in response.Headers)
                    {
                        headers.AppendLine($"{h.Name}: {h.Value}");
                    }
                    entry.ResponseHeaders = headers.ToString();

                    // Read response body if present
                    if (response.HasBody)
                    {
                        try
                        {
                            var bodyBytes = await e.GetResponseBody();
                            var bodyString = Encoding.UTF8.GetString(bodyBytes);
                            entry.ResponseBody = TryFormatJson(bodyString, response.ContentType);

                            // Important: Set the body back so the response can be delivered to the client
                            e.SetResponseBody(bodyBytes);
                        }
                        catch (Exception ex)
                        {
                            entry.ResponseBody = $"[Error reading body: {ex.Message}]";
                        }
                    }
                    else
                    {
                        entry.ResponseBody = "[No body]";
                    }

                    // Use Dispatcher.BeginInvoke with low priority to avoid blocking the UI
                    Dispatcher.BeginInvoke(new Action(() => ApplyFilters()), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the proxy
                System.Diagnostics.Debug.WriteLine($"Error in OnResponse: {ex.Message}");
            }
        }

        private string TryFormatJson(string content, string contentType)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content ?? string.Empty;

            // Check content-type first
            bool isJson = contentType != null && contentType.ToLowerInvariant().Contains("json");

            // If not JSON by content-type, try to detect by content (starts with { or [)
            if (!isJson)
            {
                var trimmed = content.TrimStart();
                isJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
            }

            if (isJson)
            {
                try
                {
                    // Parse and format JSON using Newtonsoft.Json
                    var jsonObject = JsonConvert.DeserializeObject(content);
                    return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch
                {
                    // If parsing fails, return original content
                    // This means it looked like JSON but wasn't valid
                }
            }

            return content;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (filteredEntries == null || allEntries == null)
                return;

            // Save current selection to preserve it after filtering
            var currentSelection = RequestListView?.SelectedItem as HttpTrafficEntry;

            var domainFilter = DomainFilterBox?.Text?.Trim().ToLowerInvariant();
            var protocolFilter = (ProtocolFilterBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();

            filteredEntries.Clear();

            foreach (var entry in allEntries)
            {
                bool matchesDomain = string.IsNullOrWhiteSpace(domainFilter) || 
                                    entry.Url.ToLowerInvariant().Contains(domainFilter.ToLowerInvariant());

                bool matchesProtocol = protocolFilter == "All" || 
                                      entry.Protocol == protocolFilter;

                if (matchesDomain && matchesProtocol)
                {
                    filteredEntries.Add(entry);
                }
            }

            // Restore selection if the previously selected item is still in the filtered list
            if (currentSelection != null && filteredEntries.Contains(currentSelection))
            {
                if (RequestListView != null)
                {
                    RequestListView.SelectedItem = currentSelection;
                }

                // Don't scroll to the newly added item, keep the view where it was
                // This prevents auto-scrolling when new requests come in
            }
        }

        private void RequestListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RequestListView.SelectedItem is HttpTrafficEntry entry)
            {
                SetRichTextBoxText(RequestHeadersBox, entry.RequestHeaders ?? string.Empty);
                SetRichTextBoxText(RequestBodyBox, entry.RequestBody ?? string.Empty);
                SetRichTextBoxText(ResponseHeadersBox, entry.ResponseHeaders ?? string.Empty);
                SetRichTextBoxText(ResponseBodyBox, entry.ResponseBody ?? string.Empty);
            }
            else
            {
                ClearDetailsView();
            }
        }

        private void SetRichTextBoxText(RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            if (!string.IsNullOrEmpty(text))
            {
                var paragraph = new Paragraph(new Run(text));
                paragraph.Margin = new Thickness(0);
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        private void ClearDetailsView()
        {
            RequestHeadersBox.Document.Blocks.Clear();
            RequestBodyBox.Document.Blocks.Clear();
            ResponseHeadersBox.Document.Blocks.Clear();
            ResponseBodyBox.Document.Blocks.Clear();

            // Clear search boxes and results
            RequestHeadersSearchBox.Clear();
            RequestBodySearchBox.Clear();
            ResponseHeadersSearchBox.Clear();
            ResponseBodySearchBox.Clear();
            RequestHeadersSearchResult.Text = string.Empty;
            RequestBodySearchResult.Text = string.Empty;
            ResponseHeadersSearchResult.Text = string.Empty;
            ResponseBodySearchResult.Text = string.Empty;

            // Clear search state
            requestHeadersSearchMatches.Clear();
            requestBodySearchMatches.Clear();
            responseHeadersSearchMatches.Clear();
            responseBodySearchMatches.Clear();
            requestHeadersCurrentMatch = -1;
            requestBodyCurrentMatch = -1;
            responseHeadersCurrentMatch = -1;
            responseBodyCurrentMatch = -1;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox == null)
                return;

            var searchText = searchBox.Text;
            RichTextBox targetBox = null;
            TextBlock resultLabel = null;
            List<int> matchList = null;

            // Determine which detail box, result label, and match list to use
            if (searchBox == RequestHeadersSearchBox)
            {
                targetBox = RequestHeadersBox;
                resultLabel = RequestHeadersSearchResult;
                matchList = requestHeadersSearchMatches;
            }
            else if (searchBox == RequestBodySearchBox)
            {
                targetBox = RequestBodyBox;
                resultLabel = RequestBodySearchResult;
                matchList = requestBodySearchMatches;
            }
            else if (searchBox == ResponseHeadersSearchBox)
            {
                targetBox = ResponseHeadersBox;
                resultLabel = ResponseHeadersSearchResult;
                matchList = responseHeadersSearchMatches;
            }
            else if (searchBox == ResponseBodySearchBox)
            {
                targetBox = ResponseBodyBox;
                resultLabel = ResponseBodySearchResult;
                matchList = responseBodySearchMatches;
            }

            if (targetBox == null || resultLabel == null || matchList == null)
                return;

            // Get the full text from the RichTextBox
            var textRange = new TextRange(targetBox.Document.ContentStart, targetBox.Document.ContentEnd);
            var content = textRange.Text;

            // Clear previous search results
            matchList.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                resultLabel.Text = string.Empty;
                SetCurrentMatchIndex(searchBox, -1);
                // Reset text formatting (remove red highlights)
                HighlightMatches(targetBox, content, null, 0);
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                resultLabel.Text = "No content";
                SetCurrentMatchIndex(searchBox, -1);
                return;
            }

            // Find all occurrences (case-insensitive)
            int tempIndex = 0;
            while ((tempIndex = content.IndexOf(searchText, tempIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                matchList.Add(tempIndex);
                tempIndex += searchText.Length;
            }

            if (matchList.Count > 0)
            {
                // Start at the first match
                SetCurrentMatchIndex(searchBox, 0);
                resultLabel.Text = $"1/{matchList.Count} matches";

                // Highlight ALL matches in red
                HighlightMatches(targetBox, content, searchText, matchList.Count);

                // Scroll to first match
                ScrollToMatch(targetBox, matchList[0]);
            }
            else
            {
                resultLabel.Text = "No matches found";
                SetCurrentMatchIndex(searchBox, -1);
                // Reset formatting
                HighlightMatches(targetBox, content, null, 0);
            }
        }

        private void HighlightMatches(RichTextBox richTextBox, string fullText, string searchText, int matchCount)
        {
            richTextBox.Document.Blocks.Clear();

            if (string.IsNullOrWhiteSpace(searchText) || matchCount == 0)
            {
                // No highlighting, just plain text
                var paragraph = new Paragraph(new Run(fullText));
                paragraph.Margin = new Thickness(0);
                richTextBox.Document.Blocks.Add(paragraph);
                return;
            }

            // Create paragraph with highlighted matches
            var para = new Paragraph();
            para.Margin = new Thickness(0);

            int lastIndex = 0;
            int searchIndex = 0;

            while ((searchIndex = fullText.IndexOf(searchText, lastIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                // Add normal text before the match
                if (searchIndex > lastIndex)
                {
                    para.Inlines.Add(new Run(fullText.Substring(lastIndex, searchIndex - lastIndex))
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)) // #AAAAAA
                    });
                }

                // Add highlighted match in RED
                para.Inlines.Add(new Run(fullText.Substring(searchIndex, searchText.Length))
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)), // Red
                    FontWeight = FontWeights.Bold
                });

                lastIndex = searchIndex + searchText.Length;
            }

            // Add remaining text after last match
            if (lastIndex < fullText.Length)
            {
                para.Inlines.Add(new Run(fullText.Substring(lastIndex))
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA))
                });
            }

            richTextBox.Document.Blocks.Add(para);
        }

        private void ScrollToMatch(RichTextBox richTextBox, int matchPosition)
        {
            try
            {
                var start = richTextBox.Document.ContentStart;
                var pointer = start.GetPositionAtOffset(matchPosition);
                if (pointer != null)
                {
                    var rect = pointer.GetCharacterRect(LogicalDirection.Forward);
                    richTextBox.ScrollToVerticalOffset(rect.Top);
                }
            }
            catch
            {
                // Ignore scrolling errors
            }
        }

        private void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            var searchBox = sender as TextBox;
            if (searchBox == null)
                return;

            var searchText = searchBox.Text;
            if (string.IsNullOrWhiteSpace(searchText))
                return;

            RichTextBox targetBox = null;
            TextBlock resultLabel = null;
            List<int> matchList = null;
            int currentMatchIndex = -1;

            // Determine which detail box to use
            if (searchBox == RequestHeadersSearchBox)
            {
                targetBox = RequestHeadersBox;
                resultLabel = RequestHeadersSearchResult;
                matchList = requestHeadersSearchMatches;
                currentMatchIndex = requestHeadersCurrentMatch;
            }
            else if (searchBox == RequestBodySearchBox)
            {
                targetBox = RequestBodyBox;
                resultLabel = RequestBodySearchResult;
                matchList = requestBodySearchMatches;
                currentMatchIndex = requestBodyCurrentMatch;
            }
            else if (searchBox == ResponseHeadersSearchBox)
            {
                targetBox = ResponseHeadersBox;
                resultLabel = ResponseHeadersSearchResult;
                matchList = responseHeadersSearchMatches;
                currentMatchIndex = responseHeadersCurrentMatch;
            }
            else if (searchBox == ResponseBodySearchBox)
            {
                targetBox = ResponseBodyBox;
                resultLabel = ResponseBodySearchResult;
                matchList = responseBodySearchMatches;
                currentMatchIndex = responseBodyCurrentMatch;
            }

            if (targetBox == null || resultLabel == null || matchList == null || matchList.Count == 0)
                return;

            // Navigate to next match (wrap around to 0 when reaching the end)
            currentMatchIndex = (currentMatchIndex + 1) % matchList.Count;
            SetCurrentMatchIndex(searchBox, currentMatchIndex);

            // Update label and scroll
            resultLabel.Text = $"{currentMatchIndex + 1}/{matchList.Count} matches";
            ScrollToMatch(targetBox, matchList[currentMatchIndex]);

            // Prevent the Enter key from doing anything else (like triggering button clicks)
            e.Handled = true;
        }

        private void SetCurrentMatchIndex(TextBox searchBox, int index)
        {
            if (searchBox == RequestHeadersSearchBox)
                requestHeadersCurrentMatch = index;
            else if (searchBox == RequestBodySearchBox)
                requestBodyCurrentMatch = index;
            else if (searchBox == ResponseHeadersSearchBox)
                responseHeadersCurrentMatch = index;
            else if (searchBox == ResponseBodySearchBox)
                responseBodyCurrentMatch = index;
        }
    }

    public class HttpTrafficEntry : System.ComponentModel.INotifyPropertyChanged
    {
        private string time;
        private string method;
        private string protocol;
        private string url;
        private string status;
        private string requestHeaders;
        private string requestBody;
        private string responseHeaders;
        private string responseBody;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public string Time 
        { 
            get => time; 
            set { time = value; OnPropertyChanged(nameof(Time)); } 
        }

        public string Method 
        { 
            get => method; 
            set { method = value; OnPropertyChanged(nameof(Method)); } 
        }

        public string Protocol 
        { 
            get => protocol; 
            set { protocol = value; OnPropertyChanged(nameof(Protocol)); } 
        }

        public string Url 
        { 
            get => url; 
            set { url = value; OnPropertyChanged(nameof(Url)); } 
        }

        public string Status 
        { 
            get => status; 
            set { status = value; OnPropertyChanged(nameof(Status)); } 
        }

        public string RequestHeaders 
        { 
            get => requestHeaders; 
            set { requestHeaders = value; OnPropertyChanged(nameof(RequestHeaders)); } 
        }

        public string RequestBody 
        { 
            get => requestBody; 
            set { requestBody = value; OnPropertyChanged(nameof(RequestBody)); } 
        }

        public string ResponseHeaders 
        { 
            get => responseHeaders; 
            set { responseHeaders = value; OnPropertyChanged(nameof(ResponseHeaders)); } 
        }

        public string ResponseBody 
        { 
            get => responseBody; 
            set { responseBody = value; OnPropertyChanged(nameof(ResponseBody)); } 
        }
    }
}
