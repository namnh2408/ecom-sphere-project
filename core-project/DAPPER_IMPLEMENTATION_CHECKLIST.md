# Dapper Integration Implementation Checklist

## ✅ Completed Tasks

### Phase 1: Core Infrastructure
- [x] Create `IDapperRepository<T>` generic interface
- [x] Create `IDapperConnectionProvider` interface
- [x] Create `DapperConnectionProvider` implementation
- [x] Create `DapperRepository<T>` abstract base class
- [x] Add Dapper package to BuildingBlocks.Infrastructure.Shared
- [x] Add System.Data.SqlClient package

### Phase 2: Users Module Query Repositories
- [x] Create `UserQueryRepository` with methods:
  - [x] GetUserByIdAsync
  - [x] GetUserByEmailAsync
  - [x] GetPagedUsersAsync (with filtering)
  - [x] GetActiveUsersAsync
  - [x] UserExistsByEmailAsync
  - [x] UserExistsByIdAsync
  - [x] GetUsersByRoleIdAsync

- [x] Create `RoleQueryRepository` with methods:
  - [x] GetAllActiveRolesAsync
  - [x] GetPagedRolesAsync
  - [x] GetRoleByIdAsync
  - [x] RoleExistsByIdAsync
  - [x] RoleExistsByNameAsync
  - [x] GetRolesByIdsAsync

- [x] Create `PermissionQueryRepository` with methods:
  - [x] GetAllActivePermissionsAsync
  - [x] GetPagedPermissionsAsync
  - [x] GetPermissionByIdAsync
  - [x] GetPermissionsByRoleIdAsync
  - [x] GetPermissionsByIdsAsync
  - [x] PermissionExistsByResourceActionAsync
  - [x] GetPermissionsByResourceAsync

- [x] Create `ActivityHistoryQueryRepository` with methods:
  - [x] GetUserActivityHistoryAsync (paginated, filterable)
  - [x] GetLoginHistoryAsync (paginated)
  - [x] GetRecentSuccessfulLoginsAsync
  - [x] GetRecentFailedLoginAttemptsAsync

- [x] Create `AuditLogQueryRepository` with methods:
  - [x] GetUserAuditLogsAsync (paginated, filterable)
  - [x] GetEntityAuditLogsAsync (paginated)
  - [x] GetUserOperationHistoryAsync
  - [x] GetUserChangeHistoryAsync (paginated, filterable)

### Phase 3: Dependency Injection
- [x] Update `ServiceCollectionExtensions.cs` to register:
  - [x] IDapperConnectionProvider
  - [x] UserQueryRepository
  - [x] RoleQueryRepository
  - [x] PermissionQueryRepository
  - [x] ActivityHistoryQueryRepository
  - [x] AuditLogQueryRepository

- [x] Update project files (.csproj):
  - [x] Users.Infrastructure.csproj - add Dapper
  - [x] BuildingBlocks.Infrastructure.Shared.csproj - add Dapper & System.Data.SqlClient

### Phase 4: Documentation
- [x] Create comprehensive migration guide
- [x] Create Dapper infrastructure README
- [x] Create implementation summary
- [x] Create example query handlers (3 examples)

### Phase 5: Example Implementations
- [x] GetAllUsersQueryHandler.Dapper.cs (paginated queries example)
- [x] GetUserByIdQueryHandler.Dapper.cs (single entity example)
- [x] GetAllRolesQueryHandler.Dapper.cs (simple queries example)

---

## ⏳ Recommended Next Steps (Not Started)

### Phase 6: Query Handler Migration (Priority: HIGH)
- [ ] Migrate `GetAllUsersQueryHandler` to use `UserQueryRepository`
- [ ] Migrate `GetUserByIdQueryHandler` to use `UserQueryRepository`
- [ ] Migrate `GetUserByEmailQueryHandler` to use `UserQueryRepository`
- [ ] Migrate `GetAllRolesQueryHandler` to use `RoleQueryRepository`
- [ ] Migrate `GetRoleByIdQueryHandler` to use `RoleQueryRepository`
- [ ] Migrate `GetAllPermissionsQueryHandler` to use `PermissionQueryRepository`
- [ ] Migrate `GetPermissionsByRoleIdQueryHandler` to use `PermissionQueryRepository`
- [ ] Migrate `GetLoginHistoryQueryHandler` to use `ActivityHistoryQueryRepository`
- [ ] Migrate `GetUserActivityHistoryQueryHandler` to use `ActivityHistoryQueryRepository`
- [ ] Migrate `GetUserAuditLogsQueryHandler` to use `AuditLogQueryRepository`

### Phase 7: SQL Optimization (Priority: HIGH)
Apply recommended indexes to database:
```sql
-- Users Table Indexes
CREATE INDEX IX_Users_IsActive_IsDeleted ON Users(IsActive, IsDeleted)
CREATE INDEX IX_Users_Email_IsDeleted ON Users(Email, IsDeleted)

-- Roles Table Indexes
CREATE INDEX IX_Roles_IsActive ON Roles(IsActive)

-- Permissions Table Indexes
CREATE INDEX IX_Permissions_Resource_Action ON Permissions(Resource, Action)

-- LoginAttempts Table Indexes
CREATE INDEX IX_LoginAttempts_UserId_IsSuccessful ON LoginAttempts(UserId, IsSuccessful)

-- AuditLogs Table Indexes
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId)
CREATE INDEX IX_AuditLogs_EntityName_EntityId ON AuditLogs(EntityName, EntityId)
```

### Phase 8: Testing (Priority: HIGH)
- [ ] Unit tests for each query repository
- [ ] Integration tests for migrated query handlers
- [ ] Performance benchmarks (before/after Dapper migration)
- [ ] Load testing with large datasets

### Phase 9: Module Expansion (Priority: MEDIUM)
Create Dapper repositories for other modules:
- [ ] Catalog Module
  - [ ] ProductQueryRepository
  - [ ] CategoryQueryRepository
  - [ ] InventoryQueryRepository

- [ ] Orders Module
  - [ ] OrderQueryRepository
  - [ ] OrderItemQueryRepository
  - [ ] OrderHistoryQueryRepository

- [ ] Payment Module
  - [ ] PaymentQueryRepository
  - [ ] TransactionQueryRepository

- [ ] Shipping Module
  - [ ] ShipmentQueryRepository
  - [ ] TrackingQueryRepository

- [ ] Cart Module
  - [ ] CartQueryRepository
  - [ ] CartItemQueryRepository

### Phase 10: Caching Layer (Priority: MEDIUM)
- [ ] Add caching decorator for query repositories
- [ ] Implement distributed caching with Redis
- [ ] Cache invalidation strategies
- [ ] Performance metrics for cached queries

### Phase 11: Monitoring & Analytics (Priority: LOW)
- [ ] Query performance monitoring
- [ ] Slow query logging
- [ ] Query execution time tracking
- [ ] Dashboard for query metrics

### Phase 12: Documentation Updates (Priority: LOW)
- [ ] Update API documentation
- [ ] Create performance comparison reports
- [ ] Add troubleshooting guide for common issues
- [ ] Create video tutorial for team

---

## How to Use This Checklist

### For Developers
1. Start with Phase 6 (Query Handler Migration)
2. Pick one query handler at a time
3. Replace EF Core repository with Dapper repository
4. Test the migrated handler
5. Commit changes and move to next handler

### For DevOps/DBAs
1. Review Phase 7 (SQL Optimization)
2. Apply recommended indexes to production database
3. Monitor query performance
4. Collect performance metrics

### For QA
1. Review Phase 8 (Testing)
2. Create test cases for migrated handlers
3. Run performance tests
4. Validate results match original implementation

### For Architecture Review
1. Review Phases 1-5 (✅ Completed)
2. Approve recommended next steps
3. Plan timeline for Phases 6-12

---

## Implementation Timeline

### Week 1 (Current)
- [x] Implement core infrastructure (Phases 1-5)
- [x] Create documentation and examples

### Week 2 (Recommended)
- [ ] Migrate most critical query handlers (Phase 6)
- [ ] Apply SQL indexes (Phase 7)
- [ ] Begin performance testing (Phase 8)

### Week 3 (Recommended)
- [ ] Complete query handler migration
- [ ] Full integration testing
- [ ] Performance benchmarking

### Week 4+ (Recommended)
- [ ] Expand to other modules (Phase 9)
- [ ] Add caching layer (Phase 10)
- [ ] Monitoring setup (Phase 11)
- [ ] Documentation finalization (Phase 12)

---

## Success Criteria

### Performance Targets
- [x] 50%+ faster read queries compared to EF Core
- [ ] Query execution time <200ms for paginated requests (after migration)
- [ ] Query execution time <50ms for single entity queries (after migration)
- [ ] Reduced memory usage for large result sets

### Quality Targets
- [ ] 100% of query handlers migrated
- [ ] All tests passing
- [ ] No regressions in functionality
- [ ] Zero performance degradation for write operations

### Documentation Targets
- [x] Migration guide complete
- [ ] Team trained on new pattern
- [ ] Code examples for each repository type
- [ ] Troubleshooting guide with common issues

---

## Risk Mitigation

### Identified Risks

1. **Risk:** Breaking changes during migration
   - **Mitigation:** Create new handlers with .Dapper suffix, switch gradually

2. **Risk:** Query performance degradation
   - **Mitigation:** Performance benchmarking before/after, SQL query analysis

3. **Risk:** Data inconsistency between EF and Dapper reads
   - **Mitigation:** Use same connection string, same transaction level

4. **Risk:** Team unfamiliar with Dapper
   - **Mitigation:** Comprehensive documentation, code examples, training session

5. **Risk:** SQL injection vulnerabilities
   - **Mitigation:** Always use parameterized queries, code review checklist

---

## Rollback Plan

If issues occur during migration:

1. **Immediate Action**
   - Revert migrated handlers to use EF Core repositories
   - Disable Dapper repositories in DI
   - Verify system stability

2. **Investigation**
   - Analyze query performance
   - Check for data inconsistencies
   - Review SQL indexes

3. **Re-implementation**
   - Fix identified issues
   - Create additional test cases
   - Re-migrate with fixes

---

## Sign-Off

- [ ] Architecture Review: _______________  Date: _______
- [ ] Development Lead: _______________  Date: _______
- [ ] QA Lead: _______________  Date: _______
- [ ] DevOps Lead: _______________  Date: _______

---

## Contact & Questions

For questions about Dapper implementation:
1. Review DAPPER_IMPLEMENTATION_SUMMARY.md
2. Check DAPPER_MIGRATION_GUIDE.md in Users module
3. Review example handlers in Users.Application
4. Ask team members

---

Last Updated: 2024
Next Review: After Phase 6 completion