using StackOverflow.Web.Data;
using StackOverflow.Web.Data.Repositories;
using StackOverflow.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Register database connection factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();

// Register services
builder.Services.AddScoped<ISearchService, SearchService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Add PasswordHash column if it doesn't exist
using (var connection = app.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection())
{
    connection.Open();
    using var cmd = connection.CreateCommand();
    cmd.CommandText = @"IF COL_LENGTH('Users', 'PasswordHash') IS NULL
        ALTER TABLE Users ADD PasswordHash NVARCHAR(256) NULL";
    cmd.ExecuteNonQuery();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
