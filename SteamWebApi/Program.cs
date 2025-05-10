using BusinessLayer.Services.Interfaces;
using BusinessLayer.Services;
using Microsoft.EntityFrameworkCore;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Repositories;
using BusinessLayer.DataContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICollectionsService, CollectionsService>();
builder.Services.AddScoped<IFeaturesService, FeaturesService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IFriendService, FriendService>();


builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ICollectionsRepository, CollectionsRepository>();
builder.Services.AddScoped<IFeaturesRepository, FeaturesRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();

// Register EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
