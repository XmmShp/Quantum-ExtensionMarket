# ExtensionMarket

ExtensionMarket 是一个用于分享、发布和管理扩展插件的平台。该项目提供了完整的 API 服务，允许用户上传、下载、评价和管理各种扩展插件。

## 项目特点

- 完整的用户认证和授权系统
- 扩展插件的版本管理
- 评分和评论功能
- 基于标签的搜索和分类
- 文件存储和管理
- RESTful API 接口

## 技术栈

- .NET 9
- PostgreSQL 数据库
- JWT 认证
- Docker 支持

ExtensionMarket 应用支持通过环境变量进行配置，这使得在不同环境（开发、测试、生产）中部署应用变得更加灵活。本文档列出了所有可用的环境变量及其用途。

## 数据库配置

| 环境变量       | 说明             | 默认值                                                          |
| -------------- | ---------------- | --------------------------------------------------------------- |
| `DBCONNECTION` | 数据库连接字符串 | 从 appsettings.json 的 ConnectionStrings:DefaultConnection 读取 |

示例：

```
DBCONNECTION="Host=localhost;Port=5432;Database=ExtensionMarket;Username=postgres;Password=123456"
```

## 文件存储配置

| 环境变量               | 说明             | 默认值                                                                   |
| ---------------------- | ---------------- | ------------------------------------------------------------------------ |
| `FILESTORAGE_BASEPATH` | 文件存储基础路径 | 从 appsettings.json 的 FileStorage:BasePath 读取，如果未设置则为 ./Files |

示例：

```
FILESTORAGE_BASEPATH="/app/files"
```

## JWT 配置

| 环境变量                     | 说明                   | 默认值                                                                          |
| ---------------------------- | ---------------------- | ------------------------------------------------------------------------------- |
| `JWT_KEY`                    | JWT 密钥               | 从 appsettings.json 的 JWT:Secret 读取                                          |
| `JWT_ISSUER`                 | JWT 颁发者             | 从 appsettings.json 的 JWT:ValidIssuer 读取，如果未设置则为 "ExtensionMarket"   |
| `JWT_AUDIENCE`               | JWT 受众               | 从 appsettings.json 的 JWT:ValidAudience 读取，如果未设置则为 "ExtensionMarket" |
| `JWT_TOKEN_VALIDITY_MINUTES` | JWT 令牌有效期（分钟） | 从 appsettings.json 的 JWT:TokenValidityInMinutes 读取，如果未设置则为 60       |

示例：

```
JWT_KEY="your-secure-key-here"
JWT_ISSUER="https://api.extensionmarket.com"
JWT_AUDIENCE="https://extensionmarket.com"
JWT_TOKEN_VALIDITY_MINUTES="120"
```

## 开发环境特殊变量

| 环境变量                    | 说明                               | 默认值 |
| --------------------------- | ---------------------------------- | ------ |
| `EXTENSIONMARKET_MIGRATION` | 设置此变量会触发数据库重置和初始化 | 未设置 |

示例：

```
EXTENSIONMARKET_MIGRATION="true"
```

## 在开发环境中设置环境变量

### Windows (PowerShell)

```powershell
$env:DBCONNECTION="Host=localhost;Port=5432;Database=ExtensionMarket;Username=postgres;Password=123456"
$env:JWT_KEY="your-secure-key-here"
```

### Windows (CMD)

```cmd
set DBCONNECTION=Host=localhost;Port=5432;Database=ExtensionMarket;Username=postgres;Password=123456
set JWT_KEY=your-secure-key-here
```

### Linux/macOS

```bash
export DBCONNECTION="Host=localhost;Port=5432;Database=ExtensionMarket;Username=postgres;Password=123456"
export JWT_KEY="your-secure-key-here"
```

## 在生产环境中设置环境变量

在生产环境中，建议使用容器化部署（如 Docker）或云平台的环境变量配置功能。

### Docker 示例

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENV DBCONNECTION="Host=db;Port=5432;Database=ExtensionMarket;Username=postgres;Password=secure-password"
ENV JWT_KEY="production-secure-key"
ENV JWT_ISSUER="https://api.extensionmarket.com"
ENV JWT_AUDIENCE="https://extensionmarket.com"
ENTRYPOINT ["dotnet", "ExtensionMarket.dll"]
```

或者使用 docker-compose：

```yaml
version: "3"
services:
  api:
    image: extensionmarket-api
    environment:
      - DBCONNECTION=Host=db;Port=5432;Database=ExtensionMarket;Username=postgres;Password=secure-password
      - JWT_KEY=production-secure-key
      - JWT_ISSUER=https://api.extensionmarket.com
      - JWT_AUDIENCE=https://extensionmarket.com
```

## 开发计划

### gRPC 迁移

我们正在计划将当前的 RESTful API 迁移到 gRPC 上，以提高性能和效率。这项工作正在进行中，欢迎社区贡献。

## 贡献指南

我们欢迎任何形式的贡献，包括但不限于：

- 功能增强
- Bug 修复
- 文档改进
- 测试用例
- gRPC 迁移相关工作

如果您有兴趣参与贡献，请提交 Pull Request 或创建 Issue 进行讨论。

## 许可证

[MIT](LICENSE)
