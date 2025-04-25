using Microsoft.EntityFrameworkCore;
using NotesAPI.Models;

namespace NotesAPI.Data
{
    public class NoteContext : DbContext
    {
        public NoteContext(DbContextOptions<NoteContext> options) : base(options) { }
        public DbSet<Note> Notes {  get; set; }
    }
}
