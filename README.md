# Bissel.Response
Is a small Package for Response-Handling with Error and Exception handling and pipeing/chaining of parts in an Service.

## Creating a Response
Create a plain Response(-builder)
```cs
R.Create();
R.Create<T>();
R.CreateMany<T>();
```

Create a Pipe
```cs
R.Pipe(). ...;
R.Pipe<T>(). ...;
R.PipeMany<T>(). ...;
```

## Using Pipes 

### Then-Syntax
```cs
Pipe.Create<string>() 
 .Then(DoCheck)
 .Then(builder => { builder.SetValue(value); })
 .Do();
    
DoCheck(ResponseBuilderBase builder) => { if(value.Length < 3) builder.AddMessage(new ErrorResponseMessage ...); }
```

### Pipe-Syntax
```cs
Pipe.Create<string>()
 | DoCheck
 | (builder => { builder.SetValue(value); });
    
DoCheck(ResponseBuilderBase builder) => { if(value.Length < 3) builder.AddMessage(new ErrorResponseMessage ...); }
```


## Examples
```cs
record Entity(string Name);

interface IMyService {
  Response Delete(Guid id);
  Task<Response<Entity>> Create(string name);
  ResponseMany<Entity> Search(string name);
  Response<string> ThrowOnNull(string value);
}

class MyService(IStore store) : IMyService {
  public Response Delete(Guid id) {
    var resp = new ResponseBuilder();

    if(store.DeleteEntity(id))
      resp.AddMessage(new ResponseMessage("deleteSuccess", $"Successfully deleted {nameof(Entity)} with Id: \"{id}\"."));
    else
      resp.AddMessage(new ErrorResponseMessage("deleteError", $"Error while deleting {nameof(Entity)} with Id: \"{id}\"."));

    return resp;
  }

  public Task<Response<Entity>> Create(string name) =>
    Pipe.Create<Entity>()
      .Then(builder => {
        if(string.IsNullOrWhitespace(name))
          builder.AddMessage(new ErrorResponseMessage("createNull", $"When creating an {nameof(Entity)} the \"name\" must be set."));
      }, continueBehavior: PipeContinueBehavior.IgnoreError)
      .Then(builder => {
        if(name?.Length > 2)
          builder.AddMessage(new ErrorResponseMessage("createToShort", $"When creating an {nameof(Entity)} the \"name\" must be at least 3 characters long."));
      }, previousBehavior: PipePreviousBehavior.IgnoreError)
      .Then(async builder => {
        var entity = await store.CreateEntityAsync(name);
        builder.SetResult(entity);
      })
      .Catch("createException", $"Error while creating a {nameof(Entity)}.")
      .Do();

  public ResponseMany<Entity> Search(string name) =>
    Pipe.CreateMany<Entity>()
      .Then(builder => builder.AddResults(store.Query(e => e.Name == name)));

  public Response<string> ThrowOnNull(string value) =>
    Pipe.Create<string>()
      .Then(bulder => {
        ArgumentNullException.ThrowIfNull(value);
        builder.SetValue(value);
      });
}
```
