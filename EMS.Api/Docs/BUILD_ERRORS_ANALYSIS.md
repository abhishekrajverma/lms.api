# ?? Build Errors - Complete List & Solutions

**Build Status:** FAILED ?

---

## Summary of Errors

### Total Errors: 23

#### Error Categories:

| Category | Count | Severity | File |
|----------|-------|----------|------|
| **Missing Using Directive** | 11 | ?? HIGH | EMS.Shared\Exceptions\CustomExceptions.cs |
| **Nullable Type Issue** | 1 | ?? HIGH | EMS.Domain\Entities\User.cs |
| **Missing Metadata References** | 11 | ?? MEDIUM | Various (build dependency issue) |

---

## Detailed Error List

### ?? ERROR TYPE 1: Missing `StatusCodes` Class (11 Errors)

**Location:** `EMS.Shared\Exceptions\CustomExceptions.cs`

**Issue:** `StatusCodes` class is not available - missing `using Microsoft.AspNetCore.Http;`

**Affected Lines:**
- Line 33: `NotFoundException`
- Line 38: `NotFoundException` (overload)
- Line 62: `ValidationException`
- Line 67: `ValidationException` (overload)
- Line 86: `UnauthorizedException`
- Line 104: `ForbiddenException`
- Line 123: `ConflictException`
- Line 142: `UnprocessableEntityException`
- Line 161: `InternalServerException`
- Line 181: `ServiceUnavailableException`
- Line 199: `RequestTimeoutException`

**Error Message:**
```
CS0103: The name 'StatusCodes' does not exist in the current context
```

**Solution:** Add `using Microsoft.AspNetCore.Http;` to the file

---

### ?? ERROR TYPE 2: Nullable TimeSpan Issue (1 Error)

**Location:** `EMS.Domain\Entities\User.cs`

**Issue:** Cannot access `.TotalDays` on nullable `TimeSpan?` directly

**Affected Line:** 245

**Error Code:**
```csharp
return (DateTime.UtcNow - PasswordChangedAt).TotalDays > expirationDays;
```

**Error Message:**
```
CS1061: 'TimeSpan?' does not contain a definition for 'TotalDays' and no accessible extension method 'TotalDays' accepting a first argument of type 'TimeSpan?' could be found (are you missing a using directive or an assembly reference?)
```

**Solution:** Handle nullable TimeSpan properly using `.GetValueOrDefault()` or null-coalescing

---

### ?? ERROR TYPE 3: Missing Metadata References (11 Errors)

**Issue:** Build artifacts not found (cascading from the 11 errors above)

**Error Message:**
```
CS0006: Metadata file 'D:\EMS\Backend\EMS.Domain\obj\Debug\net8.0\ref\EMS.Domain.dll' could not be found
```

**Affected Projects:**
- EMS.Domain.dll
- EMS.Shared.dll
- EMS.Application.dll

**Root Cause:** These errors occur because the above 2 error types prevent successful compilation of dependent projects

**Solution:** Fix errors 1 & 2 first, then rebuild

---

## ?? Fix Priority

### Priority 1 (MUST FIX FIRST)
1. ? Add missing `using Microsoft.AspNetCore.Http;` to CustomExceptions.cs
2. ? Fix nullable TimeSpan issue in User.cs

### Priority 2 (AUTO RESOLVED)
3. Rebuild solution (will resolve metadata errors automatically)

---

## Next Steps

1. **Fix CustomExceptions.cs** - Add using directive
2. **Fix User.cs** - Handle nullable TimeSpan
3. **Rebuild solution** - All metadata errors will resolve
4. **Verify** - Run build again

---

**Status:** Ready to fix ?
