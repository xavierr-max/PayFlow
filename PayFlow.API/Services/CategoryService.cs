using PayFlow.API.DTOs.Responses;
using PayFlow.API.DTOs.Requests;
using PayFlow.API.Exceptions;
using PayFlow.Domain.Sales.Entities;

namespace PayFlow.API.Services;

public interface ICategoryService
{
    List<CategoryResponse> GetCategories();
    CategoryResponse CreateCategory(SaveCategoryRequest request);
    CategoryResponse UpdateCategory(Guid id, SaveCategoryRequest request);
}

public class CategoryService : ICategoryService
{
    private readonly IDataRepository _repository;

    private static readonly (string Name, string Color)[] DefaultCategories =
    {
        ("Vestuário", "#F37020"),
        ("Beleza", "#FF2D55"),
        ("Calçados", "#0A84FF"),
        ("Acessórios", "#FF9F0A"),
        ("Eletrônicos", "#00C853")
    };

    public CategoryService(IDataRepository repository)
    {
        _repository = repository;
    }

    public List<CategoryResponse> GetCategories()
    {
        EnsureDefaultCategories();
        return _repository.GetCategories().Select(MapToResponse).ToList();
    }

    public CategoryResponse CreateCategory(SaveCategoryRequest request)
    {
        var category = new Category(request.Name, request.Color);
        _repository.AddCategory(category);
        return MapToResponse(category);
    }

    public CategoryResponse UpdateCategory(Guid id, SaveCategoryRequest request)
    {
        var category = _repository.GetCategory(id);
        if (category == null)
            throw new NotFoundException($"Category with ID {id} not found");

        category.Update(request.Name, request.Color);
        _repository.UpdateCategory(category);
        return MapToResponse(category);
    }

    private void EnsureDefaultCategories()
    {
        if (_repository.GetCategories().Count > 0)
            return;

        foreach (var (name, color) in DefaultCategories)
            _repository.AddCategory(new Category(name, color));
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            Color = category.Color
        };
    }
}
