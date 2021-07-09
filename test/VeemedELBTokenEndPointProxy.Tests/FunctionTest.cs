using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using Amazon.Lambda.TestUtilities;

using VeemedELBTokenEndPointProxy;

namespace VeemedELBTokenEndPointProxy.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestSampleFunction()
        {
            var function = new Function();
            var context = new TestLambdaContext();
            var request = new ApplicationLoadBalancerRequest();
            var response = function.FunctionHandler(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Contains("Hello World from Lambda", response.Body);
        }
    }
}
