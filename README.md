# NetworkWatcher

NetworkWatcher is a small .NET console application that runs an explicit HTTP/HTTPS proxy and logs HTTP requests and responses that match a configurable domain filter.

Key points
- Uses `Titanium.Web.Proxy` to capture HTTP and HTTPS traffic.
- Adds and trusts a local root certificate so HTTPS traffic can be decrypted (for inspection) — this requires administrator privileges.
- By default the proxy listens on port `8888` and sets the system proxy to `127.0.0.1:8888` while running.
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
- `-p`, `--prettify` : optional. When supplied the tool will automatically detect JSON request/response bodies, pretty-print them (indented) and show them with colorized syntax in the console output.
  - If `-p` / `--prettify` is not supplied the program will ask interactively on startup if you want to enable JSON prettify/colorization.

Examples
- Run and enable prettify from command line (watch all domains):

  dotnet run --project NetworkWatcher -- -p

- Run and monitor a specific domain with prettify enabled:

  dotnet run --project NetworkWatcher -- --domain example.com -p

Behavior
- The application creates/trusts a root certificate automatically. It starts an explicit proxy endpoint on port `8888` and sets the system proxy to route traffic through it.
- For each HTTP request/response that matches the domain filter, NetworkWatcher writes a human-readable entry to the log file and prints it to the console.
  - Requests are printed in cyan, responses in green.
  - When JSON prettify is enabled the tool will detect valid JSON bodies and:
    - Write the prettified JSON (indented) to the log file (plain text, no coloring).
    - Show the prettified JSON in the console with simple syntax colorization (keys, strings, numbers, booleans, null and punctuation colored) for readability.
- If prettify is disabled the bodies are logged and printed as received.
- Press ENTER or use CTRL+C to stop the proxy. On shutdown the system proxy is unset and the proxy is stopped.

Logs
- Logs are stored in the `logs` folder under the application directory.
- Each capture includes timestamps, request/response headers and bodies (when present). If prettify is enabled JSON bodies will be written to the log file in indented form (no console colors are written to the file).

Security & Privacy
- Because the tool intercepts and decrypts HTTPS traffic it will have access to the full contents of requests/responses. Use it only in environments where this is permitted and safe.
- Be cautious when trusting a locally generated root certificate; remove it when you no longer need the tool.

Troubleshooting
- If the application cannot create or trust the certificate, run it as Administrator.
- If traffic is not being captured, ensure your client is using the system proxy or is configured to use `127.0.0.1:8888` explicitly.

Dependencies
- `Titanium.Web.Proxy` package (referenced in the project file).

License
- MIT License (see LICENSE.md for details).

