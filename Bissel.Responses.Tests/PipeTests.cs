using Bissel.Responses;
using Bissel.Responses.Builder;
using Bissel.Responses.Messages;

namespace Bissel.Responses.Tests;

internal class TestException : Exception;

internal interface ITestHelper
{
    public static void FailAction(IBuilder builder) 
    {
        builder.MarkAsFailed();
    }
    
    public static void ThrowAction<T>(IBuilder _)
        where T : Exception, new()
    {
        throw new T();
    }
    
    public static void ThrowAction(IBuilder builder) => throw new TestException();
}

public class PipeTests
{
    
    [Fact]
    public async Task Pipe_Empty_Success()
    {
        var result = await Pipe.Create().InvokeAsync();
        Assert.True(result.IsSuccess);
    }   
    
    [Fact]
    public async Task Pipe_ThenWithoutErrors_Success()
    {
        var result = await Pipe.Create().Then(_ => { }).InvokeAsync();
        Assert.True(result.IsSuccess);
    }   
    
    [Fact]
    public async Task Pipe_ThenWithErrors_Failure()
    {
        var result = await Pipe.Create().Then(ITestHelper.FailAction).InvokeAsync();
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task Pipe_ThenWithException_Failure()
    {
        string? a = null;
        var result = await Pipe.Create().Then(ITestHelper.ThrowAction).InvokeAsync();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<TestException>(eMessage.Exception);
    }

    [Fact]
    public async Task Pipe_ThenOptional_Success()
    {
        var result = await Pipe.Create().Then(_ => { }, Pipe.Behavior.SkipOnError).InvokeAsync();
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task Pipe_ThenOptional_Error()
    {
        var result = await Pipe.Create()
            .Then(ITestHelper.FailAction)
            .Then(_ => { Assert.Fail(); }, Pipe.Behavior.SkipOnError)
            .InvokeAsync();
        
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task PipeT_ThenWithoutErrors_Success()
    {
        var result = await Pipe.Create<string>()
            .Then(builder => { builder.SetResult("");})
            .InvokeAsync();
        
        Assert.True(result.IsSuccess);
    }  
    
    [Fact]
    public async Task PipeT_ThenOperatorWithoutErrors_Success()
    {
        const string value = "a";
        Task<Response<string>> task =
            Pipe.Create<string>()
            | (b => { b.SetResult(""); })
            | (b => { b.SetResult(value); });
        
        var result = await task;
        
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetData(out var data));
        Assert.Equal(value, data);
    }  
    
    [Fact]
    public async Task PipeT_ThenWithErrors_Failure()
    {
        var result = await Pipe.Create<string>().Then(ITestHelper.FailAction).InvokeAsync();
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task PipeT_ThenWithException_Failure()
    {
        var result = await Pipe.Create<string>().Then(ITestHelper.ThrowAction).InvokeAsync();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<TestException>(eMessage.Exception);
    }
    
    [Fact]
    public async Task PipeManyT_ThenWithoutErrors_Success()
    {
        Task<ResponseMany<string>> task = Pipe.CreateMany<string>().Then(_ => {  });
        var result = await task;
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ResponseMessages);
        Assert.True(result.TryGetData(out var data));
        Assert.Empty(data);
    }  
    
    [Fact]
    public async Task PipeManyT_ThenWithDataWithoutErrors_Success()
    {
        const string value = "a";
        Task<ResponseMany<string>> task = Pipe
            .CreateMany<string>()
            .Then(builder => { builder.AddResults(value); });
        
        var result = await task;
        
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ResponseMessages);
        Assert.True(result.TryGetData(out var data));
        var actualValue = Assert.Single(data);
        Assert.Equal(value, actualValue);
    }  
    
    [Fact]
    public async Task PipeManyT_ThenWithErrors_Failure()
    {
        Task<ResponseMany<string>> task = Pipe.CreateMany<string>().Then(ITestHelper.FailAction);
        
        var result = await task;
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task PipeManyT_ThenWithException_Failure()
    {
        var result = await Pipe.CreateMany<string>().Then(ITestHelper.ThrowAction).InvokeAsync();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<TestException>(eMessage.Exception);
    }

    [Fact]
    public void Pipe_Continue_Success()
    {
        const string errorId = "D13E7E1C-E53F-434A-BAE3-D154E784BDCE";
        
        var pipe = Pipe.Create<string>()
            .Catch<TestException>(errorId, "Test");

        var secondPipe = pipe.Continue().Then(ITestHelper.ThrowAction).Invoke();
        
        Assert.False(secondPipe.IsSuccess);
        var message = Assert.Single(secondPipe.ResponseMessages);
        Assert.Equal(errorId, message.SourceId);
        
        var thirdPipe = pipe.Continue().Then(builder => builder.SetResult("a")).Invoke();
        
        Assert.True(thirdPipe.IsSuccess);

    }
}