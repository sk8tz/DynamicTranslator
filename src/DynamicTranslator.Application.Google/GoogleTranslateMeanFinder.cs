using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Abp.Dependency;

using DynamicTranslator.Application.Google.Configuration;
using DynamicTranslator.Application.Model;
using DynamicTranslator.Application.Orchestrators;
using DynamicTranslator.Configuration.Startup;
using DynamicTranslator.Constants;
using DynamicTranslator.Domain.Model;
using DynamicTranslator.Extensions;

using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace DynamicTranslator.Application.Google
{
    public class GoogleTranslateMeanFinder : IMeanFinder, IOrchestrator, ITransientDependency
    {
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        private const string AcceptEncoding = "gzip, deflate, sdch";
        private const string AcceptLanguage = "en-US,en;q=0.8,tr;q=0.6";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";

        private readonly IApplicationConfiguration _applicationConfiguration;
        private readonly IGoogleTranslatorConfiguration _googleConfiguration;
        private readonly IMeanOrganizerFactory _meanOrganizerFactory;

        public GoogleTranslateMeanFinder(IMeanOrganizerFactory meanOrganizerFactory,
            IGoogleTranslatorConfiguration googleConfiguration,
            IApplicationConfiguration applicationConfiguration)
        {
            _meanOrganizerFactory = meanOrganizerFactory;
            _googleConfiguration = googleConfiguration;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<TranslateResult> Find(TranslateRequest translateRequest)
        {
            if (!_googleConfiguration.CanSupport() || !_googleConfiguration.IsActive())
            {
                return new TranslateResult(false, new Maybe<string>());
            }

            var uri = string.Format(
                _googleConfiguration.Url,
                _applicationConfiguration.ToLanguage.Extension,
                _applicationConfiguration.ToLanguage.Extension,
                HttpUtility.UrlEncode(translateRequest.CurrentText, Encoding.UTF8));

            var response = await new RestClient(uri) { Encoding = Encoding.UTF8 }
                .ExecuteGetTaskAsync(
                    new RestRequest(Method.GET)
                        .AddHeader(Headers.AcceptLanguage, AcceptLanguage)
                        .AddHeader(Headers.AcceptEncoding, AcceptEncoding)
                        .AddHeader(Headers.UserAgent, UserAgent)
                        .AddHeader(Headers.Accept, Accept));

            var mean = new Maybe<string>();

            if (response.Ok())
            {
                var organizer = _meanOrganizerFactory.GetMeanOrganizers().First(x => x.TranslatorType == TranslatorType);
                mean = await organizer.OrganizeMean(response.Content);
            }

            return new TranslateResult(true, mean);
        }

        public TranslatorType TranslatorType => TranslatorType.Google;
    }
}
