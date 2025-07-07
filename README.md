# Bissel.Response
Is a small Package for Response-Handling with Error and Exception handling and pipeing/chaining of parts in an Service.

Example
```cs
record Entity(string Name);

interface IMyService {
  Response Delete(Guid id);
  Task<ResponseWith<Entity>> Create(string name);
  ResponseWithMany<Entity> Search(string name);
  ResponseWith<string> ThrowOnNull(string value);
}

class MyService(IStore store) : IMyService {
  public Response Delete(Guid id) {
    var resp = R.Create();

    if(store.DeleteEntity(id))
      resp.AddMessage(new ResponseMessage("deleteSuccess", $"Successfully deleted {nameof(Entity)} with Id: \"{id}\"."));
    else
      resp.AddMessage(new ErrorResponseMessage("deleteError", $"Error while deleting {nameof(Entity)} with Id: \"{id}\"."));

    return resp;
  }

  public Task<ResponseWith<Entity>> Create(string name) =>
    R.Pipe<Entity>()
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

  public ResponseWithMany<Entity> Search(string name) =>
    R.PipeMany<Entity>()
      .Then(builder => builder.AddResults(store.Query(e => e.Name == name)))
      .Do();

  public ResponseWith<string> ThrowOnNull(string value) =>
    R.Pipe<string>()
      .Then(bulder => {
        ArgumentNullException.ThrowIfNull(value);
        builder.SetValue(value);
      })
      .Do();
}
```
