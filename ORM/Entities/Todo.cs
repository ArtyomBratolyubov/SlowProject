using System.ComponentModel;

namespace ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Todo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        
        public bool IsCompleted { get; set; }

        //костыли
        [DefaultValue("false")]
        public bool IsSync { get; set; }
        [DefaultValue("false")]
        public bool IsIdSync { get; set; }
    }
}



