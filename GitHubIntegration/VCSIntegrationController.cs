using Dynacom.NutCache.DataAccessLayer.Repositories;
using Dynacom.NutCache.Models;
using Dynacom.NutCache.PresentationLayer.Mappers.Interfaces;
using Dynacom.NutCache.ServiceLayer;
using Dynacom.NutCache.Support;
using GNaP.WebApi.Versioning;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Net;
using Dynacom.NutCache.Helpers;
using Dynacom.NutCache.SignalR;
using Dynacom.NutCache.Support.ApiActionFilterAttributes;

namespace Dynacom.NutCache.ApiControllers.Version1{
    [RoutePrefix("webapi/vcsintegration")]
    public class VCSIntegrationVersion1Controller : BaseApiController {
        internal const string GIT_HUB_SIGNATURE_HEADER_KEY = "sha1";
        internal const string GIT_HUB_SIGNATURE_HEADER_VALUE_TEMPLATE = GIT_HUB_SIGNATURE_HEADER_KEY + "={0}";
        internal const string GIT_HUB_SIGNATURE_HEADER_NAME = "X-Hub-Signature";
        internal const string GIT_HUB_EVENT_HEADER_NAME = "X-Github-Event";

        /// <summary>
        /// This constructor is called while in normal execution (versus unit testing).
        /// It uses the second constructor which is already present for unit testing scenarios.
        /// </summary>

        public VCSIntegrationVersion1Controller() : this(null, null, null, null, null) { }

        /// <summary>
        /// This constructor can be called in the unit tests.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="repositoryFactory"></param>
        /// <param name="serviceLayerFactory"></param>
        /// <param name="mapperFactory"></param>
        /// <param name="signalREndPointFactory"></param>
        public VCSIntegrationVersion1Controller(IApplicationContext context, IRepositoryFactory repositoryFactory, IServiceLayerFactory serviceLayerFactory, IMapperFactory mapperFactory, ISignalREndPointFactory signalREndPointFactory)
            : base(context, repositoryFactory, serviceLayerFactory, mapperFactory, signalREndPointFactory) { }

        [AllowNoCompany, AllowNoAccessKey, AllowAnonymous]
        [HttpPost]
        [VersionedRoute("GitHubWebHook")]
        public async Task<HttpResponseMessage> GitHubWebHook(string projectToken = null) {
            try {
                if (String.IsNullOrWhiteSpace(projectToken)) {
                    throw new ArgumentNullException(nameof(projectToken), "Project token not supplied!");
                }
                Task<byte[]> gitHubPayLoadTask = Request.Content.ReadAsByteArrayAsync();

                // Get the expected hash from the signature header
                string header = Request.Headers.GetRequestHeader( GIT_HUB_SIGNATURE_HEADER_NAME);
                string[] values = header.Split('=');
                if (values.Length != 2 || !string.Equals(values[0], GIT_HUB_SIGNATURE_HEADER_KEY, StringComparison.OrdinalIgnoreCase)) {
                    throw new Exception($"Invalid '{GIT_HUB_SIGNATURE_HEADER_NAME}' header value. Expecting a value of '{GIT_HUB_SIGNATURE_HEADER_KEY}=value");
                }
                string signature = values[1];
                byte[] gitHubPayLoad = await gitHubPayLoadTask;

                if (!ServiceLayerFactory.ProjectIntegrationActions.SaveGITHub(gitHubPayLoad, signature, projectToken)) {
                    throw new Exception("Unable to save GitHub data.");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                ErrorSignaler.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost]
        [AllowNoCompany]
        [AdminApiAction]
        [ExclusiveRunningAction(LockDuration.FiveMinutes)]
        [VersionedRoute("ProcessWebhookPayload")]
        public HttpResponseMessage ProcessWebhookPayload(int payloadsToProcess) {
            if (ServiceLayerFactory.ProjectIntegrationActions.ProcessWebhookPayloads(payloadsToProcess))
                return Request.CreateResponse(HttpStatusCode.OK);

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [AllowNoCompany, AllowNoAccessKey, AllowAnonymous]
        [HttpPost]
        [VersionedRoute("ProcessWebhookPayload2")]
        public HttpResponseMessage ProcessWebhookPayloads2(int payloadsToProcess)
        {
            if (ServiceLayerFactory.ProjectIntegrationActions.ProcessWebhookPayloads(payloadsToProcess))
                return Request.CreateResponse(HttpStatusCode.OK);

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}