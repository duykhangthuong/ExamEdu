using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class Class
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime StartDay { get; set; } 
        public DateTime EndDay { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        // Many-Many module
        public ICollection<Module> Modules { get; set; }
        public ICollection<ClassModule> ClassModules { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}