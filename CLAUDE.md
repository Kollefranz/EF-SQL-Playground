# TheAPI

## Code Style
- All async controller actions take a `CancellationToken ct` parameter and pass it through to every async call.
- Always use `using` statements with braces, unless the disposable resource is used for the entire method (in which case the braceless declaration form is fine).
- Always use braces for control flow statements and method definitions, except lambda expressions.
- Use records with required properties for DTOs and entities.