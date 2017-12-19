using Dynacom.NutCache.Models.Interfaces;

namespace Dynacom.NutCache.Models
{
    public enum IntegrationCommitAction {
        Added = 1,
        Modified = 2,
        Removed = 3
    }
    public class IntegrationCommitFile : BaseEntity, IConcurrencyEntity{

        public IntegrationCommit IntegrationCommit { get; set; }
        public string FileName { get; set; }
        public string URL { get; set; }
        public IntegrationCommitAction Action { get; set; }
        public int Version{get;set;}
    }
}