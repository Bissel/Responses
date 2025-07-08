using Bissel.Response;
using Bissel.Response.Messages;
using Xunit.Sdk;

namespace Bissel.Responses.Tests;

public class ResponsePipeTests
{
    [Fact]
    public async Task Pipe_ThenWithoutErrors_Success()
    {
        var result = await R.Pipe().Then(_ => { }).Do();
        Assert.True(result.IsSuccess);
    }   
    
    [Fact]
    public async Task Pipe_ThenWithErrors_Failure()
    {
        var result = await R.Pipe().Then(b => { b.MarkAsFailed(); }).Do();
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task Pipe_ThenWithException_Failure()
    {
        string? a = null;
        var result = await R.Pipe().Then(_ => { ArgumentNullException.ThrowIfNull(a); }).Do();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<ArgumentNullException>(eMessage.Exception);
    } 
    
    [Fact]
    public async Task PipeT_ThenWithoutErrors_Success()
    {
        var result = await R.Pipe<string>().Then(b => { b.SetResult("");}).Do();
        Assert.True(result.IsSuccess);
    }  
    
    [Fact]
    public async Task PipeT_ThenOperatorWithoutErrors_Success()
    {
        const string value = "a";
        var task =
            (R.Pipe<string>()
             | (b => { b.SetResult(""); })
             | (b => { b.SetResult(value); }))
            .Do();
        
        var result = await task;
        
        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetData(out var data));
        Assert.Equal(value, data);
    }  
    
    [Fact]
    public async Task PipeT_ThenWithErrors_Failure()
    {
        var result = await R.Pipe<string>().Then(b => { b.MarkAsFailed(); }).Do();
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task PipeT_ThenWithException_Failure()
    {
        string? a = null;
        var result = await R.Pipe<string>().Then(_ => { ArgumentNullException.ThrowIfNull(a); }).Do();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<ArgumentNullException>(eMessage.Exception);
    }
    
    [Fact]
    public async Task PipeManyT_ThenWithoutErrors_Success()
    {
        var result = await R.PipeMany<string>().Then(_ => { }).Do();
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ResponseMessages);
        Assert.True(result.TryGetData(out var data));
        Assert.Empty(data);
    }  
    
    [Fact]
    public async Task PipeManyT_ThenWithDataWithoutErrors_Success()
    {
        const string value = "a";
        var result = await R.PipeMany<string>().Then(b => { b.AddResult(value); }).Do();
        Assert.True(result.IsSuccess);
        Assert.Empty(result.ResponseMessages);
        Assert.True(result.TryGetData(out var data));
        var actualValue = Assert.Single(data);
        Assert.Equal(value, actualValue);
    }  
    
    [Fact]
    public async Task PipeManyT_ThenWithErrors_Failure()
    {
        var result = await R.PipeMany<string>().Then(b => { b.MarkAsFailed(); }).Do();
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async Task PipeManyT_ThenWithException_Failure()
    {
        string? a = null;
        var result = await R.PipeMany<string>().Then(_ => { ArgumentNullException.ThrowIfNull(a); }).Do();
        Assert.False(result.IsSuccess);
        var message = Assert.Single(result.ResponseMessages);
        var eMessage = Assert.IsType<ExceptionResponseMessage>(message);
        Assert.IsType<ArgumentNullException>(eMessage.Exception);
    }
}