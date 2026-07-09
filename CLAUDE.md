# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build
```bash
dotnet build StableFit.sln
```

### Run tests
```bash
# All tests
dotnet test StableFit.sln

# Single test file
dotnet test tests/StableFit.UnitTests --filter "FullyQualifiedName~LoginCommandHandlerTests"

# With coverage (outputs opencover + cobertura to TestResults/)
dotnet test StableFit.sln --settings coverlet.runsettings --collect:"XPlat Code Coverage"
```

### Run the API locally
Requires Docker for PostgreSQL:
```bash
# Start only the database
docker compose up postgres -d

# Run the API
dotnet run --project src/StableFit.API
```
Swagger UI is available at `http://localhost:<port>/swagger` in Development only.

### Run everything via Docker
```bash
cp .env.example .env   # fill in JWT_SECRET at minimum
docker compose up --build
```
- API → `http://localhost:5225`
- Web (Blazor) → `http://localhost:5000`
- AI service → `http://localhost:8001`

### AI microservice (Python/FastAPI)
```bash
# Build and run only the AI service
docker compose build ai
docker compose up ai -d

# Health check
curl http://localhost:8001/health

# Analyse a video (exercise_type options: squat, pushup, plank, bicep_curl, lunge, pullup)
curl -X POST http://localhost:8001/analyze-exercise \
  -F "file=@video.mp4" \
  -F "exercise_type=squat"
```
Swagger UI for the AI service: `http://localhost:8001/docs`

### EF Core migrations
```bash
# Add migration (run from repo root)
dotnet ef migrations add <Name> --project src/StableFit.Infrastructure --startup-project src/StableFit.API

# Apply migrations manually
dotnet ef database update --project src/StableFit.Infrastructure --startup-project src/StableFit.API
```
In Development, migrations are applied automatically at startup (`MigrateAsync` in `Program.cs`).

---

## Architecture

### Layer structure (Clean Architecture)
```
Domain          → pure entities & enums, no dependencies
Application     → CQRS handlers, interfaces, DTOs, validators, scoring logic
Infrastructure  → EF Core, Identity, JWT, repositories, matching runtime
API             → controllers, GlobalExceptionHandler, CurrentUserService
Web             → Blazor WebAssembly frontend (MudBlazor)
tests/          → StableFit.UnitTests (xUnit + NSubstitute + FluentAssertions)
```

Dependency rule: `API → Application → Domain` and `Infrastructure → Application → Domain`. `Infrastructure` implements interfaces declared in `Application`. The `API` project only calls `ISender` (MediatR); controllers never touch EF Core directly.

### CQRS & MediatR split across two assemblies
Handlers are registered in **two** places:
- `Application/DependencyInjection.cs` — registers handlers in `StableFit.Application` (most commands/queries, validators, `ValidationBehavior` pipeline).
- `Infrastructure/DependencyInjection.cs` — registers handlers in `StableFit.Infrastructure` (matching queries/commands that need direct `DbContext` access: `GetMyRecommendationsQueryHandler`, `GetMyActiveMatchQueryHandler`, `SubmitDecisionCommandHandler`).

`IApplicationDbContext` (defined in `Application/Interfaces/`) is intentionally minimal — it only exposes `SaveChangesAsync`, no `DbSet<T>` properties. Application-layer handlers therefore **cannot query the database directly**; they must go through a repository interface (e.g. `IUserProfileRepository`) or be moved to Infrastructure. This is the root reason some handlers live in Infrastructure rather than Application.

When adding a new handler: if it only writes (via a repository or `IApplicationDbContext.SaveChangesAsync`) → Application; if it needs to query EF Core directly → Infrastructure.

### Matching system (Weeks 5-6 feature)
The matching flow involves four Infrastructure persistence entities (`MatchRun`, `MatchRecommendation`, `Match`, `MatchDecision`) that are not Domain entities — they live in `Infrastructure/Persistence/Entities/`.

Flow:
1. `POST /api/matching/run/ensure` → `EnsureMatchingRunCommand` → `MatchingRunBuilder.EnsureRunAndMaterializeRecommendationsAsync` scores all eligible users with `DefaultMatchScoringService` and writes a `MatchRun` + `MatchRecommendation` rows (TTL: configurable via `MatchingRunOptions.RunTtlHours`, default 24h).
2. `GET /api/matching/recommendations` → reads current user's pre-scored recommendations from the active run.
3. `POST /api/matching/decisions` → `SubmitDecisionCommand` → creates a `MatchDecision`; if both users liked each other, creates a `Match` with `Status = Active`.

`DefaultMatchScoringService` scores: shared `FitnessGoal` (50 pts), schedule day overlap (up to 40 pts), age closeness (up to 15 pts), weight closeness (up to 10 pts).

`MatchingRunOptions` defaults (configurable via the `MatchingRunOptions` section in `appsettings.json`):
- `TopK = 5` — recommendations stored per user per run
- `ActiveWithinDays = 14` — only users who logged in within this window are eligible
- `RunTtlHours = 24` — how long a run stays valid before a new one is created

### Authentication
JWT is stored in an `HttpOnly` cookie named `sf_access_token`. The token is read in `JwtBearerOptionsSetup.OnMessageReceived`. `ICurrentUserService` (implemented in `API/Infrastructure/CurrentUserService.cs`) extracts the `sub` claim using the short claim name `"sub"` — **not** `ClaimTypes.NameIdentifier` — because `MapInboundClaims = false` in `Infrastructure/DependencyInjection.cs`.

**Critical registration order:** `JwtBearerOptionsSetup` (which sets `IssuerSigningKey`) must be registered **after** `AddJwtAuthentication(services)` in `Infrastructure/DependencyInjection.cs`. The `AddJwtBearer` lambda replaces `options.TokenValidationParameters` with a new object; if `JwtBearerOptionsSetup` runs first (registered first), the signing key it sets is wiped out by the lambda. The fix: register the singleton after `AddJwtAuthentication` so it runs last and sets the key on the final object.

### Entity creation pattern
All entities — both `Domain/Entities/` and `Infrastructure/Persistence/Entities/` — must use the **Factory Method pattern**: private constructor(s) + one or more public static `Create(...)` methods that enforce invariants. Never instantiate entities with `new` from outside the class. The parameterless private constructor is kept only for EF Core/serializers. See `UserProfile.Create()` and `Match.CreateActive()` as reference.

DTOs and Commands/Queries must be `record` types with `{ get; init; }` properties.

### Identity vs Domain separation
`AppUser : IdentityUser` lives in `Infrastructure/Identity/` and holds auth concerns (`CreatedAt`, `LastLoginAt`). `UserProfile` in `Domain/Entities/` holds fitness data and links to `AppUser` via `UserProfile.UserId` (string FK). These two are never merged.

### Validation pipeline
`ValidationBehavior<TRequest, TResponse>` runs all `IValidator<TRequest>` implementations before any handler. Validators use FluentValidation and live alongside their command/query in the same feature folder. **Do not add validators for query types** (GET operations).

### Error handling
`GlobalExceptionHandler` (API layer) maps exceptions to `ProblemDetails`:
- `NotFoundException` → 404
- `ConflictException` → 409
- `UnauthorizedException` → 401
- `FluentValidation.ValidationException` → 400

### Database connection resolution
`Infrastructure/DependencyInjection.cs` prefers `POSTGRES_HOST/PORT/DB/USER/PASSWORD` env vars over `ConnectionStrings:StableFit` in `appsettings.json`. Docker Compose sets the env vars; local dev uses `appsettings.Development.json`.

### Testing conventions
- Framework: xUnit, NSubstitute (mocks), FluentAssertions, EF Core InMemory provider.
- Naming: `Given_When_Then` method names.
- Test classes implement `IDisposable` for cleanup.
- Tests mirror the `src/` folder structure under `tests/StableFit.UnitTests/`.

### AI integration (.NET side, Weeks 8-9)
The .NET API proxies exercise video to the AI microservice. Architecture:

```
Blazor (browser)
  → POST /api/exercise/analyze  (multipart: file + exerciseType)
      → ExerciseController (API)
          → AnalyzeExerciseCommand (Application/Exercise/Commands/)
              → IAiAnalysisService  (Application/Interfaces/)
                  → AiAnalysisService (Infrastructure/Services/) — typed HttpClient
                      → POST http://ai:8000/analyze-exercise
```

- `IAiAnalysisService` / `AiAnalysisService` — typed `HttpClient` registered via `AddHttpClient<IAiAnalysisService, AiAnalysisService>` in `Infrastructure/DependencyInjection.cs`. Base URL: `AI_SERVICE_URL` env var (Docker) or `AiService:BaseUrl` config (local dev, defaults to `http://localhost:8001`).
- `AnalyzeExerciseCommandHandler` lives in **Application** (no DbContext needed — only calls `IAiAnalysisService`).
- `ExerciseController` reads `IFormFile` into `byte[]`, limits: `[RequestSizeLimit(100MB)]` + `[RequestFormLimits(MultipartBodyLengthLimit=100MB)]`.
- `ExerciseAnalysisDto` intentionally omits `frame_angles` — too large for the frontend.
- Exercise.razor: MudSelect for type, InputFile (.mp4 only, 100 MB), results card with colour-coded score chip, rep count (hidden for plank), duration, feedback alerts.

### nginx proxy (Blazor Web, Weeks 8-9)
The Blazor web container proxies `/api/` through nginx to the API container, making all API calls same-origin from the browser's perspective. This avoids `SameSite` cookie issues and removes the need for CORS credential handling.

Key nginx settings (in `src/StableFit.Web/nginx.conf`):
- `client_max_body_size 150m` — allows video upload up to 150 MB
- `proxy_read_timeout 180s` / `proxy_send_timeout 180s` — covers AI analysis latency
- `proxy_pass http://api:8080/api/` — routes to the API container on Docker's internal network

In Docker (Production build), `wwwroot/appsettings.Production.json` sets `"ApiBaseUrl": ""`, which causes `Program.cs` to fall back to `builder.HostEnvironment.BaseAddress` (the web app's own origin). Local dev still uses `wwwroot/appsettings.json` with `"ApiBaseUrl": "http://localhost:5225/"`.

`@using Microsoft.AspNetCore.Authorization` is in `src/StableFit.Web/_Imports.razor` (global) — do not add it per-page.

### Gym Locator (Weeks 9-10)
Uses the **Overpass API** (OpenStreetMap) — no API key, no billing, no rate-limit concerns for dev traffic.

- `IOverpassService` / `OverpassService` — typed `HttpClient` registered in `Infrastructure/DependencyInjection.cs`. Base URL: `https://overpass-api.de/`. Timeout: 30 s. **Must set a `User-Agent` header** — Overpass blocks blank agents.
- Query uses `[out:json][timeout:25]` with `out center` so `way` elements (buildings) return a `center` object with lat/lon instead of a node list. Parse `element.Center` for ways, `element.Lat/Lon` for nodes.
- `GymsController` validates lat/lng range and radius (100–10 000 m) directly — no FluentValidation validator needed for simple numeric query params.
- `GetNearbyGymsQueryHandler` lives in **Application** (only calls `IOverpassService`, no DbContext).
- JS interop: `wwwroot/js/geolocation.js` exposes `window.getGeolocation()` returning a Promise. Called from Razor via `IJSRuntime.InvokeAsync<GeoLocation>("getGeolocation")`. Script tag added to `wwwroot/index.html`.
- Razor format specifiers (`:F1`, `:F4`) inside `@(...)` break the Razor parser — use `.ToString("F1")` instead.

### Preferred Gym & Matching Enhancement (Weeks 10-11)
Extends the Gym Locator so users can save a preferred gym to their profile and have it contribute to the matching score.

**Storage — denormalized on `UserProfile`** (no separate Gym entity — Overpass results are ephemeral OSM data):
- Four nullable columns added to `user_profiles`: `preferred_gym_name`, `preferred_gym_address`, `preferred_gym_latitude`, `preferred_gym_longitude`.
- Domain methods: `UserProfile.SetPreferredGym(name, address, lat, lon)` and `UserProfile.ClearPreferredGym()`.
- Migration: `AddPreferredGymToUserProfile`.

**CQRS commands** (both in Application — only use `IUserProfileRepository`, no DbContext queries):
- `SetPreferredGymCommand` + `SetPreferredGymCommandValidator` (validates name not empty, lat in [-90,90], lng in [-180,180]) + handler.
- `ClearPreferredGymCommand` + handler (no validator — parameterless delete).

**API endpoints** (added to `UserProfilesController`):
- `PUT /api/user-profiles/me/preferred-gym` — saves gym from request body.
- `DELETE /api/user-profiles/me/preferred-gym` — clears it.

**`UserProfileDto`** — four gym fields added; all five `MapToDto` call sites updated (`CreateUserProfile`, `GetMyUserProfile`, `GetUserProfileById`, `GetAllUserProfiles` handlers).

**Matching score** (`DefaultMatchScoringService`) — `ScoreGymProximity` added using Haversine distance (max +30 pts):
- ≤ 100 m → 30 pts (same gym)
- ≤ 1 km → 20 pts
- ≤ 5 km → 10 pts
- Otherwise → 0 pts
- Max total score is now **145 pts** (was 115).

**Frontend**:
- `Gyms.razor` — loads current preferred gym on init (`GET /api/user-profiles/me`); shows green "My Gym" chip on the matching card; all other cards show "Set as My Gym" button that PUTs to the API.
- `Profile.razor` — "Preferred Gym" section below the form; shows name + address + Clear button; links to `/gyms` when unset.

**ValidationBehavior fix** — MediatR 13 requires `where TRequest : notnull` on `IPipelineBehavior<TRequest, TResponse>` instead of `where TRequest : IRequest<TResponse>`. The old constraint caused `AddOpenBehavior` to skip all validators silently. Fixed in `src/StableFit.Application/Behaviors/ValidationBehavior.cs`.

### AI microservice architecture (`ai-service/`)
The AI service is a self-contained Python/FastAPI application — it has no dependency on the .NET projects.

```
ai-service/
├── Dockerfile
├── requirements.txt
└── app/
    ├── main.py                  # FastAPI app, CORS, router registration
    ├── api/routers.py           # GET /health, POST /analyze-exercise
    ├── models/pose_models.py    # Pydantic models: FrameAngles, ExerciseAnalysisResult
    ├── services/pose_service.py # MediaPipe Pose wrapper — extracts landmarks from video
    └── analysis/
        ├── __init__.py          # ANALYSERS registry: dict[str, BaseExerciseAnalyser]
        ├── base_analyser.py     # ABC with analyse(frames, frame_count, fps) interface
        ├── angle_utils.py       # calculate_angle(), torso_lean_angle() helpers
        ├── squat_analyser.py
        ├── pushup_analyser.py
        ├── plank_analyser.py
        ├── bicep_curl_analyser.py
        ├── lunge_analyser.py
        └── pullup_analyser.py
```

**Adding a new exercise:** create `app/analysis/<name>_analyser.py` inheriting `BaseExerciseAnalyser`, then add one entry to `ANALYSERS` in `app/analysis/__init__.py`. No other files need changing.

**Key design decisions:**
- `PoseService` initialises the MediaPipe `Pose` model once at startup (singleton) — model loading is expensive and must not happen per-request.
- `FrameAngles.angles` is `dict[str, float]` so each exercise reports its own angle names without changing the shared Pydantic schema.
- The `plank` analyser sets `rep_count = 0` always — it is a static hold scored by alignment consistency over time.
- `hip_offset` (positive = sagging, negative = raised) is stored in the angles dict for push-up and plank to distinguish the two failure modes without passing raw landmarks into the feedback functions.

### Frontend Visual Redesign & Auth-Aware Navigation
The Blazor Web frontend was fully redesigned with a dark emerald theme and a header-only navigation that adapts to authentication state.

**Color palette:**
- Background: `#051F20` (body radial gradient to `#0B2B26`)
- Surfaces/cards: `#163832`
- Borders/inactive: `#235347`
- Secondary text: `#8EB69B`
- Primary CTAs/headings: `#DAF1DE`

**Typography (locally hosted woff2, never CDN):**
- Syne 700/800 — headings (`h1`–`h6`, `.mud-typography-h*`)
- Plus Jakarta Sans 400/500/600/700 — body text
- JetBrains Mono 400 — `code`, `kbd`, `.stat-value`
- 13 woff2 files in `wwwroot/fonts/`, declared in `wwwroot/fonts/fonts.css`, linked from `index.html`

**Key CSS in `wwwroot/css/app.css`:**
- `body`: radial-gradient background, `min-height:100vh`
- `.mud-appbar`: `background: rgba(11,43,38,0.8)`, `backdrop-filter: blur(12px)`, `border-bottom: 1px solid #235347` — glassmorphism effect
- `.reveal-card`: stagger animation (`revealUp` keyframe, `animation-delay` set via `Style="@($"animation-delay: {index * 80}ms")"` in foreach loops)
- `.loading-progress`: `opacity: 0; animation: spinnerFadeIn 0.1s ease 0.25s forwards` — 250 ms delay before spinner appears

**Auth-Aware Header Navigation:**
The sidebar drawer (`MudDrawer`) was removed entirely. Navigation lives exclusively in the `MudAppBar`.

Key files:
- `Infrastructure/CookieAuthenticationStateProvider.cs` — custom `AuthenticationStateProvider`; calls `api/auth/me` once on startup (`_initialized` guard). Exposes `MarkLoggedIn(string username)` and `MarkLoggedOut()` which call `NotifyAuthenticationStateChanged`.
- `Shared/RedirectToLogin.razor` — calls `Nav.NavigateTo("/login")` in `OnInitialized`; used by `AuthorizeRouteView.NotAuthorized`.
- `App.razor` — `CascadingAuthenticationState` wrapping `Router` + `AuthorizeRouteView` (replaces plain `RouteView`). Unauthenticated visits to `[Authorize]` pages redirect via `RedirectToLogin`.
- `Program.cs` — registers `AddAuthorizationCore()`, `AddScoped<CookieAuthenticationStateProvider>()`, and `AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>())`.
- `_Imports.razor` — global `@using Microsoft.AspNetCore.Components.Authorization`, `@using StableFit.Web.Infrastructure`, `@using StableFit.Web.Shared`.
- `StableFit.Web.csproj` — `Microsoft.AspNetCore.Components.Authorization 10.0.5` package reference.
- `MainLayout.razor` — `<AuthorizeView>` in appbar: `<Authorized>` shows user dropdown, `<NotAuthorized>` shows Login + Get Started buttons. Logout calls `Http.PostAsync("api/auth/logout")` then `AuthProvider.MarkLoggedOut()`.
- `Login.razor` / `Register.razor` — after success: call `Http.GetFromJsonAsync<UserInfo>("api/auth/me")` then `AuthProvider.MarkLoggedIn(username)` then `NavigateTo("/profile")`.
- `Home.razor` — hero CTAs inside `<AuthorizeView>`: logged-in users see "My Profile" + "Find Matches"; guests see "Get Started" + "Login".
- `Layout/NavMenu.razor` and `NavMenu.razor.css` — **deleted** (no longer referenced).

**MudBlazor v9.4.0 — critical `MudMenu` gotcha:**
The `<ActivatorContent>/<ChildContent>` render-fragment pattern for `MudMenu` does NOT wire up the click handler correctly in v9.4.0 — the popover appears in the DOM at `opacity:0` with no content and never opens. Use the `Label=` / `EndIcon=` / direct child items API instead:
```razor
<MudMenu Label="@username" EndIcon="@Icons.Material.Filled.KeyboardArrowDown"
         Color="Color.Inherit" Variant="Variant.Text" Dense="true"
         AnchorOrigin="Origin.BottomLeft" TransformOrigin="Origin.TopLeft">
    <MudMenuItem Href="/profile">My Profile</MudMenuItem>
    ...
</MudMenu>
```

**CORS for local dev:**
`appsettings.Development.json` must include `http://localhost:5034` (Blazor dev server) and `https://localhost:7104` in `CorsOrigins`. Without this, all API calls from the web app are blocked by the browser — login appears to work because the page navigates, but `api/auth/me` fails silently and the auth state stays anonymous.

**Stagger animation pattern (Razor):**
```razor
@foreach (var (item, index) in list.Select((x, i) => (x, i)))
{
    <MudItem Class="reveal-card" Style="@($"animation-delay: {index * 80}ms")">
```
Use capital `Class=` and `Style=` — lowercase HTML attributes don't work on Blazor components.

### Nutrition Diary (Weeks 9-10)
Users log meals under four categories (Breakfast, Lunch, Dinner, Snack) for any day and track calories against a daily target.

```
Blazor (Nutrition.razor)
  → GET  /api/nutrition/search?query={q}       → SearchFoodQuery → INutritionApiService → Spoonacular
  → GET  /api/nutrition/food/{id}?grams={g}    → GetFoodNutritionQuery → INutritionApiService → Spoonacular
  → GET  /api/nutrition/diary?date={date}      → GetDiaryEntriesQuery → INutritionEntryRepository
  → POST /api/nutrition/diary                  → AddNutritionEntryCommand → INutritionEntryRepository
  → DELETE /api/nutrition/diary/{id}           → DeleteNutritionEntryCommand → INutritionEntryRepository
```

**Key design decisions:**
- `NutritionEntry` lives in `Domain/Entities/` (it is a domain concept, unlike `MatchRun` which lives in Infrastructure).
- `INutritionApiService` / `SpoonacularService` — typed `HttpClient` to `https://api.spoonacular.com/`, API key sent via `x-api-key` default request header. Key: `SPOONACULAR_API_KEY` env var (Docker) or `Spoonacular:ApiKey` config (local dev). **Critical:** always use `GetAsync` + `IsSuccessStatusCode` before calling `ReadFromJsonAsync` — `GetFromJsonAsync` throws on 401/402 (missing/rate-limited key) and surfaces as a 500. `SearchFoodsAsync` returns `[]` on failure; `GetFoodNutritionAsync` returns `null`.
- All three handlers (`AddNutritionEntry`, `DeleteNutritionEntry`, `GetDiaryEntries`) live in **Application** — they use `INutritionEntryRepository`, not DbContext directly.
- `NutritionEntryRepository` in Infrastructure; table `nutrition_entries`; `meal_type` stored as string; index on `(user_id, date)`.
- `UserProfile` extended with `HeightCm (int?)`, `BiologicalSex (BiologicalSex?)`, `DailyCalorieTarget (int?)`. Migration: `AddNutritionDiary` (covers both the new `nutrition_entries` table and these 3 new profile columns).
- Calorie auto-calculation is **client-side** in `Profile.razor` using Mifflin-St Jeor × 1.55 activity factor:
  - Male: `(10W + 6.25H − 5A + 5) × 1.55`
  - Female: `(10W + 6.25H − 5A − 161) × 1.55`
  - PreferNotToSay: average offset (−78) × 1.55
- `BiologicalSex` and `MealType` enums exist in both `Domain/Enums/` and `StableFit.Web/Models/NutritionModels.cs` (Web has local copies — same pattern as `FitnessGoal`).
- `Nutrition.razor` — `@page "/nutrition"`, `[Authorize]`; date navigation, 4 meal cards in `MudGrid` (2×2), `MudProgressLinear` calorie bar, inline food-search → select → set grams → add flow per card.
