using Dynacom.NutCache.Models.Interfaces;

namespace Dynacom.NutCache.Models {
    public class IntegrationCommitProjectBoardCard : BaseEntity, IConcurrencyEntity {

        public IntegrationCommit IntegrationCommit { get; set; }
        public ProjectBoardCard ProjectBoardCard { get; set; }
        public int Version { get; set; }
    }
}