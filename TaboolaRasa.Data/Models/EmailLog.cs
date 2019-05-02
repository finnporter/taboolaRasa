using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TaboolaRasa.Models.Enums;

namespace TaboolaRasa.Data.Models
{
    public partial class EmailLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public CommunicationReason Reason { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        public string RelatedId { get; set; }

        [Required]
        public DateTime SentOn { get; set; }
    }
}
