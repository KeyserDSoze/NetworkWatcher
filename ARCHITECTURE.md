# Network Watcher Extension - Architecture

## Component Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Visual Studio IDE                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │         Network Watcher Extension Package             │  │
│  │                                                         │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │    NetworkWatcherExtensionPackage              │  │  │
│  │  │    - Package registration                       │  │  │
│  │  │    - Command initialization                     │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  │                         │                              │  │
│  │                         ▼                              │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │    NetworkWatcherToolWindowCommand             │  │  │
│  │  │    - Menu command handler                       │  │  │
│  │  │    - Opens tool window                          │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  │                         │                              │  │
│  │                         ▼                              │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │        NetworkWatcherToolWindow                │  │  │
│  │  │        - Tool window host                       │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  │                         │                              │  │
│  │                         ▼                              │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │   NetworkWatcherToolWindowControl (WPF UI)     │  │  │
│  │  │                                                 │  │  │
│  │  │   ┌─────────────────────────────────────────┐  │  │  │
│  │  │   │  Toolbar                                │  │  │  │
│  │  │   │  [Start] [Stop] [Clear] Status          │  │  │  │
│  │  │   └─────────────────────────────────────────┘  │  │  │
│  │  │                                                 │  │  │
│  │  │   ┌─────────────────────────────────────────┐  │  │  │
│  │  │   │  Filters                                │  │  │  │
│  │  │   │  Domain: [________] Protocol: [▼]      │  │  │  │
│  │  │   └─────────────────────────────────────────┘  │  │  │
│  │  │                                                 │  │  │
│  │  │   ┌─────────────────────────────────────────┐  │  │  │
│  │  │   │  Request List (ListView)                │  │  │  │
│  │  │   │  Time | Method | Protocol | URL | Status│  │  │  │
│  │  │   │  ─────┼────────┼──────────┼─────┼────── │  │  │  │
│  │  │   │  12:34│ GET    │ HTTPS    │ ... │ 200   │  │  │  │
│  │  │   │  12:35│ POST   │ HTTPS    │ ... │ 201   │  │  │  │
│  │  │   └─────────────────────────────────────────┘  │  │  │
│  │  │                                                 │  │  │
│  │  │   ┌─────────────────────────────────────────┐  │  │  │
│  │  │   │  Details Tabs                           │  │  │  │
│  │  │   │  [Req Headers][Req Body][Res Headers]  │  │  │  │
│  │  │   │  [Response Body]                        │  │  │  │
│  │  │   │  ┌───────────────────────────────────┐  │  │  │  │
│  │  │   │  │ {                                 │  │  │  │  │
│  │  │   │  │   "id": 1,                        │  │  │  │  │
│  │  │   │  │   "name": "Test",                 │  │  │  │  │
│  │  │   │  │   "data": {...}                   │  │  │  │  │
│  │  │   │  │ }                                 │  │  │  │  │
│  │  │   │  └───────────────────────────────────┘  │  │  │  │
│  │  │   └─────────────────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Proxy Server (Port 8888)
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  Titanium.Web.Proxy                          │
│                                                               │
│  ┌──────────────┐     ┌──────────────┐    ┌──────────────┐ │
│  │ Certificate  │     │   Proxy      │    │   Events     │ │
│  │  Manager     │────▶│   Server     │───▶│  - OnRequest │ │
│  │              │     │              │    │  - OnResponse│ │
│  └──────────────┘     └──────────────┘    └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/HTTPS Traffic
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Your Application                          │
│                                                               │
│  HttpClient with Proxy: 127.0.0.1:8888                      │
│                                                               │
│  ┌──────────────┐                         ┌──────────────┐  │
│  │ HTTP Client  │────────────────────────▶│ API Endpoint │  │
│  │              │  All traffic goes       │              │  │
│  │              │  through proxy          │              │  │
│  └──────────────┘◀────────────────────────└──────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

```
1. User clicks "Start Proxy"
   └─▶ Initialize Titanium.Web.Proxy
       └─▶ Create root certificate
           └─▶ Trust certificate
               └─▶ Start listening on port 8888

2. Application makes HTTP/HTTPS request
   └─▶ Request configured to use proxy 127.0.0.1:8888
       └─▶ Proxy intercepts request
           └─▶ OnRequest event fired
               └─▶ Create HttpTrafficEntry
                   └─▶ Add to allEntries collection
                       └─▶ Apply filters
                           └─▶ Update UI (filteredEntries)

3. Proxy forwards request to actual server
   └─▶ Server responds
       └─▶ Proxy intercepts response
           └─▶ OnResponse event fired
               └─▶ Update existing HttpTrafficEntry
                   └─▶ Add response data
                       └─▶ Apply filters
                           └─▶ Update UI

4. User clicks on request in list
   └─▶ SelectionChanged event
       └─▶ Load entry details into tabs
           └─▶ Display headers
               └─▶ Display body (with JSON prettification)

5. User types in filter box
   └─▶ FilterChanged event
       └─▶ ApplyFilters()
           └─▶ Clear filteredEntries
               └─▶ Loop through allEntries
                   └─▶ Check domain/URL match
                       └─▶ Check protocol match
                           └─▶ Add to filteredEntries if match
```

## JSON Prettification Flow

```
OnRequest/OnResponse
   │
   ├─▶ Check Content-Type header
   │    │
   │    └─▶ Contains "json"?
   │         │
   │         ├─▶ YES: TryFormatJson()
   │         │    │
   │         │    └─▶ JsonDocument.Parse()
   │         │         │
   │         │         ├─▶ Success: JsonSerializer.Serialize()
   │         │         │             with WriteIndented = true
   │         │         │
   │         │         └─▶ Fail: Return original content
   │         │
   │         └─▶ NO: Return content as-is
   │
   └─▶ Store prettified content in HttpTrafficEntry
```

## Key Components

### NetworkWatcherExtensionPackage
- Entry point for the extension
- Registers the tool window
- Initializes commands

### NetworkWatcherToolWindow
- Hosts the WPF control
- Provides the tool window frame

### NetworkWatcherToolWindowControl
- Main UI implementation
- Manages proxy lifecycle
- Handles filtering
- Displays traffic data
- Formats JSON

### HttpTrafficEntry
- Data model for captured traffic
- Properties:
  - Time, Method, Protocol, URL, Status
  - RequestHeaders, RequestBody
  - ResponseHeaders, ResponseBody

## Threading Model

```
UI Thread (Main Thread)
   │
   ├─▶ Button clicks (Start/Stop/Clear)
   │   └─▶ SwitchToMainThreadAsync()
   │
   ├─▶ Filter changes
   │   └─▶ ApplyFilters() on UI thread
   │
   └─▶ ListView selection changes
       └─▶ Update detail tabs on UI thread

Background Thread (Proxy Events)
   │
   ├─▶ OnRequest event
   │   ├─▶ Process request data
   │   └─▶ SwitchToMainThreadAsync()
   │       └─▶ Update UI collections
   │
   └─▶ OnResponse event
       ├─▶ Process response data
       └─▶ SwitchToMainThreadAsync()
           └─▶ Update UI collections
```

## Extension Points

To add new features:

1. **Add new filter type**:
   - Add UI control in XAML
   - Update `ApplyFilters()` method
   - Add filter logic

2. **Add new detail view**:
   - Add TabItem in XAML
   - Add TextBox for content
   - Update `RequestListView_SelectionChanged()`

3. **Add export functionality**:
   - Add Export button
   - Serialize allEntries to JSON/CSV
   - Save to file

4. **Add request modification**:
   - Hook into proxy BeforeRequest
   - Modify headers/body before forwarding
   - Add UI for modification rules

5. **Add statistics view**:
   - Aggregate data from allEntries
   - Show request count, timing, status codes
   - Add charts/graphs
