using System;
using System.Runtime.Serialization;

namespace Benchmark.Models
{
    public class Account
    {
        internal long Id { get; set; }

        /// <summary>
        /// The reference code of the user/account (example: PR553J9).
        /// </summary>
        public string ReferenceCode { get; set; }
    }

    public class GeocacheActivity
    {
        internal long Id { get; set; }

        /// <summary>
        /// Unique code used to reference the geocache activity.
        /// </summary>
        public string ReferenceCode { get; set; }

        /// <summary>
        /// Owner of the activity (formerly known as Finder).
        /// </summary>
        public Account ActivityOwner { get; set; }

        /// <summary>
        /// Date/time this activity was created (in UTC).
        /// </summary>
        public DateTime DateTimeCreatedUtc { get; set; }

        /// <summary>
        /// Date/time this activity is stated to have taken place (formerly known as VisitDate).
        /// </summary>
        public DateTime ActivityDate { get; set; }

        /// <summary>
        /// Whether this activity is archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Activity text (formerly known as LogText).
        /// </summary>
        public string ActivityText { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity (formerly known as LogType).
        /// </summary>
        public GeocacheActivityType ActivityType { get; set; }

        public double? UpdatedLatitude { get; set; }

        public double? UpdatedLongitude { get; set; }

        /// <summary>
        /// Whether the ActivityText:
        ///     has been encoded via the ROT-13 cipher (on reads).
        ///     should be encoded via the ROT-13 cipher (on writes).
        /// </summary>
        public bool IsTextRot13 { get; set; }

        internal int ActivityTypeID { get; set; }

        public bool IsApproved { get; set; }

        public bool CannotDelete { get; set; }
        
        /// <summary>
        /// Geocache this activity applies to (example: GC25).
        /// </summary>
        public Geocache Geocache { get; set; }

        public GeocacheActivity()
        {
        }
    }

    public enum GeocacheActivityType : int
    {
        /// <summary>
        /// Unarchive
        /// </summary>
        [EnumMember(Value = "Unarchive")]
        Unarchive = 1,

        /// <summary>
        /// Found It
        /// </summary>
        [EnumMember(Value = "Found It")]
        FoundIt = 2,

        /// <summary>
        /// Did not find it
        /// </summary>
        [EnumMember(Value = "Did not find it")]
        DidntFindIt = 3,

        /// <summary>
        /// Write note
        /// </summary>
        [EnumMember(Value = "Write note")]
        WriteNote = 4,

        /// <summary>
        /// Archive (Visible)
        /// </summary>
        [EnumMember(Value = "Archive (Visible)")]
        ArchiveShow = 5,

        /// <summary>
        /// Archive (Hidden)
        /// </summary>
        [EnumMember(Value = "Archive (Hidden)")]
        ArchiveNoShow = 6,

        /// <summary>
        /// Needs archiving
        /// </summary>
        [EnumMember(Value = "Needs archiving")]
        NeedsArchive = 7,

        /// <summary>
        /// Mark as destroyed
        /// </summary>
        [EnumMember(Value = "Mark as destroyed")]
        MarkDestroyed = 8,

        /// <summary>
        /// Will attend
        /// </summary>
        [EnumMember(Value = "Will attend")]
        WillAttend = 9,

        /// <summary>
        /// Attended
        /// </summary>
        [EnumMember(Value = "Attended")]
        Attended = 10,

        /// <summary>
        /// Webcam photo taken
        /// </summary>
        [EnumMember(Value = "Webcam photo taken")]
        WebcamPhotoTaken = 11,

        /// <summary>
        /// Visited
        /// </summary>
        [EnumMember(Value = "Visited")]
        Visited = 75
    }

    public class Geocache
    {
        internal long Id { get; set; }

        /// <summary>
        /// The reference code of the geocache (example: GC25).
        /// </summary>
        public string ReferenceCode { get; set; }
    }
}