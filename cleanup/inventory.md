# 📋 Repository Cleanup Inventory

> **Generated**: 2025-07-02  
> **Purpose**: Detailed analysis of files and directories outside `src/` and `test/` for cleanup and normalization

---

## 📁 Root Level Analysis

| Item | Type | Size | Status | Action | Rationale |
|------|------|------|--------|--------|-----------|
| `.dockerignore` | File | 314B | ✅ **Keep** | None | Docker build configuration - essential |
| `.github/` | Directory | - | ✅ **Keep** | None | CI/CD workflows - essential for automation |
| `.gitignore` | File | 648B | ✅ **Keep** | Enhance | Current version is basic, needs expansion |
| `.markdownlint.json` | File | 1.2KB | ✅ **Keep** | None | Documentation tooling configuration |
| `Conaprole.Orders/` | Directory | - | ❌ **Delete** | Remove entirely | Redundant old project structure, real code is in `src/` |
| `Conaprole.Orders.sln` | File | 6KB | ✅ **Keep** | None | .NET solution file - required for builds |
| `Keycloak/` | Directory | - | ✅ **Keep** | None | Infrastructure/auth setup - documented and used |
| `LOGS.md` | File | 2.8KB | 🔄 **Move** | Move to `docs/` | Documentation should be in docs folder |
| `Makefile` | File | 3.2KB | ✅ **Keep** | Enhance | Build automation - add cleanup commands |
| `README.md` | File | 6.9KB | ✅ **Keep** | None | Project documentation - essential |
| `docker-compose.yaml` | File | 1.2KB | ✅ **Keep** | None | Container orchestration - used for development |
| `docs/` | Directory | - | ✅ **Keep** | None | Project documentation - well structured |
| `export/` | Directory | - | ❌ **Delete** | Remove entirely | Keycloak export artifacts - should be generated, not stored |
| `scripts/` | Directory | - | ✅ **Keep** | None | Automation scripts - essential tooling |
| `src/` | Directory | - | ✅ **Keep** | None | Source code - core project content |
| `test/` | Directory | - | ✅ **Keep** | Clean artifacts | Tests - but remove generated artifacts |

---

## 🗑️ Test Artifacts to Clean

| Item | Location | Status | Action |
|------|----------|--------|--------|
| `TestResults/` | `test/Conaprole.Orders.Application.IntegrationTests/` | ❌ **Delete** | Remove directory and add to .gitignore |
| `*.trx` files | Various test directories | ❌ **Delete** | Should be generated, not committed |

---

## 📂 Detailed Analysis

### ❌ Files/Directories to DELETE

#### 1. `Conaprole.Orders/` Directory
- **Reason**: Appears to be an old/legacy project structure
- **Content**: Simple "Hello World" ASP.NET Core app with minimal functionality
- **Impact**: No impact - real application is in `src/Conaprole.Orders.Api/`
- **Size**: ~4 files, minimal

#### 2. `export/` Directory  
- **Reason**: Contains Keycloak export files that should be generated, not stored in repo
- **Content**: 
  - `Conaprole-realm.json`
  - `Conaprole-users-0.json` 
  - `conaprole-realm-export.json`
  - `script.py` (merge script)
- **Impact**: Low - Keycloak directory has proper export file
- **Alternative**: These exports can be regenerated from Keycloak instance

#### 3. `test/Conaprole.Orders.Application.IntegrationTests/TestResults/`
- **Reason**: Test execution artifacts should not be committed
- **Content**: `_fv-az1112-52_2025-06-07_15_17_23.trx` - CI test results
- **Impact**: None - automatically generated during test runs

### 🔄 Files to MOVE

#### 1. `LOGS.md` → `docs/reference/logging.md`
- **Reason**: Documentation should be organized in docs/ folder
- **Content**: Comprehensive logging documentation for Serilog setup
- **Impact**: Positive - better documentation organization

### ✅ Files to KEEP (with enhancements)

#### 1. `.gitignore` - Enhance with comprehensive exclusions
#### 2. `Makefile` - Add `clean-repo` command for local cleanup

---

## 🎯 Proposed Actions Summary

### Phase 1: Immediate Cleanup
1. ❌ Delete `Conaprole.Orders/` directory completely
2. ❌ Delete `export/` directory completely  
3. ❌ Delete `test/Conaprole.Orders.Application.IntegrationTests/TestResults/` directory
4. 🔄 Move `LOGS.md` to `docs/reference/logging.md`

### Phase 2: Configuration Enhancement  
5. ✅ Enhance `.gitignore` with comprehensive exclusions
6. ✅ Add `clean-repo` command to `Makefile`

### Phase 3: Validation
7. ✅ Verify CI/CD pipeline still works
8. ✅ Verify documentation links are updated
9. ✅ Test local development workflow

---

## 📊 Impact Assessment

| Category | Before | After | Impact |
|----------|--------|-------|--------|
| **Root directories** | 11 dirs | 9 dirs | 📉 Cleaner structure |
| **Documentation** | Scattered | Organized | 📈 Better organization |
| **Build artifacts** | Some committed | None committed | 📈 Cleaner repo |
| **CI/CD functionality** | Working | Working | 🔄 No impact |

---

## ⚠️ Risk Analysis

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking CI/CD | Low | High | Test after each change |
| Missing documentation | Low | Medium | Verify all moves/links |
| Lost Keycloak config | Low | Medium | Verify Keycloak/ has needed files |
| Developer confusion | Medium | Low | Clear commit messages and documentation |

---

*Total cleanup impact: **Minimal risk**, **High benefit** for repository organization and developer experience.*