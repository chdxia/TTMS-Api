var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Dev.json")
    .Build();

Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
{
    IFreeSql fsql = new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.PostgreSQL, @config.GetSection("DBConnectionStrings")["DefaultConnection"])
        .UseMonitorCommand(cmd => Console.WriteLine($"Sql��{cmd.CommandText}"))//����SQL���
        .UseAutoSyncStructure(true) //�Զ�ͬ��ʵ��ṹ�����ݿ⣬FreeSql����ɨ����򼯣�ֻ��CRUDʱ�Ż����ɱ�
        .Build();
    return fsql;
};

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "LRtest.Api.xml"));// ���� XML ע��
});
builder.Services.AddSingleton(fsqlFactory);
builder.Services.AddSingleton<IUserRepository, UserRepository>();

var app = builder.Build();

/*
using (IServiceScope serviceScope = app.Services.CreateScope())
{
    var fsql = serviceScope.ServiceProvider.GetRequiredService<IFreeSql>();
    fsql.CodeFirst.SyncStructure(typeof(LRtest.Domain.User));//Topic ΪҪͬ����ʵ����
}
*/

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
