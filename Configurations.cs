namespace ExtensionMarket;

/// <summary>
/// 应用程序配置
/// </summary>
public static class Configurations
{
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    public static string DbConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 文件存储基础路径
    /// </summary>
    public static string FileStorageBasePath { get; set; } = string.Empty;

    /// <summary>
    /// JWT密钥
    /// </summary>
    public static string JwtKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT颁发者
    /// </summary>
    public static string JwtIssuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT受众
    /// </summary>
    public static string JwtAudience { get; set; } = string.Empty;

    /// <summary>
    /// JWT令牌有效期（分钟）
    /// </summary>
    public static int JwtTokenValidityInMinutes { get; set; } = 60;
}


/// <summary>
/// 常量定义
/// </summary>
public static class Constants
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public const string ExtensionMarket = "ExtensionMarket";
}
