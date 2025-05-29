using Server_1_.Models;
using Microsoft.EntityFrameworkCore;
using Server_1_.Data;
using Server_1_.Services;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Server_1_.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IChatroomService, ChatroomService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IFirebaseNotificationService, FirebaseNotificationService>();
//builder.Services.AddSingleton<IWebHostEnvironment>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        // Cho phép các origin cụ thể trong môi trường phát triển
        // Thay đổi các URL này cho phù hợp với nơi client của bạn sẽ chạy
        builder.WithOrigins(
                "http://localhost:5237", // Cổng HTTP của server (nếu bạn dùng)
                "https://localhost:7092", // Cổng HTTPS của server (nếu bạn dùng)
                "http://localhost:3000",  // Ví dụ: nếu bạn có React/Angular client
                "http://127.0.0.1:5500"   // Ví dụ: nếu bạn dùng Live Server của VS Code
            )
            .AllowAnyHeader() // Cho phép tất cả các header
            .AllowAnyMethod() // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE, OPTIONS)
            .AllowCredentials(); // RẤT QUAN TRỌNG: Cho phép gửi cookie, header ủy quyền, v.v.
                                 // Cần thiết cho SignalR để gửi thông tin kết nối
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapHub<ChatHub>("/chathub"); // Endpoint mà client sẽ kết nối đến

app.MapControllers();

app.Run();
