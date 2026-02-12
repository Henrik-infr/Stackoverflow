# Stack Overflow Clone

A full-featured Stack Overflow clone built with ASP.NET Core 10, Dapper, and Razor Pages.

## Technology Stack

- **ASP.NET Core 10** - Web framework
- **Dapper** - Micro-ORM (same as Stack Overflow uses)
- **Razor Pages** - Server-rendered UI
- **SQL Server** - Database
- **Docker** - Containerization

## Features

- **Questions & Answers** - Full Q&A functionality with voting
- **Comments** - Comment on questions and answers
- **Voting System** - Upvote/downvote posts
- **Tags** - Categorize questions with tags
- **User Profiles** - View user statistics and activity
- **Badges** - Display user achievements
- **Search** - Full-text search across questions
- **Pagination** - Browse through large result sets

## Project Structure

```
stackoverflow/
├── src/
│   └── StackOverflow.Web/
│       ├── Data/              # Database access layer
│       │   └── Repositories/  # Dapper repositories
│       ├── Models/            # Domain models
│       │   └── ViewModels/    # Page view models
│       ├── Pages/             # Razor Pages
│       ├── Services/          # Business logic services
│       └── wwwroot/           # Static files (CSS, JS)
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (or use Docker)
- Docker (optional, for containerized deployment)

### Local Development

1. Clone the repository
2. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "StackOverflow": "Server=localhost;Database=StackOverflow;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
     }
   }
   ```
3. Run the application:
   ```bash
   cd src/StackOverflow.Web
   dotnet run
   ```
4. Open http://localhost:5000 in your browser

### Docker Deployment

Build and run with Docker Compose:

```bash
docker-compose up --build
```

Or build just the web application:

```bash
docker build -t stackoverflow .
docker run -p 5000:8080 -e "ConnectionStrings__StackOverflow=YOUR_CONNECTION_STRING" stackoverflow
```

## Database Schema

This application uses the standard Stack Exchange data schema:

- **Posts** - Questions and answers (linked via ParentId)
- **Users** - User profiles with reputation
- **Comments** - Post comments
- **Votes** - Upvotes, downvotes, favorites
- **Tags** - Question categorization
- **Badges** - User achievements
- **PostHistory** - Edit history
- **PostLinks** - Related/duplicate questions

## Configuration

### Environment Variables

- `ConnectionStrings__StackOverflow` - SQL Server connection string
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)

### Health Check

The application exposes a health check endpoint at `/health` for container orchestration.

## API Endpoints

The application is primarily server-rendered with Razor Pages:

- `/` - Home page with recent questions
- `/Questions` - Browse all questions
- `/Questions/Ask` - Ask a new question
- `/Questions/Details/{id}` - View question with answers
- `/Tags` - Browse all tags
- `/Tags/Details/{tagName}` - Questions by tag
- `/Users` - User directory
- `/Users/Profile/{id}` - User profile
- `/Search` - Search questions

## Development Notes

- The application uses Dapper for data access, providing lightweight and performant database operations
- All database operations are asynchronous
- Error handling includes graceful fallbacks when the database is unavailable
- The UI is inspired by Stack Overflow's design

## Deploying to Intility Developer Platform (Argo CD)

### Prerequisites

- Access to Intility Developer Platform
- Container registry access (to push Docker images)
- SQL Server database with Stack Exchange schema

### Step 1: Build and Push Docker Image

```bash
# Build the image
docker build -t your-registry.intility.com/stackoverflow:latest .

# Push to registry
docker push your-registry.intility.com/stackoverflow:latest
```

### Step 2: Configure Kubernetes Manifests

1. Update `k8s/secret.yaml` with your SQL Server connection string:
   ```yaml
   stringData:
     connection-string: "Server=your-server;Database=StackOverflow;User Id=user;Password=pass;TrustServerCertificate=true;"
   ```

2. Update `k8s/ingress.yaml` with your domain:
   ```yaml
   spec:
     rules:
       - host: stackoverflow.apps.intility.com
   ```

3. Update `k8s/kustomization.yaml` with your container registry:
   ```yaml
   images:
     - name: stackoverflow
       newName: your-registry.intility.com/stackoverflow
       newTag: latest
   ```

4. (Optional) Configure external database route in `k8s/external-db-service.yaml`

### Step 3: Deploy with Argo CD

**Option A: Using Argo CD Application manifest**

1. Update `k8s/argocd-application.yaml` with your Git repository URL
2. Apply the Application:
   ```bash
   kubectl apply -f k8s/argocd-application.yaml
   ```

**Option B: Using Argo CD CLI**

```bash
argocd app create stackoverflow \
  --repo https://github.com/YOUR_ORG/stackoverflow.git \
  --path k8s \
  --dest-server https://kubernetes.default.svc \
  --dest-namespace stackoverflow \
  --sync-policy automated
```

**Option C: Using Argo CD UI**

1. Navigate to Argo CD UI
2. Click "New App"
3. Fill in:
   - Application Name: `stackoverflow`
   - Project: `default`
   - Repository URL: Your Git repo
   - Path: `k8s`
   - Cluster: Your target cluster
   - Namespace: `stackoverflow`
4. Click "Create"

### Step 4: Verify Deployment

```bash
# Check pods
kubectl get pods -n stackoverflow

# Check service
kubectl get svc -n stackoverflow

# Check ingress
kubectl get ingress -n stackoverflow

# View logs
kubectl logs -l app=stackoverflow-web -n stackoverflow
```

### Kubernetes Resources

| File | Description |
|------|-------------|
| `k8s/namespace.yaml` | Namespace definition |
| `k8s/deployment.yaml` | Web app deployment (2 replicas) |
| `k8s/service.yaml` | ClusterIP service |
| `k8s/ingress.yaml` | Ingress for external access |
| `k8s/secret.yaml` | Database connection string |
| `k8s/external-db-service.yaml` | External database service (optional) |
| `k8s/kustomization.yaml` | Kustomize configuration |
| `k8s/argocd-application.yaml` | Argo CD Application manifest |

### Connecting to External SQL Server

If your SQL Server is outside the cluster, you have two options:

1. **ExternalName Service** (for DNS-resolvable hosts):
   ```yaml
   spec:
     type: ExternalName
     externalName: your-sqlserver.database.windows.net
   ```

2. **Static IP Endpoints** (for IP addresses):
   See commented section in `k8s/external-db-service.yaml`

## License

This project is for educational purposes.
