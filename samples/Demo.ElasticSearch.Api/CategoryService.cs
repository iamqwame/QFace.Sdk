public class CategoryService : ICategoryService
{
    private readonly ElasticsearchRepository<Category> _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ElasticsearchRepository<Category> repository, 
        ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        try 
        {
            var result = await _repository.GetAllAsync();
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return new List<Category>();
        }
    }

    public async Task<Category> GetCategoryByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> CreateCategoryAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            _logger.LogWarning("Cannot create category with empty name");
            return false;
        }

        category.CreatedDate = DateTime.UtcNow;
        category.LastModifiedDate = DateTime.UtcNow;

        return await _repository.IndexAsync(category);
    }

    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Id))
        {
            _logger.LogWarning("Cannot update category without ID");
            return false;
        }

        category.LastModifiedDate = DateTime.UtcNow;

        return await _repository.UpdateAsync(category);
    }
}