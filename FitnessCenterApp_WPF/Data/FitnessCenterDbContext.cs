using Microsoft.EntityFrameworkCore;
using FitnessCenterApp.Models;

namespace FitnessCenterApp.Data;

public class FitnessCenterDbContext : DbContext
{
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Trainer> Trainers { get; set; } = null!;
    public DbSet<RateSubscription> RateSubscriptions { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<Training> Trainings { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<ScheduleTrainer> ScheduleTrainers { get; set; } = null!;
    public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; } = null!;
    public DbSet<ClientRecordsTraining> ClientRecords { get; set; } = null!;
    
    public FitnessCenterDbContext(DbContextOptions<FitnessCenterDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdClients);
            entity.Property(e => e.IdClients).HasColumnName("ID_Clients");
            entity.Property(e => e.Firstname).HasColumnName("Firstname").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Lastname).HasColumnName("Lastname").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Password).HasColumnName("Password").HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.PhoneNumber).HasColumnName("Phone_number").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.DateOfBirth).HasColumnName("Date_of_birth").HasColumnType("date");
            entity.Property(e => e.Gender).HasColumnName("Gender").HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Weight).HasColumnName("Weight").HasColumnType("decimal(5,2)");
            entity.Property(e => e.Height).HasColumnName("Height").HasColumnType("decimal(5,2)");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.ToTable("Trainer", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdTrainer);
            entity.Property(e => e.IdTrainer).HasColumnName("ID_Trainer");
            entity.Property(e => e.Firstname).HasColumnName("Firstname").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Lastname).HasColumnName("Lastname").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Specialization).HasColumnName("Specialization").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Password).HasColumnName("Password").HasMaxLength(255).IsUnicode(false);
        });

        modelBuilder.Entity<RateSubscription>(entity =>
        {
            entity.ToTable("Rate_Subscription", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdRateSubscription);
            entity.Property(e => e.IdRateSubscription).HasColumnName("ID_Rate_Subscription");
            entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Type).HasColumnName("Type").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Cost).HasColumnName("Cost").HasColumnType("decimal(10,2)");
            entity.Property(e => e.NumberOfVisits).HasColumnName("Number_of_visits");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscription", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdSubscription);
            entity.Property(e => e.IdSubscription).HasColumnName("ID_Subscription");
            entity.Property(e => e.IdRateSubscription).HasColumnName("ID_Rate_Subscription");
            entity.Property(e => e.IdClients).HasColumnName("ID_Clients");
            entity.Property(e => e.StartDate).HasColumnName("Start_date").HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnName("End_date").HasColumnType("date");
            entity.HasOne(e => e.RateSubscription)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.IdRateSubscription);
            entity.HasOne(e => e.Client)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.IdClients);
        });

        modelBuilder.Entity<Training>(entity =>
        {
            entity.ToTable("Training", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdTraining);
            entity.Property(e => e.IdTraining).HasColumnName("ID_Training");
            entity.Property(e => e.TrainingName).HasColumnName("Training_name").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Description).HasColumnName("Description").HasColumnType("text").IsUnicode(false);
            entity.Property(e => e.Category).HasColumnName("Category").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Intensity).HasColumnName("Intensity").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MaxParticipants).HasColumnName("Max_participants");
            entity.Property(e => e.Place).HasColumnName("Place").HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedule", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdSchedule);
            entity.Property(e => e.IdSchedule).HasColumnName("ID_Schedule");
            entity.Property(e => e.IdTraining).HasColumnName("ID_Training");
            entity.Property(e => e.TrainingDate).HasColumnName("Training_date").HasColumnType("date");
            entity.Property(e => e.StartTime).HasColumnName("Start_time").HasColumnType("time");
            entity.Property(e => e.EndTime).HasColumnName("End_time").HasColumnType("time");
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(20).IsUnicode(false);
            entity.HasOne(e => e.Training)
                .WithMany(e => e.Schedules)
                .HasForeignKey(e => e.IdTraining);
        });

        modelBuilder.Entity<ScheduleTrainer>(entity =>
        {
            entity.ToTable("Schedule_Trainer", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdScheduleTrainer);
            entity.Property(e => e.IdScheduleTrainer).HasColumnName("ID_Schedule_Trainer");
            entity.Property(e => e.IdSchedule).HasColumnName("ID_Schedule");
            entity.Property(e => e.IdTrainer).HasColumnName("ID_Trainer");
            entity.HasIndex(e => new { e.IdSchedule, e.IdTrainer }).IsUnique();
            entity.HasOne(e => e.Schedule)
                .WithMany(e => e.ScheduleTrainers)
                .HasForeignKey(e => e.IdSchedule);
            entity.HasOne(e => e.Trainer)
                .WithMany(e => e.ScheduleTrainers)
                .HasForeignKey(e => e.IdTrainer);
        });

        modelBuilder.Entity<TrainerAvailability>(entity =>
        {
            entity.ToTable("Trainer_Availability", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdTrainerAvailability);
            entity.Property(e => e.IdTrainerAvailability).HasColumnName("ID_Trainer_Availability");
            entity.Property(e => e.IdTrainer).HasColumnName("ID_Trainer");
            entity.Property(e => e.Date).HasColumnName("Date").HasColumnType("date");
            entity.Property(e => e.WorkStartTime).HasColumnName("Work_start_time").HasColumnType("time");
            entity.Property(e => e.WorkEndTime).HasColumnName("Work_end_time").HasColumnType("time");
            entity.HasOne(e => e.Trainer)
                .WithMany(e => e.Availabilities)
                .HasForeignKey(e => e.IdTrainer);
        });

        modelBuilder.Entity<ClientRecordsTraining>(entity =>
        {
            entity.ToTable("Client_Records_Training", tb => tb.UseSqlOutputClause(false));
            entity.HasKey(e => e.IdClientRecordsTraining);
            entity.Property(e => e.IdClientRecordsTraining).HasColumnName("ID_Client_Records_Training");
            entity.Property(e => e.IdTraining).HasColumnName("ID_Training");
            entity.Property(e => e.IdClients).HasColumnName("ID_Clients");
            entity.Property(e => e.RecordDate).HasColumnName("Record_date").HasColumnType("datetime");
            entity.Property(e => e.Presence).HasColumnName("Presence").HasMaxLength(20).IsUnicode(false);
            entity.HasIndex(e => new { e.IdTraining, e.IdClients }).IsUnique();
            entity.HasOne(e => e.Training)
                .WithMany(e => e.ClientRecords)
                .HasForeignKey(e => e.IdTraining);
            entity.HasOne(e => e.Client)
                .WithMany(e => e.ClientRecords)
                .HasForeignKey(e => e.IdClients);
        });
    }
}
