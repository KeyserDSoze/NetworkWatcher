# NetworkWatcher

NetworkWatcher is a small .NET console application that runs an explicit HTTP/HTTPS proxy and logs HTTP requests and responses that match a configurable domain filter.

Key points
- Uses Titanium.Web.Proxy to capture HTTP and HTTPS traffic.
- Adds and trusts a local root certificate so HTTPS traffic can be decrypted (for inspection) — this requires administrator privileges.
- By default the proxy listens on port 8888 and sets the system proxy to `127.0.0.1:8888` while running.
- Logs are written to the `logs` directory with names like `traffic_YYYY-MM-DD_HH-mm-ss.log`.

Requirements
- .NET 10 SDK
- Administrator privileges to install/trust root certificate and to set the system proxy (Windows).

Build
- From the project folder run:

  dotnet build

Run
- Run from your IDE or with `dotnet run --project NetworkWatcher`.

Command-line
- `--domain <domain>` : optional. When provided, NetworkWatcher only logs requests/responses whose URL contains `<domain>` (case-insensitive).
- If no `--domain` is supplied, the application will prompt for a domain. Leave empty to monitor all traffic.

Behavior
- The application creates/trusts a root certificate automatically. It starts an explicit proxy endpoint on port 8888 and sets the system proxy to route traffic through it.
- For each HTTP request/response that matches the domain filter, NetworkWatcher writes a human-readable entry to the log file and prints it to the console (requests in cyan, responses in green).
- Press ENTER or use CTRL+C to stop the proxy. On shutdown the system proxy is unset and the proxy is stopped.

Logs
- Logs are stored in the `logs` folder under the application directory.
- Each capture includes timestamps, request/response headers and bodies (when present).

Security & Privacy
- Because the tool intercepts and decrypts HTTPS traffic it will have access to the full contents of requests/responses. Use it only in environments where this is permitted and safe.
- Be cautious when trusting a locally generated root certificate; remove it when you no longer need the tool.

Troubleshooting
- If the application cannot create or trust the certificate, run it as Administrator.
- If traffic is not being captured, ensure your client is using the system proxy or is configured to use `127.0.0.1:8888` explicitly.

Dependencies
- `Titanium.Web.Proxy` package (referenced in the project file).

License
- This repository does not include a license file. Add one if you plan to publish the project.

