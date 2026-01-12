namespace QimErp.Shared.Common.Entities;

public abstract class LongAuditableEntity : BaseAuditableEntity<long>
{
}

public abstract class BaseAuditableEntity<T> : AuditableEntity where T : struct
{
    public T Id { get; set; }
}

public abstract class WorkflowBaseAuditableEntity<T> : WorkflowEnabledEntity where T : struct
{
    public T Id { get; set; }

    public void WithId(T id)
    {
        Id = id;
    }
}



public abstract class AuditableEntity
{
    // Domain Events Collection
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events for this entity (read-only)
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public string ReferenceNumber { get; set; } = string.Empty;

    // Tenant Information
    public string TenantId { get; set; } = string.Empty;

    // Created Information
    public string CreatedByUserId { get; private set; } = string.Empty;
    public string CreatedByEmail { get; private set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime Created { get; private set; }

    // Last Modified Information
    public string LastModifiedByUserId { get; private set; } = string.Empty;
    public string LastModifiedByEmail { get; private set; } = string.Empty;
    public string LastModifiedByName { get; private set; } = string.Empty;
    public DateTime? LastModified { get; private set; }

    // Data Status Information
    public DataState? DataStatus { get; private set; }
    public DataState? PreviousDataStatus { get; private set; } // Tracks previous state
    public Dictionary<string, string>? CustomFields { get; set; }
    public DateTime? EntityStatusCreated { get; private set; }
    public string EntityStatusCreatedByUserId { get; private set; } = string.Empty;
    public string EntityStatusCreatedByEmail { get; private set; } = string.Empty;
    public string EntityStatusCreatedByName { get; private set; } = string.Empty;

    public DateTime? EntityStatusLastModified { get; private set; }
    public string EntityStatusLastModifiedByUserId { get; private set; } = string.Empty;
    public string EntityStatusLastModifiedByEmail { get; private set; } = string.Empty;
    public string EntityStatusLastModifiedByName { get; private set; } = string.Empty;

    public string? EntityStatusName => DataStatus?.ToString();

    // Computed Properties
    public bool IsNewEntry => CreatedByUserId.IsEmpty();
    public bool IsEditMode => !IsNewEntry;

    // Other Properties
    public string OtherProperty { get; private set; } = string.Empty;
    public string OtherProperty1 { get; private set; } = string.Empty;
    public bool IsGlobal { get; private set; }
    public string CreatedDate => Created.Humanize();
    public string LastModifiedDate => LastModified?.Humanize() ?? string.Empty;
    public string EntityStatusCreatedDate => EntityStatusCreated?.Humanize() ?? string.Empty;
    public string EntityStatusLastModifiedDate => EntityStatusLastModified?.Humanize() ?? string.Empty;

    /// <summary>
    /// Add a domain event to be published when the entity is saved
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove a specific domain event
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
    public IEnumerable<IDomainEvent> PullDomainEvents()
    {
        var items = _domainEvents.ToArray();
        _domainEvents.Clear();
        return items;
    }

    /// <summary>
    /// Clear all domain events (typically called after publishing)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Check if entity has pending domain events
    /// </summary>
    public bool HasDomainEvents => _domainEvents.Count > 0;

    public AuditableEntity OnCreate(string userId, string email, string name)
    {
        CreatedByUserId = userId;
        CreatedByEmail = email;
        CreatedByName = name;
        Created = DateTime.UtcNow;
        PreviousDataStatus = null;

        EntityStatusCreated = Created;
        EntityStatusCreatedByUserId = userId;
        EntityStatusCreatedByEmail = email;
        EntityStatusCreatedByName = name;

        LastModified = Created;
        LastModifiedByUserId = userId;
        LastModifiedByEmail = email;
        LastModifiedByName = name;

        return this;
    }

    public AuditableEntity OnModify(string userId, string email, string name)
    {
        LastModified = DateTime.UtcNow;
        LastModifiedByUserId = userId;
        LastModifiedByEmail = email;
        LastModifiedByName = name;

        return this;
    }

    public AuditableEntity WithTenantId(string tenantId)
    {
        TenantId = tenantId;
        return this;
    }
     public AuditableEntity WithTenantId(Guid tenantId)
    {
        TenantId = tenantId.ToString();
        return this;
    }
    
    public AuditableEntity WithReferenceNumber(string referenceNumber)
    {
        ReferenceNumber = referenceNumber;
        return this;
    }

    public AuditableEntity OnDataStatusChange(DataState newState)
    {
        if (DataStatus != newState)
        {
            PreviousDataStatus = DataStatus;
            DataStatus = newState;

            // Automatically set status modification metadata
            EntityStatusLastModified = DateTime.UtcNow;
        }

        return this;
    }

    public AuditableEntity OnSoftRemove()
    {
        return OnDataStatusChange(DataState.Deleted);
    }

    public AuditableEntity AddAuditMetadata(string userId, string email, string name, DateTime timestamp)
    {
        // Set creation metadata if this is a new entry
        if (string.IsNullOrWhiteSpace(CreatedByUserId))
        {
            CreatedByUserId = userId;
            CreatedByEmail = email;
            CreatedByName = name;
            Created = timestamp;
        }

        // Always set last modification metadata
        LastModifiedByUserId = userId;
        LastModifiedByEmail = email;
        LastModifiedByName = name;
        LastModified = timestamp;

        // Set entity status modification metadata
        EntityStatusLastModifiedByUserId = userId;
        EntityStatusLastModifiedByEmail = email;
        EntityStatusLastModifiedByName = name;

        return this;
    }


    public AuditableEntity EnableGlobal()
    {
        IsGlobal = true;
        return this;
    }

    public AuditableEntity DisableGlobal()
    {
        IsGlobal = false;
        return this;
    }
 
    public void AsActive()
    {
        DataStatus = DataState.Active;
    }
    public void MarkAsDeleted()
    {
        DataStatus = DataState.Deleted;
    }

    public void Deactivate()
    {
        DataStatus = DataState.Deactivate;
        
        // If this is a workflow-enabled entity, also set workflow status to cancelled
        if (this is WorkflowEnabledEntity workflowEntity)
        {
            workflowEntity.WorkflowStatus = WorkflowStatus.Cancelled;
        }
    }

    public void AsDraft()
    {
        DataStatus = DataState.Drafted;
    }

    public bool IsDraft => DataStatus == DataState.Drafted;

    public void ActivateFromDraft()
    {
        if (DataStatus == DataState.Drafted)
        {
            DataStatus = DataState.Active;
        }
    }

   
}




public enum DataState
{
    Active = 0,
    Deactivate = 1,
    Deleted = 2,
    Archived = 3,
    Block = 4,
    Drafted = 5  // Add this new state for workflow entities
}
