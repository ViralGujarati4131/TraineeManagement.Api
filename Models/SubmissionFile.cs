using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.Submissions.Models;

namespace TraineeManagementApi.SubmissionFiles.Models;

    public class SubmissionFile : ICreateTimestamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id 
        { 
            get; 
            set; 
        }

        public int SubmissionId 
        { 
            get; 
            set; 
        }
         public Submission? Submission 
        { 
            get; 
            set; 
        }

        public string OriginalFileName 
        { 
            get; 
            set; 
        } = string.Empty;
        
        public string StorageFileName 
        { 
            get; 
            set; 
        } = string.Empty;

        public string ContentType 
        { 
            get; 
            set; 
        } = string.Empty;

        public long Size 
        { 
            get; 
            set; 
        }

        public string Checksum 
        { 
            get; 
            set; 
        } = string.Empty;

        public string UploadedByUserId 
        { 
            get; 
            set; 
        } = string.Empty;

        public DateTime CreatedDate
        { 
            get; 
            set; 
        }
    }
