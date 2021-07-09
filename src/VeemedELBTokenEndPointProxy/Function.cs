using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using RestSharp;
using System.Text.Json;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace VeemedELBTokenEndPointProxy
{
    public class Function
    {
        
        /// <summary>
        /// Lambda function handler to respond to events coming from an Application Load Balancer.
        /// 
        /// Note: If "Multi value headers" is disabled on the ELB Target Group then use the Headers and QueryStringParameters properties 
        /// on the ApplicationLoadBalancerRequest and ApplicationLoadBalancerResponse objects. If "Multi value headers" is enabled then
        /// use MultiValueHeaders and MultiValueQueryStringParameters properties.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ApplicationLoadBalancerResponse> FunctionHandler(ApplicationLoadBalancerRequest request, ILambdaContext context)
        {

            var response = new ApplicationLoadBalancerResponse
            {
                StatusCode = 200,
                StatusDescription = "200 OK",
                IsBase64Encoded = false
            };


            string platformUrl = Environment.GetEnvironmentVariable("DOWNSTREAM_ENDPOINT");
            string token = Environment.GetEnvironmentVariable("DOWNSTREAM_TOKEN");

            string serialNumber = Environment.GetEnvironmentVariable("SERIAL_NUMBER");
            string mrn = Environment.GetEnvironmentVariable("MRN");
            string patientFirstName = Environment.GetEnvironmentVariable("PATIENT_FIRST_NAME");
            string patientLastName = Environment.GetEnvironmentVariable("PATIENT_LAST_NAME");
            string patientDOB = Environment.GetEnvironmentVariable("PATIENT_DOB");
            string patientSex = Environment.GetEnvironmentVariable("PATIENT_SEX");
            string reason = Environment.GetEnvironmentVariable("REASON");
            string providerId = Environment.GetEnvironmentVariable("PROVIDER_ID");

            int timeout = Int32.Parse(Environment.GetEnvironmentVariable("DOWNSTREAM_TIMEOUT"));

            var client = new RestClient(platformUrl);
            client.Timeout = (timeout >= 3 ? timeout : 5) * 1000;

            var httpRequest = new RestRequest(Method.POST);
            httpRequest.AddHeader("x-api-key", token);
            httpRequest.AddHeader("Accept", "application/json");
            httpRequest.AddHeader("Content-Type", "application/json");

            var tokenPostBody = new TokenPostBody()
            {
                SerialNumber = serialNumber ?? "SN-01",
                MRN = mrn ?? "6933",
                PatientFirstName = patientFirstName ?? "SMART",
                PatientLastName = patientLastName ?? "TIMMY",
                PatientDOB = patientDOB ?? "2012-02-19",
                PatientSex = patientSex ?? "Male",
                Reason = reason ?? "Pain",
                ProviderId = providerId ?? "zeeshan.anwer@veemed.com"
            };

            var postBodyString = JsonSerializer.Serialize(tokenPostBody);
            LambdaLogger.Log($"POST BODY: [{postBodyString}]");

            httpRequest.AddParameter("application/json", postBodyString, ParameterType.RequestBody);

            var result = await Task.Run(() =>
            {
                IRestResponse downstreamResponse = null;
                try
                {
                    LambdaLogger.Log("Initiating request..");
                    downstreamResponse = client.Execute(httpRequest);
                }
                catch (Exception ex)
                {

                    LambdaLogger.Log($"ERROR: [{ex.Message}]");
                    var error = new
                    {
                        Error = ex.Message
                    };

                    response.Body = JsonSerializer.Serialize(error);
                }

                return downstreamResponse;
            });

            if (result is null) {
                var error = new
                {
                    Error = "Unable to get response from downstream."
                };
                response.Body = JsonSerializer.Serialize(error);

                return response;
            }

            LambdaLogger.Log(result.Content);

            if (!result.StatusCode.Equals(HttpStatusCode.OK)) { 
                var error = new
                {
                    Error = $"Invalid token response: StatusCode[{ result.StatusCode}] ErrorMessage[{ result.ErrorMessage}]"
                };
                response.Body = JsonSerializer.Serialize(error);

                return response;
            }

            LambdaLogger.Log($"Downstream response StatusCode[{ result.StatusCode}] ErrorMessage[{ result.ErrorMessage}]");
            // If "Multi value headers" is enabled for the ELB Target Group then use the "response.MultiValueHeaders" property instead of "response.Headers".
            response.Headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json" }
            };

            response.Body = result.Content.ToString();

            LambdaLogger.Log($"Response: [{JsonSerializer.Serialize(response)}]");

            return response;
        }
    }
}
