# Sprint 2 Frontend Testing Guide

**Sprint:** Sprint 2 - Project & Task Management  
**Status:** ✅ Ready for Testing  
**Date:** January 2, 2026

---

## 🎯 **Overview**

This guide covers testing the Sprint 2 frontend features:

- ✅ Projects list and creation
- ✅ Kanban board (drag & drop)
- ✅ Task list view with filters
- ✅ Task creation and status updates
- ✅ Comments on tasks
- ✅ File uploads and attachments
- ✅ AI-powered semantic search

---

## 📋 **Prerequisites**

### 1. **Backend Services Running**

```bash
# Start Docker services
cd /path/to/SPM
docker-compose up -d

# Verify services are healthy
docker-compose ps

# Expected services:
# - spm-postgres (PostgreSQL)
# - spm-kafka (Kafka)
# - spm-api-gateway (port 5000)
# - spm-user-service (port 5001)
# - spm-project-service (port 5002)
# - spm-file-service (port 5003)
```

### 2. **Frontend Configuration**

```bash
# Verify .env.local points to API Gateway
cat frontend/.env.local

# Should show:
# NEXT_PUBLIC_API_URL=http://localhost:5000
```

✅ **DONE** - Already configured

### 3. **Dependencies Installed**

```bash
cd frontend
npm install
```

✅ **DONE** - Already installed

---

## 🚀 **Start Frontend**

```bash
cd frontend
npm run dev

# Frontend will be available at: http://localhost:3000
```

---

## 🧪 **Test Scenarios**

### **Test 1: Authentication Flow** ✅ (Sprint 1)

**Purpose:** Verify auth works before testing Sprint 2 features

```
1. Open http://localhost:3000
2. Click "Register" (if new user)
3. Fill form:
   - Email: testuser@example.com
   - Password: Test@1234
   - Full Name: Test User
4. Click "Register"
5. Expected: Success message, redirect to verify email page
6. Login with same credentials
7. Expected: Redirect to dashboard
```

**✅ Pass Criteria:**

- Registration succeeds
- Login succeeds
- JWT token stored in cookies
- Redirected to dashboard

---

### **Test 2: Projects Page** 🆕

**Purpose:** Test project listing and creation

```
1. Navigate to http://localhost:3000/projects
2. Expected: See "Projects" page
3. If no projects: See "No projects yet" message
4. Click "New Project" button
5. Fill form:
   - Name: "Test Project"
   - Description: "My first project"
6. Click "Create Project"
7. Expected:
   - Success toast
   - Redirect to project Kanban board
   - Project appears in projects list
```

**✅ Pass Criteria:**

- Projects list loads
- Create form appears on button click
- Project creation succeeds
- Redirect to project detail page
- New project visible in list

**🐛 Common Issues:**

- **401 Unauthorized**: JWT token missing/expired → Re-login
- **500 Error**: Backend service down → Check docker-compose ps
- **CORS Error**: API Gateway not configured → Check CORS settings

---

### **Test 3: Kanban Board** 🆕

**Purpose:** Test Kanban view with drag & drop

```
1. From projects list, click on a project
2. Expected: Kanban board with 4 columns:
   - To Do
   - In Progress
   - Done
   - Blocked
3. Click "New Task" button
4. Fill task form:
   - Title: "Implement login feature"
   - Description: "Add JWT authentication"
   - Priority: "High"
   - Status: "ToDo"
   - Due Date: (select future date)
5. Click "Create"
6. Expected: Task appears in "To Do" column
7. Drag task from "To Do" to "In Progress"
8. Expected:
   - Task moves visually
   - Status updates in backend
   - Success toast
```

**✅ Pass Criteria:**

- Kanban board renders with 4 columns
- Tasks display in correct columns
- Drag & drop works smoothly
- Status updates persist
- Task cards show: title, description, priority, due date

**🎨 UI Elements to Verify:**

- Task cards have colored priority badges
- Due dates show in human-readable format
- Hover effects on cards
- Loading states during drag
- Empty column states

---

### **Test 4: Task List View** 🆕

**Purpose:** Test alternative list view with filters

```
1. From Kanban board, click "List View" (or navigate to /projects/{id}/list)
2. Expected: Table/list view of all tasks
3. Test filters:
   - Filter by Status: "In Progress"
   - Expected: Only in-progress tasks shown
   - Filter by Priority: "High"
   - Expected: Only high-priority tasks shown
   - Clear filters
   - Expected: All tasks shown again
4. Click on a task row
5. Expected: Task detail modal opens
```

**✅ Pass Criteria:**

- List view displays all tasks
- Filters work correctly
- Status filter dropdown functional
- Priority filter dropdown functional
- Task click opens detail modal
- Sorting works (if implemented)

---

### **Test 5: Task Detail Modal** 🆕

**Purpose:** Test task details, comments, and file attachments

```
1. Click on any task (from Kanban or List view)
2. Expected: Modal opens with:
   - Task title and description
   - Priority and status badges
   - Due date
   - Comments section
   - File attachments section
   - Edit/Delete buttons (if implemented)
3. Scroll to comments section
4. Type a comment: "This looks good!"
5. Click "Add Comment"
6. Expected:
   - Comment appears immediately
   - Shows your name and timestamp
   - Success toast
7. Close modal
8. Reopen same task
9. Expected: Comment persists
```

**✅ Pass Criteria:**

- Modal displays all task information
- Comments section loads existing comments
- New comment submission works
- Comments show author and timestamp
- Modal closes properly
- Data persists after reopen

---

### **Test 6: File Upload** 🆕

**Purpose:** Test file upload and task attachments

```
1. Open task detail modal
2. Scroll to "Attachments" section
3. Click "Upload File" or drag & drop a file
4. Select a file (e.g., test.txt, image.png)
5. Expected:
   - Upload progress indicator
   - Success message
   - File appears in attachments list
6. Click "Attach to Task" (if separate step)
7. Expected:
   - File linked to task
   - File name, size, and type displayed
8. Click file name
9. Expected: File downloads or opens
10. Click "Remove" (if implemented)
11. Expected: Attachment removed from task
```

**✅ Pass Criteria:**

- File upload UI is intuitive
- Upload progress shown
- File appears after upload
- File can be attached to task
- File can be downloaded
- File can be removed
- Supports multiple file types

**📁 Test Files:**

- Text file (.txt)
- Image (.png, .jpg)
- Document (.pdf)
- Code file (.js, .ts)

**⚠️ Limits:**

- Max file size: 100 MB (per file-service config)

---

### **Test 7: AI Semantic Search** 🆕 (Optional - Requires Gemini API Key)

**Purpose:** Test vector similarity search

```
1. Create multiple tasks with different descriptions:
   - Task 1: "Implement user authentication with JWT"
   - Task 2: "Add login and registration forms"
   - Task 3: "Create database schema for products"
   - Task 4: "Design homepage layout"
2. Click "Search" or "AI Search" button
3. Enter query: "authentication and login"
4. Click "Search"
5. Expected:
   - Results ranked by similarity
   - Task 1 and Task 2 appear at top
   - Similarity scores shown (if implemented)
   - Task 3 and 4 ranked lower or not shown
```

**✅ Pass Criteria:**

- Search UI is accessible
- Query input works
- Results return quickly (< 3 seconds)
- Results are relevant to query
- Similarity ranking makes sense

**⚠️ Note:** Requires `GEMINI_API_KEY` environment variable set in project-service

---

### **Test 8: Responsive Design** 🎨

**Purpose:** Test UI on different screen sizes

```
1. Open browser DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M)
3. Test on:
   - Mobile (375px width)
   - Tablet (768px width)
   - Desktop (1920px width)
4. Verify:
   - Navigation menu adapts
   - Kanban board scrolls horizontally on mobile
   - Forms are usable on small screens
   - Buttons are touch-friendly
   - Text is readable
```

**✅ Pass Criteria:**

- UI adapts to screen size
- No horizontal scroll (except Kanban)
- Touch targets ≥ 44px
- Text remains readable
- Forms are usable

---

### **Test 9: Error Handling** 🐛

**Purpose:** Test error states and messages

```
1. Create task with empty title
   - Expected: Validation error "Title is required"
2. Try to create project while offline
   - Expected: Network error message
3. Upload file > 100 MB
   - Expected: "File too large" error
4. Try to access non-existent project
   - Expected: 404 page or error message
5. Let JWT expire (wait 15 minutes)
   - Expected: Auto-refresh or redirect to login
```

**✅ Pass Criteria:**

- Validation errors show inline
- Network errors show toast
- Error messages are clear
- No console errors (check DevTools)
- Graceful degradation

---

### **Test 10: Performance** ⚡

**Purpose:** Test loading times and responsiveness

```
1. Open browser DevTools → Network tab
2. Reload projects page
3. Check:
   - Initial load time < 2 seconds
   - API calls complete < 500ms
   - No unnecessary re-renders
4. Create 20 tasks in one project
5. Open Kanban board
6. Check:
   - Board renders smoothly
   - Drag & drop is responsive
   - No lag or stuttering
```

**✅ Pass Criteria:**

- Page load < 2 seconds
- API calls < 500ms
- Smooth animations
- No memory leaks
- Efficient re-renders

---

## 📊 **Test Results Checklist**

| Test | Feature           | Status | Notes                      |
| ---- | ----------------- | ------ | -------------------------- |
| 1    | Authentication    | ⬜     | Sprint 1 feature           |
| 2    | Projects List     | ⬜     | Create, Read               |
| 3    | Kanban Board      | ⬜     | Drag & drop                |
| 4    | Task List View    | ⬜     | Filters, sorting           |
| 5    | Task Detail Modal | ⬜     | View, edit                 |
| 6    | Comments          | ⬜     | Add, view                  |
| 7    | File Upload       | ⬜     | Upload, attach, download   |
| 8    | AI Search         | ⬜     | Semantic search (optional) |
| 9    | Responsive Design | ⬜     | Mobile, tablet, desktop    |
| 10   | Error Handling    | ⬜     | Validation, network errors |
| 11   | Performance       | ⬜     | Load times, responsiveness |

---

## 🐛 **Common Issues & Fixes**

### **Issue: "Cannot connect to API"**

**Symptoms:** Network errors, 500 responses

**Fix:**

```bash
# Check if services are running
docker-compose ps

# Restart services
docker-compose restart

# Check logs
docker logs spm-api-gateway -f
docker logs spm-project-service -f
```

### **Issue: "401 Unauthorized"**

**Symptoms:** Redirected to login, API calls fail

**Fix:**

```bash
# Clear cookies and re-login
# Or check JWT secret key matches across services
grep JWT_SECRET_KEY docker-compose.yml
```

### **Issue: "CORS Error"**

**Symptoms:** Browser console shows CORS error

**Fix:**

```bash
# Verify API Gateway CORS config
cat services/api-gateway/Program.cs | grep -A 5 "WithOrigins"

# Should include: http://localhost:3000
```

### **Issue: "Drag & Drop Not Working"**

**Symptoms:** Tasks don't move when dragged

**Fix:**

- Check browser console for errors
- Verify `react-beautiful-dnd` is installed
- Try refreshing the page
- Check if task status update API is working

### **Issue: "File Upload Fails"**

**Symptoms:** Upload progress stuck, error message

**Fix:**

```bash
# Check file-service logs
docker logs spm-file-service -f

# Verify file size < 100 MB
# Check storage volume is mounted
docker volume ls | grep file_storage
```

---

## 🎯 **Success Criteria**

Sprint 2 Frontend is considered **COMPLETE** when:

1. ✅ All 11 tests pass
2. ✅ No critical bugs found
3. ✅ UI is responsive on mobile/tablet/desktop
4. ✅ Error handling is graceful
5. ✅ Performance meets targets (< 2s load time)
6. ✅ All features work through API Gateway
7. ✅ No console errors in browser DevTools
8. ✅ Accessibility basics met (keyboard navigation, labels)

---

## 📝 **Bug Report Template**

If you find a bug, document it:

```markdown
**Bug Title:** [Short description]

**Steps to Reproduce:**

1. Go to...
2. Click on...
3. See error

**Expected Behavior:**
[What should happen]

**Actual Behavior:**
[What actually happens]

**Screenshots:**
[If applicable]

**Environment:**

- Browser: Chrome 120
- OS: macOS 14
- Frontend: localhost:3000
- API Gateway: localhost:5000

**Console Errors:**
[Copy from browser DevTools console]

**Severity:** Critical / High / Medium / Low
```

---

## 🚀 **Next Steps After Testing**

1. ✅ Document all test results
2. ✅ Fix critical bugs
3. ✅ Update IMPLEMENTATION_PLAN.md
4. ✅ Create Sprint 2 completion report
5. ⏭️ Move to Sprint 3 (Notification Service)

---

**Happy Testing! 🎉**

---

_Last Updated: January 2, 2026_
