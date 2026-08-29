# Context Diagram

```mermaid
flowchart LR
    User[Authenticated user] --> Web[Blazor web application]
    Web --> Api[Web API]
    Api --> App[Application commands and queries]
    App --> Governance[Governance enforcement]
    Governance --> Agent[Microsoft Agent Framework agent]
    Agent --> Tools[Scoped AI tools]
    Tools --> App
    App --> Sql[(SQL Server)]
    Tools --> External[Approved external providers]
```

The web application never accesses persistence directly. Tools are infrastructure adapters that invoke typed application requests. Governance is applied before each model inference.