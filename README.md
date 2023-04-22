# DTOGenerator

Automatically generated records from interfaces

### Example

```csharp
public interface IUser {
    public Guid Id { get; }
    public string FirstName { get; 
    public string LastName { get; }
}
```

Generated code

```csharp
public record User(Guid Id, string FirstName, string LastName);
```