using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicManagementSystem.Models
{
    // ====================================
    // Medical Document Model
    // ====================================
    public class MedicalDocument : BaseModel
    {
        private int _documentID;
        private int _patientID;
        private string _documentType;
        private string _title;
        private string _description;
        private string _filePath;
        private long _fileSize;
        private DateTime _uploadDate;

        public int DocumentID
        {
            get => _documentID;
            set => SetProperty(ref _documentID, value);
        }

        public int PatientID
        {
            get => _patientID;
            set => SetProperty(ref _patientID, value);
        }

        public string DocumentType
        {
            get => _documentType;
            set => SetProperty(ref _documentType, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public long FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        public DateTime UploadDate
        {
            get => _uploadDate;
            set => SetProperty(ref _uploadDate, value);
        }
    }

}
