using QimErp.Shared.Common.Services.Workflow;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for mapping workflow properties between entities and DTOs
/// </summary>
public static class MappingExtensions
{

    public static TResponse MapWorkflowProperties<TResponse>(this TResponse response, IWorkflowEnabled entity, bool includeWorkflow = false)
        where TResponse : WorkflowEnabledResponse
    {
        // Workflow properties have been removed from WorkflowEnabledResponse to reduce response size
        // This method is kept for backward compatibility but does nothing
        return response;
    }

    /// <summary>
    /// Maps workflow properties from IWorkflowEnabled entity to WorkflowEnabledResponse DTO with fluent syntax
    /// </summary>
    public static TResponse WithWorkflow<TResponse>(this TResponse response, IWorkflowEnabled entity, bool includeWorkflow = false)
        where TResponse : WorkflowEnabledResponse
    {
        return response.MapWorkflowProperties(entity, includeWorkflow);
    }
    

}



public abstract class ResponseBuilder<TEntity, TResponse> 
    where TEntity : AuditableEntity
    where TResponse : WorkflowEnabledResponse, new()
{
    protected TEntity _entity;
    protected TResponse _response;

    protected ResponseBuilder(TEntity entity)
    {
        _entity = entity;
        _response = new TResponse();
    }

    public ResponseBuilder<TEntity, TResponse> WithWorkflow(bool includeWorkflow = false)
    {
        if (_entity is IWorkflowEnabled workflowEntity)
        {
            _response.MapWorkflowProperties(workflowEntity, includeWorkflow);
        }
        return this;
    }

    public virtual TResponse Create()
    {
        _response.Status = _entity.DataStatus.ToString();
        _response.CreatedByUserId = _entity.CreatedByUserId;
        _response.CreatedByEmail = _entity.CreatedByEmail;
        _response.CreatedByName = _entity.CreatedByName;
        _response.Created = _entity.Created;
        _response.LastModifiedByUserId = _entity.LastModifiedByUserId;
        _response.LastModifiedByEmail = _entity.LastModifiedByEmail;
        _response.LastModifiedByName = _entity.LastModifiedByName;
        _response.LastModified = _entity.LastModified;
        _response.CreatedAt = _entity.CreatedDate;
        _response.UpdatedAt = _entity.LastModifiedDate;

        return _response;
    }
}