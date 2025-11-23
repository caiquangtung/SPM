# Tests

Folder `tests/` chứa tất cả test projects cho các services trong hệ thống SPM.

## 📁 Cấu trúc

```
tests/
├── user-service.Tests/          # Tests cho User Service
├── project-service.Tests/       # Tests cho Project Service
└── file-service.Tests/          # Tests cho File Service
```

## 🎯 Mục đích

Tách biệt test code khỏi production code để:

- ✅ **Separation of Concerns**: Test code không nằm trong service folders
- ✅ **Clean Structure**: Service folders chỉ chứa production code
- ✅ **Easy Maintenance**: Tất cả tests ở một nơi, dễ quản lý
- ✅ **CI/CD Friendly**: Dễ configure test runs trong pipelines

## 🧪 Test Framework

Tất cả test projects sử dụng:

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database cho integration tests

## 🚀 Chạy Tests

### Chạy tất cả tests:

```bash
# Từ root directory
dotnet test tests/

# Hoặc từ tests folder
cd tests
dotnet test
```

### Chạy tests cho service cụ thể:

```bash
# User Service tests
dotnet test tests/user-service.Tests/

# Project Service tests
dotnet test tests/project-service.Tests/

# File Service tests
dotnet test tests/file-service.Tests/
```

### Chạy với coverage:

```bash
dotnet test tests/ --collect:"XPlat Code Coverage"
```

### Chạy tests trong Visual Studio / Rider:

- Mở solution file (nếu có)
- Right-click test project → Run Tests
- Hoặc dùng Test Explorer

## 📝 Test Organization

Mỗi test project mirror structure của service tương ứng:

```
user-service.Tests/
├── Services/
│   ├── PasswordServiceTests.cs
│   └── AuthServiceVerifyEmailTests.cs
└── Validators/
    ├── LoginRequestValidatorTests.cs
    └── RegisterRequestValidatorTests.cs
```

## ✅ Best Practices

### ✅ DO:

- **Arrange-Act-Assert (AAA)** pattern
- **Descriptive test names**: `MethodName_Scenario_ExpectedBehavior`
- **One assertion per test** (khi có thể)
- **Mock external dependencies** (database, HTTP clients, Kafka)
- **Use InMemory database** cho repository tests
- **Test edge cases** và error scenarios

### ❌ DON'T:

- Test implementation details
- Create real database connections trong unit tests
- Test multiple things trong một test
- Hard-code test data (dùng builders/factories)
- Ignore failing tests

## 📊 Test Coverage Goals

- **Unit Tests**: >80% coverage cho Services và Repositories
- **Integration Tests**: Critical paths và workflows
- **E2E Tests**: Main user flows (sẽ được thêm sau)

## 🔗 Project References

Test projects reference service projects qua relative paths:

```xml
<ProjectReference Include="../../services/user-service/user-service.csproj" />
```

**Path structure:**

```
tests/
  └── user-service.Tests/
      └── user-service.Tests.csproj → ../../services/user-service/user-service.csproj
```

## 🛠️ Development Workflow

1. **Write test first** (TDD) hoặc **after implementation**
2. **Run tests locally** trước khi commit
3. **Ensure all tests pass** trong CI/CD pipeline
4. **Maintain test coverage** >80%

## 📚 Related Documentation

- [Service READMEs](../services/*/README.md) - Service-specific documentation
- [IMPLEMENTATION_PLAN.md](../documents/IMPLEMENTATION_PLAN.md) - Testing requirements
