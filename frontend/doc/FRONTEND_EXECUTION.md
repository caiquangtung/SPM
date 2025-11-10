# Frontend Execution Plan (Sprint 1 → Sprint 2)

## Scope

- Implement core auth flows and minimal UI per Sprint 1.
- Prepare foundations for Sprint 2 (projects/tasks) without over-engineering.

## Tech Stack

- Next.js 14 (App Router) + TypeScript
- Tailwind CSS + shadcn/ui (later)
- Axios + Interceptors
- TanStack Query (React Query)
- react-hook-form + zod
- Toast: sonner
- Icons: lucide-react
- Date: dayjs

## Directory Structure (current)

```
frontend/
  app/
    login/page.tsx
    register/page.tsx
    verify-email/[token]/page.tsx
    profile/page.tsx
    dashboard/page.tsx
    layout.tsx
    globals.css
  components/
    ProtectedRoute.tsx
  contexts/
    AuthContext.tsx
  hooks/
  lib/
    api.ts
    auth.ts
    queryClient.ts
    validators/
      auth.ts
  types/
```

## API Contracts (dependent on user-service)

- POST `/api/auth/register` → { message, userId, verificationToken }
- POST `/api/auth/login` → { accessToken, refreshToken, expiresAt, user }
- POST `/api/auth/verify-email` → { message }
- POST `/api/auth/refresh` → { accessToken, refreshToken, expiresAt, user }
- user.role: string ("Admin"|"PM"|"Member")

## Auth Strategy

- Access token: memory (via AuthContext) + attached by Axios.
- Refresh token: httpOnly cookie (server-controlled). Client sends `/auth/refresh` on 401 and retries once.
- Private pages guarded with `ProtectedRoute`.

## Implementation Tasks (Sprint 1)

1. Foundation

- Ensure Tailwind/ESLint/Prettier set; environment `NEXT_PUBLIC_API_URL`.
- Add React Query provider (`lib/queryClient.ts`, app/layout.tsx).

2. API & Interceptors

- `lib/api.ts`: baseURL from env, request attach `Authorization`, response 401 → refresh → retry.
- `lib/auth.ts`: login/register/verify/refresh helpers; cookie storage for tokens.

3. Auth Context & Guard

- `contexts/AuthContext.tsx`: holds `user`, `isAuthenticated`, `login`, `logout`, `register`.
- `components/ProtectedRoute.tsx`: redirect unauthenticated to `/login`.

4. Forms + Validation

- `lib/validators/auth.ts`: zod schemas (login/register/verify).
- `/login`: react-hook-form + zodResolver + sonner; on success → `/dashboard`.
- `/register`: react-hook-form + zodResolver (+ confirm password refine); success toast, redirect `/login`.
- `/verify-email/[token]`: validate token with zod; success/fail toast; redirect `/login`.

5. Pages

- `/profile`: show basic user info from context; logout.
- `/dashboard`: placeholder after login.

## Acceptance Criteria (Sprint 1)

- Flows: register → verify → login → refresh (auto on 401) → profile → logout.
- `ProtectedRoute` blocks `/profile` and `/dashboard` when unauthenticated.
- Axios attaches token; refresh-on-401 works and retries once.
- No console errors; lint passes; responsive basics.

## Commands

```bash
# Install deps (if missing)
cd frontend
npm i @tanstack/react-query axios js-cookie react-hook-form zod @hookform/resolvers sonner dayjs lucide-react

# Dev
npm run dev
```

## Risks & Notes

- Ensure backend CORS allows frontend origin.
- Time in UTC; handle ISO strings on client side.
- Consider adding a lightweight `me` endpoint to hydrate user on mount.

## Sprint 2 Preview (Project & Tasks)

- Pages: `/projects`, `/projects/[id]` (Kanban), `/projects/[id]/list`.
- Components (later): `KanbanBoard`, `TaskCard`, `TaskForm`, `CommentSection`, `FileUpload`.
- State: prefer React Query; only consider Redux if DnD + real-time + optimistic updates become complex.

## Task Breakdown (Sprint 1)

- [x] 1. Foundation & Configuration (0.5d)

  - [x] Verify Tailwind, ESLint/Prettier, Husky/lint-staged baseline
  - [x] Ensure `NEXT_PUBLIC_API_URL` configured for environments
  - [x] Add React Query provider (done): `lib/queryClient.ts`, wrap in `app/layout.tsx`
  - Deliverables: working dev server, no console errors

- [x] 2. API Client & Interceptors (0.5d)

  - [x] Configure axios baseURL from env: `lib/api.ts`
  - [x] Request interceptor: attach `Authorization: Bearer <accessToken>` if present
  - [x] Response interceptor: on 401, call `/api/auth/refresh` then retry once
  - [x] Token storage policy: access (cookie+memory), refresh (httpOnly cookie - server)
  - Deliverables: manual 401 test refreshes token and retries successfully

- [x] 3. Auth Context & Session (0.5d)

  - [x] Implement `AuthContext` with user state and helpers: `login`, `logout`, `register`, `refresh`
  - [x] `ProtectedRoute` to guard private pages
  - [x] Optional: add `loadSession()` hydration via refresh token (implemented)
  - Deliverables: `/dashboard`, `/profile` redirect to `/login` when unauthenticated

- [x] 4. Forms & Validation (0.5d)

  - [x] Zod schemas: `lib/validators/auth.ts` (login, register+confirm, verify token)
  - [x] Integrate zodResolver with react-hook-form in `/login` and `/register`
  - [x] Replace toasts with `sonner` across auth pages
  - Deliverables: client-side validation messages, disabled submit on pending, success/error toasts

- [x] 5. Pages Implementation (0.5d)

  - [x] `/register`: form (email, password, confirm, fullName) → call register → success toast → `/login`
  - [x] `/login`: form (email, password) → call login → set tokens → `/dashboard`
  - [x] `/verify-email/[token]`: validate token → call verify → toast → redirect `/login`
  - [x] `/profile`: display user info; logout button
  - [x] `/dashboard`: placeholder after login
  - Deliverables: E2E happy-path flows working against user-service

- [ ] 6. QA & Hardening (0.5d)
  - [ ] Smoke test flows: register → verify → login → refresh-on-401 → profile → logout
  - [x] Handle loading and error states consistently
  - [x] Lint/type-check clean; responsive basics OK
  - Deliverables: checklist signed off; demo run

## Owners & Deliverables

- **Owner**: Frontend (You)
- **Dependencies**: user-service auth endpoints live; CORS configured; DTO role is string
- **Deliverables**: All Sprint 1 frontend routes/components completed per Acceptance Criteria

## Execution Checklist Mapping → Files

- Config
  - [x] `frontend/app/layout.tsx` (QueryClientProvider, Toaster, AuthProvider)
  - [x] `frontend/lib/queryClient.ts`
  - [x] `frontend/lib/api.ts`
- Auth
  - [x] `frontend/contexts/AuthContext.tsx`
  - [x] `frontend/components/ProtectedRoute.tsx`
- Validation
  - [x] `frontend/lib/validators/auth.ts`
- Pages
  - [x] `frontend/app/register/page.tsx`
  - [x] `frontend/app/login/page.tsx`
  - [x] `frontend/app/verify-email/[token]/page.tsx`
  - [x] `frontend/app/profile/page.tsx`
  - [x] `frontend/app/dashboard/page.tsx`

## Milestones

- [x] M1: Foundation + API client ready
- [x] M2: Auth context + Protected routes wired
- [x] M3: Auth pages implemented with validation
- [x] M4: Verify email flow implemented
- [ ] M5: QA completed; Sprint 1 DoD met
