# 02_Browser_Engine.md

**Document Status**

**Version:** 1.0
**Status:** Approved for Implementation

---

# 1. Purpose

The Browser Engine is the execution layer of the Browser Agent Framework.

Its responsibility is **browser automation only**.

It must never know:

* AI models
* Resume parsing
* Job applications
* User profiles
* Authentication
* PostgreSQL
* Business rules

The Browser Engine is designed to be reusable for **any browser workflow**.

Its only responsibilities are:

* Opening browsers
* Navigating websites
* Extracting structured information
* Executing user-like interactions
* Returning execution results

---

# 2. Design Principles

## Generic

The Browser Engine must never contain workflow-specific logic.

Example:

❌ Bad

```csharp
if(page.Url.Contains("greenhouse"))
```

Example:

```csharp
if(label == "Resume")
```

Neither should exist inside the Browser Engine.

---

## Stateless

The Browser Engine owns no persistent data.

Every workflow execution is driven by the .NET Orchestrator.

---

## Asynchronous

Every browser operation must be asynchronous.

Never block execution.

---

## Human-like Interaction

The Browser Engine should interact with pages the same way a user would.

Preferred order:

* Click visible elements
* Type into focused controls
* Select dropdown values
* Upload through file inputs

Avoid manipulating DOM through JavaScript unless absolutely necessary.

---

## Observable

Every significant action must produce an event.

Example:

```
BrowserStarted

PageLoaded

NavigationCompleted

FormExtracted

ElementClicked

TextEntered

ScreenshotCaptured

WorkflowPaused

WorkflowResumed
```

---

# 3. Responsibilities

The Browser Engine owns:

* Browser lifecycle
* Context lifecycle
* Page lifecycle
* Navigation
* DOM extraction
* Form extraction
* Screenshots
* Element interaction
* Waiting strategies
* Retry strategies
* Download handling
* Upload handling
* Session persistence

It does **not** own:

* AI reasoning
* Field mapping
* Resume understanding
* Workflow decisions
* User settings

---

# 4. Internal Architecture

```
Browser Engine

│

├── Browser Manager

├── Context Manager

├── Page Manager

├── Navigation Engine

├── DOM Extractor

├── Form Extractor

├── Element Locator

├── Interaction Engine

├── Upload Engine

├── Screenshot Service

├── Session Manager

├── Event Publisher

└── Error Recovery
```

Every module must be independently testable.

---

# 5. Folder Structure

```
apps/api

Modules/

Browser/

BrowserManager/

ContextManager/

Navigation/

Extraction/

Interaction/

Upload/

Screenshot/

Events/

Models/

Interfaces/

Exceptions/

Extensions/

Utilities/
```

No browser logic should exist outside this module.

---

# 6. Browser Lifecycle

Each workflow creates:

```
Browser

↓

Context

↓

Page

↓

Navigate

↓

Execute Workflow

↓

Dispose Resources
```

Contexts should be isolated.

Never reuse contexts between different users.

---

# 7. Browser Configuration

Support:

* Chromium
* Firefox (future)
* WebKit (future)

Initial implementation:

Chromium only.

Configuration should include:

* Headless mode
* Slow motion
* User agent
* Viewport
* Locale
* Timezone
* Download directory

No values should be hardcoded.

---

# 8. Session Management

Each workflow owns its own session.

The Browser Engine must support:

* Cookies
* Local Storage
* Session Storage

Future support:

* Session persistence
* Export session
* Import session

---

# 9. Navigation Engine

Responsibilities:

* Open URL
* Wait for navigation
* Wait for network idle
* Handle redirects
* Detect navigation failures

Must expose:

```
Navigate()

Back()

Forward()

Refresh()

Reload()
```

Every navigation should return a result object.

---

# 10. Waiting Strategy

Never use arbitrary sleeps like:

```
Thread.Sleep()

Task.Delay(5000)
```

Preferred order:

1. Element visible
2. Element enabled
3. Network idle
4. DOM stable
5. Custom wait conditions

---

# 11. Retry Strategy

Retry only recoverable failures.

Examples:

* Temporary network timeout
* Detached element
* Loading delay

Never retry:

* Invalid selectors
* Missing required elements
* Permission denied

Retries should use exponential backoff.

---

# 12. DOM Extraction

The Browser Engine must convert webpages into structured data.

Never send raw HTML directly to the AI.

Instead, create an intermediate representation.

Example:

```
Page

Sections

Forms

Buttons

Inputs

Links

Tables

Dialogs

Navigation
```

The extracted model should be serializable as JSON.

---

# 13. Form Extraction

Every form should become a structured model.

Each field must contain:

* Unique ID
* Label
* Placeholder
* Type
* Required
* Disabled
* Readonly
* Validation Rules
* Available Options
* Current Value

Supported field types:

* Text
* Email
* Password
* Phone
* Number
* TextArea
* Date
* File
* Checkbox
* Radio
* Dropdown
* Hidden

Unknown types should still be represented.

---

# 14. Button Extraction

Every actionable button should include:

* Text
* Type
* Visible
* Enabled
* Position
* Associated Form

Examples:

```
Apply

Continue

Submit

Next

Save

Login
```

---

# 15. Element Identification

Never rely on a single selector.

Priority:

1. Accessible Name
2. Label Association
3. Role
4. Name
5. ID
6. Placeholder
7. CSS Selector
8. XPath (last resort)

Accessibility-first improves robustness.

---

# 16. Interaction Engine

Supported actions:

```
Click

Double Click

Right Click

Hover

Focus

Blur

Type

Clear

Select

Check

Uncheck

Upload File

Scroll Into View
```

Every interaction returns success/failure.

---

# 17. File Upload

Requirements:

* Single file
* Multiple files
* Drag & Drop (future)

Validation:

* File exists
* File size
* Supported type

The Browser Engine only uploads paths provided by the Orchestrator.

---

# 18. Screenshot Service

Capture screenshots:

* Before navigation
* After navigation
* Before form filling
* After form filling
* Before pause
* Before workflow completion
* On every error

Future:

* Video recording
* DOM snapshots

---

# 19. Dialog Handling

Support:

* Alert
* Confirm
* Prompt

Default behavior should be configurable.

---

# 20. Popup Windows

Support:

* OAuth popups
* Login popups
* New tabs
* Multiple tabs

The Browser Engine should track every open page.

---

# 21. Download Handling

Future-proof architecture.

Support:

* Resume downloads
* PDF downloads
* ZIP downloads

Track:

* Filename
* MIME type
* Size
* Path

---

# 22. Error Recovery

Recoverable:

* Detached element
* Timeout
* Navigation retry

Non-recoverable:

* Browser crash
* Invalid workflow
* Missing page

Return structured errors.

Never throw generic exceptions.

---

# 23. Logging

Every action must be logged.

Example:

```
Browser Started

URL Navigated

Apply Button Clicked

Form Extracted

Input Filled

Checkbox Checked

Screenshot Saved

Workflow Paused
```

Sensitive values must be masked.

Never log:

* Passwords
* API Keys
* Resume contents
* Personal information

---

# 24. Events

Publish events for:

```
BrowserStarted

BrowserClosed

NavigationStarted

NavigationCompleted

PageLoaded

FormDetected

FormCompleted

ScreenshotTaken

HumanVerificationRequired

WorkflowPaused

WorkflowResumed

ErrorOccurred
```

Future plugins can subscribe.

---

# 25. Browser State

The Browser Engine must expose:

Current URL

Current Title

Current DOM Snapshot

Current Screenshot

Current Form

Current Step

Open Tabs

Cookies

Storage

The Orchestrator decides what to do.

---

# 26. Human Verification Detection

The Browser Engine should detect situations requiring human intervention.

Examples include:

* CAPTCHA widgets
* Email verification pages
* SMS verification prompts
* Security challenge screens

The Browser Engine must **detect and report** these conditions.

It must **not** attempt to bypass, solve, or automate security verification mechanisms.

Instead, it should:

* Capture the current state
* Notify the Orchestrator
* Pause execution until instructed to resume

---

# 27. Testing Requirements

Unit Tests

* Navigation
* DOM extraction
* Form extraction
* Element detection
* Interaction methods

Integration Tests

* Local HTML forms
* Multi-page forms
* File uploads
* Dialogs

End-to-End Tests

* Browser startup
* Workflow execution
* Screenshot generation
* Session handling

Target coverage:

Minimum **80%**.

---

# 28. Performance Goals

Browser startup:

< 3 seconds

Navigation:

Dependent on website

DOM extraction:

< 500 ms (average page)

Form extraction:

< 200 ms

Memory leaks:

Zero tolerated

Browser cleanup:

Always guaranteed

---

# 29. Out of Scope

The Browser Engine must not implement:

* AI reasoning
* Resume parsing
* Question answering
* Provider selection
* Authentication
* Database access
* Plugin discovery
* Business workflows
* Job application logic

Those belong to other components.

---

# 30. Acceptance Criteria

The Browser Engine is considered complete when it can:

* Launch a browser instance
* Navigate to any URL
* Extract page structure into structured JSON
* Detect forms and interactive elements
* Perform reliable user interactions
* Upload files
* Handle multi-step navigation
* Detect human verification checkpoints
* Capture screenshots throughout execution
* Publish lifecycle events
* Cleanly dispose browser resources
* Operate without knowledge of any specific website or workflow

---

# 31. Final Statement

The Browser Engine is the execution backbone of the Browser Agent Framework. It is intentionally generic, reusable, and independent of AI or business logic. Every future plugin—including Job Applications, Government Forms, CRM Automation, and Enterprise Workflows—must rely on this engine without requiring modifications to its core.

---

**End of Document — 02_Browser_Engine.md**
