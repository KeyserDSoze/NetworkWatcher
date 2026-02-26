# Network Watcher - Visual Studio Extension

A Visual Studio extension that provides real-time HTTP/HTTPS network traffic monitoring during debugging sessions.

## Features

- **Real-time Traffic Capture**: Monitors HTTP and HTTPS requests and responses through a local proxy server
- **Domain/URL Filtering**: Filter captured traffic by domain or URL pattern
- **Protocol Filtering**: Filter by HTTP or HTTPS protocol
- **Request/Response Details**: View complete headers and body content for each request
- **JSON Prettification**: Automatically formats and indents JSON content in both requests and responses
- **Integrated Tool Window**: Seamlessly integrated into Visual Studio's window system

## How to Use

1. Open the Network Watcher tool window from **View → Other Windows → Network Watcher**
2. Click **Start Proxy** to begin capturing traffic
3. Configure your application to use proxy `127.0.0.1:8888` or set it as the system proxy
4. Use the filter boxes to narrow down traffic:
   - **Domain/URL**: Enter a domain name or URL pattern (case-insensitive)
   - **Protocol**: Select HTTP, HTTPS, or All
5. Click on any request in the list to view detailed information in the tabs below
6. Click **Stop Proxy** when finished

## Requirements

- Visual Studio 2022 or later
- .NET Framework 4.7.2 or later
- Administrator privileges (required for certificate installation on first run)

## Tool Window Layout

The Network Watcher window consists of:

- **Toolbar**: Start/Stop proxy controls and status indicator
- **Filter Bar**: Domain/URL and Protocol filters
- **Request List**: Shows all captured requests with timestamp, method, protocol, URL, and status
- **Details Tabs**: 
  - Request Headers
  - Request Body (with JSON prettification)
  - Response Headers
  - Response Body (with JSON prettification)

## Security Note

This extension creates and trusts a local root certificate to decrypt HTTPS traffic for inspection. This is necessary to view HTTPS request/response content. The certificate is only used locally and only while the proxy is active.

Use this extension only in development environments and remove the certificate when no longer needed.

## Technical Details

- **Proxy Port**: 8888 (localhost only)
- **Certificate**: Auto-generated and trusted on first use
- **Protocol Support**: HTTP/1.0, HTTP/1.1, HTTP/2
- **JSON Detection**: Based on Content-Type header

## Troubleshooting

### Proxy won't start
- Ensure port 8888 is not already in use
- Run Visual Studio as Administrator for certificate installation

### No traffic captured
- Verify your application is configured to use proxy `127.0.0.1:8888`
- Check that the Status shows "Running on port 8888"

### HTTPS errors in application
- The root certificate may not be trusted
- Stop and restart the proxy to regenerate the certificate

## Building from Source

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Build the solution
5. Press F5 to launch the experimental instance of Visual Studio

## Dependencies

- **Titanium.Web.Proxy**: HTTP/HTTPS proxy server implementation
- **Microsoft.VisualStudio.SDK**: Visual Studio extensibility framework

## License

MIT License - See LICENSE file for details
