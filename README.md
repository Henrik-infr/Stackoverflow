# Stack Overflow Clone

A full-featured Stack Overflow clone built with ASP.NET Core 8, Dapper, and Razor Pages.

## Technology Stack

- **ASP.NET Core 8** - Web framework
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

## License

This project is for educational purposes.
