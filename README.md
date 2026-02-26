# NetworkWatcher

[![CI Build](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/ci-build.yml)
[![Release](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/KeyserDSoze/NetworkWatcher/actions/workflows/build-and-release.yml)
[![GitHub release](https://img.shields.io/github/v/release/KeyserDSoze/NetworkWatcher)](https://github.com/KeyserDSoze/NetworkWatcher/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/KeyserDSoze/NetworkWatcher/total)](https://github.com/KeyserDSoze/NetworkWatcher/releases)

NetworkWatcher is a **Visual Studio extension** and .NET console application that monitors HTTP/HTTPS network traffic with filtering, JSON prettification, and search capabilities.

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

Add the helper script or set PATH manually
-----------------------------------------

This repository includes a small PowerShell helper `install-path.ps1` to guide you through adding the build output folder (for example `bin\Debug\net10.0`) to your PATH. Two options are provided below.

1) Use the included script (recommended)
- Open PowerShell (you can run non-elevated to update the user PATH).
- From the repository root run:

  pwsh .\install-path.ps1

  or (if your system associates .ps1 files with PowerShell):

  .\install-path.ps1

- The script will:
  - suggest typical build folders (`bin\Debug\net10.0` and `bin\Release\net10.0`),
  - optionally create the folder if it does not exist,
  - let you choose to add the folder to the User PATH or the Machine PATH (the latter requires Administrator).

2) Manual method (quick example)
- Build the project first (e.g. `dotnet build`).
- Find the folder that contains `NetworkWatcher.exe` (for example `C:\Users\<you>\source\repos\NetworkWatcher\bin\Debug\net10.0`).
- Add it to your user PATH from PowerShell (no elevation required):

  $folder = 'C:\Users\<you>\source\repos\NetworkWatcher\bin\Debug\net10.0'
  $old = [Environment]::GetEnvironmentVariable('Path','User')
  [Environment]::SetEnvironmentVariable('Path', "$old;$folder", 'User')

- Or set it with `setx` from an elevated prompt for system-wide PATH (careful with length limits):

  setx /M PATH "%PATH%;C:\Users\<you>\source\repos\NetworkWatcher\bin\Debug\net10.0"

Example end-to-end
- Build the app:

  dotnet build

- Add build folder to PATH using the script or the manual commands above.
- Open a new elevated terminal and run (example):

  NetworkWatcher --domain example.com -p

  - `-p` enables JSON prettify/colorization; omit it to be asked interactively.

Notes
- After changing PATH open a new terminal to pick up the change.
- Running NetworkWatcher to create/trust a local root certificate and to set the system proxy typically requires Administrator privileges on Windows.

Optional: add the built executable to your PATH (Windows)
If you want to run `NetworkWatcher` from any command prompt without specifying the full path, you can add the build output folder to your system `PATH`. Steps:

- Build the project (e.g. `dotnet build` or build in Visual Studio).
- Locate the build output folder. By default Visual Studio places the executable in your project folder under `bin\Debug\<TFM>\` (for example `bin\Debug\net10.0\`).
- GUI method:
  - Open Start → type "Edit the system environment variables" → Environment Variables → select `Path` under your user or system variables → Edit → New → paste the folder path → OK.
  - Open a new Command Prompt or PowerShell window and run `NetworkWatcher.exe -p` (or `NetworkWatcher -p`).
- PowerShell method (example):
  - Run in an elevated PowerShell if you want to set system-level PATH, or in a normal PowerShell to set the user PATH:
    - setx PATH "$Env:PATH;C:\full\path\to\NetworkWatcher\bin\Debug\net10.0"
  - Close and reopen your terminal for the change to take effect.
- Alternative: copy `NetworkWatcher.exe` to a folder already in your PATH (for example `C:\Tools`) so you can run `NetworkWatcher -p` directly.

Note: when running the tool from the command line you may need to start the shell as Administrator so the application can create/trust the local root certificate and set the system proxy.

