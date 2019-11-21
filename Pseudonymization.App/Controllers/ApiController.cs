using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Pseudonymization.App.Hubs;
using Pseudonymization.Core;

namespace Pseudonymization.App.Controllers
{
    // not WebApi controller
    public class ApiController : Controller
    {
        private PseudonymizationService _pseudonymizationService = new PseudonymizationService();

        public ApiController()
        {

        }

        [HttpPost]
        public async Task<JsonResult> ProcessConnectionString(string connectionString, CancellationToken cancellationToken)
        {
            var tokenMap = CancellationTokenSource.CreateLinkedTokenSource(Response.ClientDisconnectedToken, Request.TimedOutToken, cancellationToken);
            try
            {
                var data = await _pseudonymizationService.GetSchemaList(connectionString, tokenMap.Token);
                return Json(
                    new
                    {
                        schemas = data.AnalysisData,
                        providerName = data.ProviderName,
                        connectionToken = Guid.NewGuid().ToString()
                    });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public HttpStatusCodeResult RunPseudonymization(
            string connectionString,
            IEnumerable<PseudonymizationSchemaRepresentation> schemaList,
            string providerName,
            string connectionToken)
        {
            HostingEnvironment.QueueBackgroundWorkItem((t) =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(1000);
                        var chub = new CommunicationHub();

                        var service = new PseudonymizationService();
                        service.OnProgressUpdated((s, e) =>
                        {
                            Trace.Write(e.Percetange);
                            chub.updateProgress(e.Percetange, connectionToken);
                        });
                        service.OnSucceeded((s, e) =>
                        {
                            chub.Success(connectionToken);
                        });

                        service.OnFailed((s, e) =>
                        {
                            chub.Failure(connectionToken);
                        });

                        await service.Pseudonymize(connectionString, providerName, schemaList);
                    }
                    catch { } // log must be
                });
            });

            return new HttpStatusCodeResult(200);
        }
    }
}