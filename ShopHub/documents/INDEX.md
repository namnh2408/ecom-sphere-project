# 📚 ShopHub Documentation Index

Hướng dẫn đọc các tài liệu theo thứ tự để hiểu toàn bộ hệ thống:

---

## 📖 Reading Order (Thứ Tự Đề Nghị)

### **1️⃣ [01_README_ARCHITECTURE.md](./01_README_ARCHITECTURE.md)**
📌 **Bắt đầu từ đây**
- Overview của kiến trúc dự án
- High-level architecture diagram
- Component overview
- **Dành cho**: Người mới bắt đầu

---

### **2️⃣ [02_ARCHITECTURE.md](./02_ARCHITECTURE.md)**
📌 **Sau khi hiểu overview**
- Chi tiết từng layer (Domain, Application, Infrastructure)
- Design patterns sử dụng
- Database schema
- **Dành cho**: Developers muốn hiểu deep architecture

---

### **3️⃣ [03_CQRS_MEDIATR_GUIDE.md](./03_CQRS_MEDIATR_GUIDE.md)**
📌 **Hiểu CQRS pattern**
- CQRS pattern implementation
- MediatR usage
- Command vs Query separation
- **Dành cho**: Developers làm việc với queries & commands

---

### **4️⃣ [04_DAPPER_REDIS_QUICK_START.md](./04_DAPPER_REDIS_QUICK_START.md)**
📌 **Nhanh chóng setup & test**
- 5-minute quick setup guide
- Start Redis in Docker
- Common usage patterns
- Troubleshooting tips
- **Dành cho**: DevOps & Developers muốn nhanh chóng test

---

### **5️⃣ [05_DAPPER_REDIS_GUIDE.md](./05_DAPPER_REDIS_GUIDE.md)**
📌 **Deep dive vào Dapper & Redis**
- Dapper query optimization
- Redis caching strategy
- Cache invalidation patterns
- Performance benchmarks
- **Dành cho**: Developers optimize queries & caching

---

### **6️⃣ [06_IMPLEMENTATION_GUIDE.md](./06_IMPLEMENTATION_GUIDE.md)**
📌 **Step-by-step implementation**
- How to create new queries
- How to add caching
- Configuration details
- Testing guidelines
- **Dành cho**: Developers extend hệ thống

---

### **7️⃣ [07_IMPLEMENTATION_SUMMARY.md](./07_IMPLEMENTATION_SUMMARY.md)**
📌 **Tóm tắt kỹ thuật**
- All changes made
- Files created/modified
- Technical decisions
- Cache key formats
- **Dành cho**: Code reviewers & architects

---

## 🎯 Quick Navigation by Role

### 👨‍💼 **Project Manager / Stakeholder**
→ Read: `01_README_ARCHITECTURE.md`

### 🏗️ **Solution Architect**
→ Read: `01` → `02` → `07`

### 👨‍💻 **New Developer**
→ Read: `01` → `02` → `03` → `04`

### ⚙️ **DevOps Engineer**
→ Read: `04` → `05` (Redis & Performance sections)

### 🔧 **Backend Developer**
→ Read: `02` → `03` → `05` → `06`

### 🧪 **QA / Tester**
→ Read: `04` → `06` (Testing sections)

---

## 📊 Quick Reference

| File | Focus | Lines | Time |
|------|-------|-------|------|
| 01 | Overview | ~150 | 5 min |
| 02 | Architecture | ~200 | 10 min |
| 03 | CQRS/MediatR | ~300 | 15 min |
| 04 | Quick Setup | ~100 | 5 min |
| 05 | Dapper/Redis | ~631 | 30 min |
| 06 | How-to Guide | ~400 | 20 min |
| 07 | Summary | ~400 | 15 min |

**Total**: ~2,100 lines | **Est. Time**: 100 minutes

---

## ⚡ Essential Topics

### 🚀 Getting Started
- See: `04_DAPPER_REDIS_QUICK_START.md`

### 🏗️ Understanding Architecture
- See: `01_README_ARCHITECTURE.md` → `02_ARCHITECTURE.md`

### 📝 CQRS Pattern
- See: `03_CQRS_MEDIATR_GUIDE.md`

### ⚡ Performance Optimization
- See: `05_DAPPER_REDIS_GUIDE.md`

### 🔨 Implementation How-to
- See: `06_IMPLEMENTATION_GUIDE.md`

### 📋 Complete Details
- See: `07_IMPLEMENTATION_SUMMARY.md`

---

## 🔗 Cross-References

- **Dapper queries**: See `05_DAPPER_REDIS_GUIDE.md` + `06_IMPLEMENTATION_GUIDE.md`
- **Redis caching**: See `04_DAPPER_REDIS_QUICK_START.md` + `05_DAPPER_REDIS_GUIDE.md`
- **Cache invalidation**: See `07_IMPLEMENTATION_SUMMARY.md` (Cache Invalidation Strategy section)
- **CQRS pattern**: See `03_CQRS_MEDIATR_GUIDE.md`

---

**Last Updated**: Oct 25, 2025  
**Status**: ✅ Production Ready