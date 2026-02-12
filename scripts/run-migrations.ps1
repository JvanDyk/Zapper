dotnet ef database update --project src/Zapper.LoyaltyPoints.Infrastructure --startup-project src/Zapper.LoyaltyPoints.Api.Commands --context LoyaltyDbContext
dotnet run --project src/Zapper.LoyaltyPoints.Api.Commands -- --seed
