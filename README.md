# DTOGenerator

Automatically generated records from interfaces

### Example

```csharp
public interface IUser {
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
}
```

Generated code (readonly struct because IUser just contains value-types)

```csharp
public readonly record struct User(Guid Id, string FirstName, string LastName);
```

### Todo

- Make it configurable https://github.com/IDjinn/DTO.Generator/pull/2
