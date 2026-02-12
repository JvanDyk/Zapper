#!/bin/bash
dotnet ef database update --project src/Zapper.LoyaltyPoints.Infrastructure --startup-project src/Zapper.LoyaltyPoints.Api --context LoyaltyDbContext
dotnet ef database update --project src/Zapper.LoyaltyPoints.Infrastructure --startup-project src/Zapper.LoyaltyPoints.Api --context QueueDbContext
