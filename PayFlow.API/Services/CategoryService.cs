using PayFlow.API.DTOs.Responses;

namespace PayFlow.API.Services;

public interface ICategoryService
{
    List<CategoryResponse> GetCategories();
}

public class CategoryService : ICategoryService
{
    private static readonly List<CategoryResponse> Categories = new()
    {
        new() { Id = "vestuario", Name = "Vestuário", Color = "#F37020" },
        new() { Id = "beleza", Name = "Beleza", Color = "#FF2D55" },
        new() { Id = "calcados", Name = "Calçados", Color = "#0A84FF" },
        new() { Id = "acessorios", Name = "Acessórios", Color = "#FF9F0A" },
        new() { Id = "eletronicos", Name = "Eletrônicos", Color = "#00C853" }
    };

    public List<CategoryResponse> GetCategories()
    {
        return Categories;
    }
}
