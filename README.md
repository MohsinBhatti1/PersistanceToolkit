# 📚 PersistenceToolkit — DDD-Friendly Persistence Orchestration

---

## 🚀 What is PersistenceToolkit?

**PersistenceToolkit** is your reusable, modular orchestration layer for **safe, consistent, Domain-Driven Design (DDD)-first persistence** with **EF Core**.

It gives you:
✅ True aggregate root boundaries  
✅ Smart state management & tracking  
✅ Clean separation of read/write responsibilities  
✅ Soft deletes, tenant isolation, and audit logging — all automated  
✅ Full snapshot-based change detection  
✅ Navigation ignore rules for safe partial updates  
✅ Consistent detach after save — zero stale tracking

It turns plain EF Core into a **domain-safe store**.

---

## 🎯 The main goal

PersistenceToolkit’s main goal is to **encapsulate all EF Core behavior behind proper DDD guardrails**, so:
- Your **Domain** layer stays pure.
- Your **Application** layer depends only on clean repository contracts — never `DbContext`.
- Your **Infrastructure** handles orchestration, specs, audit logic, and tracking rules.
- Your aggregates are always saved **as a whole**, with no orphaned child updates.

---

## 🗂️ Project structure

| Package | What it does | Used by |
|---------|---------------|---------|
| **PersistenceToolkit.Domain** | `Entity`, `IAggregateRoot`, `AggregateWalker` | Domain only |
| **PersistenceToolkit.Abstractions** | `IAggregateRepository<T>`, `IEntityRepository<T>`, `IGenericRepository<T>`, `ISystemUser`, `BaseSpecification<T>` | Application |
| **PersistenceToolkit.Persistence** | EF Core implementations: `BaseContext`, `EntityStateProcessor`, `NavigationIgnoreTracker`, config extensions, repositories | Infrastructure |

---

## ✅ Key features that make this DDD-safe

---

### 🟢 1️⃣ Aggregate root enforcement

Your `IAggregateRepository<T>`:
- Works only with `where T : Entity, IAggregateRoot`.
- Enforces that you **never persist a child entity in isolation**.
- You must go through the aggregate root — always.

✅ Compile-time safety for your domain model.

---

### 🟢 2️⃣ Smart EntityStateProcessor

**Your orchestration brain:**
- Uses `HasChange()` snapshot comparison.
- Walks the entire aggregate graph recursively.
- Sets `Added` or `Modified` correctly.
- Skips navigations flagged with `.IgnoreOnUpdate()` via `NavigationIgnoreTracker`.
- Calls `EntityAuditLogSetter` to apply `TenantId`, `CreatedBy`, `UpdatedBy` on each node.
- Detaches all tracked entries after `SaveChanges`.

✅ EF Core never leaves stale tracked entities hanging in memory.

---

### 🟢 3️⃣ Graph traversal done right

Your `AggregateWalker`:
- Walks your root and all nested children.
- Runs any action you want — snapshotting, audit, state setting.
- Prevents missing nested entities when persisting.

---

### 🟢 4️⃣ Detect changes with snapshots

Your `Entity` base:
- Automatically snapshots state when loaded.
- Uses `HasChange()` before update.
- Your `DetectChange` method generates a detailed diff of **which properties changed**, with old & new values.
- Works across the entire graph.

✅ Makes your audit logs bulletproof.

---

### 🟢 5️⃣ Navigation ignore support

You can safely **exclude** nav properties from updates:
```csharp
builder.IgnoreOnUpdate(x => x.MyNavProperty);
