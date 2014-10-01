using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace uhttpsharp.Tests
{
    [TestFixture]
    public class HttpMethodProviderTests
    {

        private static IHttpMethodProvider GetTarget()
        {
            return new HttpMethodProvider();
        }


        private IEnumerable<string> Methods
        {
            get
            {
                return Enum.GetNames(typeof(HttpMethods));
            }
        }
            
        [TestCaseSource("Methods")]
        public void Should_Get_Right_Method(string methodName)
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = target.Provide(methodName);

            // Assert
            Assert.IsTrue(StringComparer.InvariantCultureIgnoreCase.Equals(actual.ToString(), methodName));
        }

    }
}
