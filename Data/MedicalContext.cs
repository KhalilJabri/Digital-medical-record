using Microsoft.EntityFrameworkCore;
using DigitalMedicalRecord.Models;

namespace DigitalMedicalRecord.Data
{
    public class MedicalContext : DbContext
    {
        public MedicalContext(DbContextOptions<MedicalContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
    }
}
