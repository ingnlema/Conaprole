# 🧪 Integration Tests Setup Guide

## Overview

The integration tests use **Testcontainers** to set up PostgreSQL and Keycloak instances for testing. This document provides guidance on troubleshooting and configuring the test environment.

## Container Configuration

### PostgreSQL Container
- **Image**: `postgres:15-alpine` (specific version for stability)
- **Database**: `conaprole.orders`
- **Credentials**: `postgres/postgres`
- **Startup time**: ~3-5 seconds

### Keycloak Container
- **Image**: `quay.io/keycloak/keycloak:21.1.1` (specific version for stability)
- **Configuration**: Imports realm from `.files/conaprole-realm-export.json`
- **Startup time**: ~15-30 seconds (variable based on environment)

## Common Issues & Solutions

### 1. LINQ Translation Errors

**Problem**: `System.InvalidOperationException: The LINQ expression '__testEmails_0.Contains(u.Email.Value)' could not be translated.`

**Solution**: EF Core cannot translate complex LINQ expressions involving Value Objects. Use Value Object equality patterns instead:

```csharp
// ❌ Avoid this pattern
var users = await DbContext.Set<User>()
    .Where(u => testEmails.Contains(u.Email.Value))
    .ToListAsync();

// ❌ Also avoid direct .Value property access in LINQ
var user = await DbContext.Set<User>()
    .FirstOrDefaultAsync(u => u.Email.Value == email);

// ✅ Use Value Object equality instead
var existingUsers = new List<User>();
foreach (var email in testEmails)
{
    var emailValueObject = new Domain.Users.Email(email);
    var user = await DbContext.Set<User>()
        .FirstOrDefaultAsync(u => u.Email == emailValueObject);
    if (user != null)
        existingUsers.Add(user);
}
```

**Key Points:**
- Create Value Object instances for comparison: `new Domain.Users.Email(email)`
- Use direct equality: `u.Email == emailValueObject` instead of `u.Email.Value == email`
- This leverages EF Core's HasConversion configuration for proper SQL translation

### 2. Container Startup Failures

**Problem**: Docker container fails to start with internal server errors.

**Causes & Solutions**:

- **Docker platform compatibility**: Set environment variable `DOCKER_DEFAULT_PLATFORM=linux/amd64`
- **Resource limitations**: Ensure sufficient memory (>8GB recommended)
- **Network restrictions**: Check firewall/proxy settings
- **Container conflicts**: Clean up existing containers

**Recovery strategies**:
- Automatic retry logic (implemented for Keycloak)
- Gradual timeout increases
- Container cleanup on failure

### 3. Timeout Issues

**Problem**: Tests fail due to container startup timeouts.

**Configuration**:
- Container startup timeout: 5 minutes
- PostgreSQL readiness check: 2 seconds delay
- Keycloak readiness check: 5 seconds delay + retry logic

### 4. CI/CD Environment Issues

**Common CI issues**:
- Limited Docker resources
- Network restrictions (blocked registries)
- Longer startup times

**Recommendations**:
- Use specific container versions (not `latest`)
- Implement retry logic for critical containers
- Add appropriate timeouts for CI environments
- Consider container registry caching

## Environment Variables

Set these environment variables for better compatibility:

```bash
# Docker platform compatibility
DOCKER_DEFAULT_PLATFORM=linux/amd64

# Optional: Increase timeouts for slower environments
TESTCONTAINERS_WAIT_TIMEOUT=300
```

## Testing in Different Environments

### Local Development
- Usually works out of the box
- Docker Desktop should be running
- Sufficient system resources required

### CI/CD
- May require additional Docker configuration
- Consider using pre-built container images
- Monitor for network/registry access issues

### Docker-in-Docker
- May require privileged containers
- Socket mounting considerations
- Additional security constraints

## Troubleshooting Commands

```bash
# Check Docker status
docker system info

# Clean up test containers
docker container prune -f

# Check available resources
docker system df

# View container logs (if container ID known)
docker logs <container_id>
```

## Performance Optimization

1. **Use specific image versions** (not `latest`)
2. **Pre-pull images** in CI pipelines
3. **Clean up containers** between test runs
4. **Monitor resource usage** during tests
5. **Consider image caching** strategies

## Best Practices

1. **Always call base.InitializeAsync()** in test setup
2. **Use proper `new` keywords** to avoid method hiding warnings
3. **Handle container failures gracefully** with retry logic
4. **Clean up test data** between tests
5. **Use appropriate timeouts** for your environment

---

*For more information, see [Testing Architecture](../testing/testing-architecture.md)*