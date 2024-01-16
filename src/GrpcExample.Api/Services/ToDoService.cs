using Grpc.Core;
using GrpcExample.Api.Data;
using GrpcExample.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GrpcExample.Api.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _dbContext;

    public ToDoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ServerCallContext helps us manage the call (for example, cancel it)
    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(
                // It's like an analog of HTTP 400 response
                new Status(
                    StatusCode.InvalidArgument,
                    "You must supply a valid object"));
        }
        
        // In production-like environment better to use automapper-like tools
        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _dbContext.AddAsync(toDoItem);
        await _dbContext.SaveChangesAsync();

        return new CreateToDoResponse { Id = toDoItem.Id };
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater that 0"));
        }

        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if (toDoItem is not null)
        {
            // In production-like environment better to use automapper-like tools
            return new ReadToDoResponse
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            };
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }

    public override async Task<ListToDoResponse> ListToDo(ListToDoRequest request, ServerCallContext context)
    {
        var response = new ListToDoResponse();
        var toDoItems = await _dbContext.ToDoItems.ToArrayAsync();

        foreach (var toDo in toDoItems)
        {
            response.ToDo.Add(
                new ReadToDoResponse
                {
                    Id = toDo.Id,
                    Title = toDo.Title,
                    Description = toDo.Description,
                    ToDoStatus = toDo.ToDoStatus
                });
        }

        return response;
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));        }
        
        
        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if (toDoItem is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }

        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.ToDoStatus = request.ToDoStatus;

        await _dbContext.SaveChangesAsync();

        return new UpdateToDoResponse { Id = toDoItem.Id };
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater that 0"));
        }

        var toDoItem = await _dbContext.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        
        if (toDoItem is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }

        _dbContext.Remove(toDoItem);
        await _dbContext.SaveChangesAsync();

        return new DeleteToDoResponse { Id = toDoItem.Id };
    }
}
