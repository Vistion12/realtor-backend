using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Seed
{
    public static class UserSeeder
    {
        public static void Seed(PropertyStoreDBContext context)
        {
            
            if (!context.Users.Any())
            {
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin123";

                context.Users.Add(new UserEntity
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });

                SeedDefaultPipelines(context);

                context.SaveChanges();
                Console.WriteLine("Admin user seeded successfully");
            }
            else
            {
                Console.WriteLine("Users already exist, skipping seed");
            }
        }

        private static void SeedDefaultPipelines(PropertyStoreDBContext context)
        {
            if (!context.DealPipelines.Any())
            {
                var pipelines = new[]
                {
                    new DealPipelineEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Покупка недвижимости",
                        Description = "Процесс покупки недвижимости для клиента",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new DealPipelineEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Продажа недвижимости",
                        Description = "Процесс продажи недвижимости клиента",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new DealPipelineEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Аренда",
                        Description = "Процесс аренды недвижимости",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.DealPipelines.AddRange(pipelines);

                // Создаем стандартные этапы для каждой воронки
                foreach (var pipeline in pipelines)
                {
                    var stages = CreateDefaultStagesForPipeline(pipeline.Id);
                    context.DealStages.AddRange(stages);
                }
            }
        }

        private static List<DealStageEntity> CreateDefaultStagesForPipeline(Guid pipelineId)
        {
            var stages = new List<DealStageEntity>();
            var stageDefinitions = new[]
            {
                new { Name = "Первичный контакт", Duration = TimeSpan.FromDays(2), Order = 0 },
                new { Name = "Определение потребностей", Duration = TimeSpan.FromDays(3), Order = 1 },
                new { Name = "Подбор объектов", Duration = TimeSpan.FromDays(5), Order = 2 },
                new { Name = "Показ объектов", Duration = TimeSpan.FromDays(7), Order = 3 },
                new { Name = "Переговоры по сделке", Duration = TimeSpan.FromDays(5), Order = 4 },
                new { Name = "Предоплата", Duration = TimeSpan.FromDays(2), Order = 5 },
                new { Name = "Оформление документов", Duration = TimeSpan.FromDays(7), Order = 6 },
                new { Name = "Сделка завершена", Duration = TimeSpan.FromDays(0), Order = 7 }
            };

            foreach (var definition in stageDefinitions)
            {
                stages.Add(new DealStageEntity
                {
                    Id = Guid.NewGuid(),
                    Name = definition.Name,
                    Description = $"Этап {definition.Name.ToLower()}",
                    Order = definition.Order,
                    ExpectedDuration = definition.Duration,
                    PipelineId = pipelineId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return stages;
        }
    }
}
