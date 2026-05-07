using AutoMapper;
using FoodShop.API.Mappings;
using Microsoft.Extensions.Configuration;

namespace FoodShop.Tests;

/// <summary>
/// Shared helpers dùng lại ở tất cả test classes
/// </summary>
public static class TestHelpers
{
    /// <summary>Tạo IMapper thật (không mock) với MappingProfile của app</summary>
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    /// <summary>Tạo IConfiguration giả với JWT settings</summary>
    public static IConfiguration CreateJwtConfig()
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["Jwt:Key"]        = "TestSuperSecretKeyHere_MustBe32CharsOrMore!",
            ["Jwt:Issuer"]     = "FoodShopAPI",
            ["Jwt:Audience"]   = "FoodShopClient",
            ["Jwt:ExpiryDays"] = "7"
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();
    }
}
