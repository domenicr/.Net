using Dynacom.NutCache.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dynacom.NutCache.Models
{
    public class IntegrationCommit : BaseEntity, IConcurrencyEntity {

        public IntegrationCommit() {
            IntegrationCommitFiles = new Collection<IntegrationCommitFile>();
            IntegrationCommitProjectBoardCards = new Collection<IntegrationCommitProjectBoardCard>();
        }

        public ProjectIntegration ProjectIntegration { get; set; }
        public string Repository { get; set; }
        public string URL { get; set; }
        public string Revision { get; set; }
        public string Comment { get; set; }
        public string Author { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorUserName { get; set; }
        public DateTime CommitUTCDate { get; set; }
        public int Version { get; set; }
        public ICollection<IntegrationCommitFile> IntegrationCommitFiles { get; set; }

        [IdentifyingRelationship]
        public ICollection<IntegrationCommitProjectBoardCard> IntegrationCommitProjectBoardCards { get; set; }


    }
}