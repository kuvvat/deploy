using System;
using System.Threading.Tasks;
using Sovren.Configuration;
using Sovren.Models.API.Parsing;
namespace Sovren
{
    public static class ResumeService
    {
        public static async Task<ParseResumeResponse> Parse(byte[] resume)
        {

            var doc = new Models.Document(resume,DateTime.Now);
            var client = new SovrenClient(SovrenConfig.SovrenAccountId, SovrenConfig.SovrenServiceKey, DataCenter.EU);
            var request = new ParseRequest(doc, new ParseOptions());
            ParseResumeResponse response = null;

            try
            {
                response = await client.ParseResume(request);
            }
            catch (SovrenException exception)
            {
                // this was an outright failure, always try/catch for SovrenExceptions when using SovrenClient
                Console.WriteLine($"Error: {exception.SovrenErrorCode}, Message: {exception.Message}");
            }

            return response;
        }
    }
}