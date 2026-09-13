using Task2.DTOs;
using Task2.Models;

namespace Task2.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public static List<CategoryDto> ToDtoList(List<Category> categories)
        {
            return categories.Select(category => ToDto(category)).ToList();
        }
    }
}