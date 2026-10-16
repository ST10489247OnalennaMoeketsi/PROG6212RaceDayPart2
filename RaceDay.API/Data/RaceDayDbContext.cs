using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        // The constructor passes configuration options (like the connection string)
        // from Program.cs to the base DbContext class.
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        // DbSet represents a table in the database.
        // EF Core will create tables named after these properties.
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        // This method is where we configure the database schema using the Fluent API.
        // It overrides the default conventions EF Core would use.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // 1. USERS CONFIGURATION (Organisers and Participants)
            // =========================================================
            modelBuilder.Entity<User>(entity =>
            {
                // UQ_Users_Email: Email must be unique
                entity.HasIndex(e => e.Email).IsUnique();

                // CK_Users_Role: Role must be 'Organiser' or 'Participant'
                entity.ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser', 'Participant')"));
            });

            // =========================================================
            // 2. EVENTS CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Event>(entity =>
            {
                // FK_Events_Organiser: An Event belongs to one Organiser.
                // We use Restrict to prevent accidentally deleting an Organiser
                // who still has events linked to them.
                entity.HasOne(e => e.Organiser)
                      .WithMany()
                      .HasForeignKey(e => e.OrganiserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // CK_Events_Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Draft', 'Open', 'Closed', 'Completed')"));

                // CK_Events_RegistrationDeadline: Deadline must be before or on the EventDate
                entity.ToTable(t => t.HasCheckConstraint("CK_Events_RegistrationDeadline", "[RegistrationDeadline] <= [EventDate]"));
            });

            // =========================================================
            // 3. CATEGORIES CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Category>(entity =>
            {
                // UQ_Categories_Name: Category names must be unique
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // =========================================================
            // 4. EVENTCATEGORIES (Many-to-Many Join Table)
            // =========================================================
            modelBuilder.Entity<EventCategory>(entity =>
            {
                // Composite Primary Key: (EventId, CategoryId)
                entity.HasKey(ec => new { ec.EventId, ec.CategoryId });

                // Relationship to Event (Cascade delete: if Event is deleted, remove its category links)
                entity.HasOne(ec => ec.Event)
                      .WithMany(e => e.EventCategories)
                      .HasForeignKey(ec => ec.EventId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship to Category (Cascade delete: if Category is deleted, remove its event links)
                entity.HasOne(ec => ec.Category)
                      .WithMany(c => c.EventCategories)
                      .HasForeignKey(ec => ec.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================================================
            // 5. ENROLMENTS CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Enrolment>(entity =>
            {
                // UQ_Enrolments_Participant_Event: A participant can only enroll once per event
                entity.HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();

                // Simple FK: Enrolment -> Event
                entity.HasOne(e => e.Event)
                      .WithMany()
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Simple FK: Enrolment -> Category
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Composite FK: Enrolment -> EventCategory
                // This enforces the rule that the Event+Category combination MUST exist
                // in the EventCategories table (which means the Organiser has assigned it).
                entity.HasOne<EventCategory>()
                      .WithMany()
                      .HasForeignKey(e => new { e.EventId, e.CategoryId })
                      .HasPrincipalKey(ec => new { ec.EventId, ec.CategoryId })
                      .OnDelete(DeleteBehavior.Restrict);

                // CK_Enrolments_Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Active', 'Cancelled')"));
            });

            // =========================================================
            // 6. RESULTS CONFIGURATION
            // =========================================================
            modelBuilder.Entity<Result>(entity =>
            {
                // UQ_Results_Enrolment: One result per enrolment (One-to-One relationship)
                entity.HasIndex(e => e.EnrolmentId).IsUnique();

                // Relationship to Enrolment (Cascade delete: if Enrolment is deleted, remove the result)
                entity.HasOne(r => r.Enrolment)
                      .WithOne(e => e.Result)
                      .HasForeignKey<Result>(r => r.EnrolmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // CK_Results_Position: Position must be greater than 0 or null
                entity.ToTable(t => t.HasCheckConstraint("CK_Results_Position", "[Position] IS NULL OR [Position] > 0"));

                // CK_Results_Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Results_Status", "[ResultStatus] IN ('Finished', 'DNF', 'DNS', 'Disqualified')"));
            });
        }
    }
}
