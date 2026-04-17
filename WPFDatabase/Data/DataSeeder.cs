using Microsoft.EntityFrameworkCore;
using WPFDatabase.Models;

namespace WPFDatabase.Data;

public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Brands.Any())
            return;

        var apple = new Brand
        {
            Name = "Apple",
            Country = "USA",
            FoundedYear = 1976
        };

        var iphone = new Series
        {
            Name = "iPhone",
            Segment = "Flagship",
            Brand = apple
        };

        iphone.SmartphoneModels.Add(new SmartphoneModel
        {
            Name = "iPhone 15",
            ReleaseYear = 2023,
            Price = 799,
            RamGb = 6,
            StorageGb = 128
        });

        iphone.SmartphoneModels.Add(new SmartphoneModel
        {
            Name = "iPhone 15 Pro",
            ReleaseYear = 2023,
            Price = 999,
            RamGb = 8,
            StorageGb = 256
        });

        apple.Series.Add(iphone);

        var xiaomi = new Brand
        {
            Name = "Xiaomi",
            Country = "China",
            FoundedYear = 2010
        };

        var redmi = new Series
        {
            Name = "Redmi Note",
            Segment = "Mid-range",
            Brand = xiaomi
        };

        redmi.SmartphoneModels.Add(new SmartphoneModel
        {
            Name = "Redmi Note 13",
            ReleaseYear = 2024,
            Price = 299,
            RamGb = 8,
            StorageGb = 256
        });


        // Данные для структуры дерева
        xiaomi.Series.Add(redmi);

        context.Brands.AddRange(apple, xiaomi);
        context.SaveChanges();
    }
}
