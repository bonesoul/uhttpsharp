using Moq;
using NUnit.Framework;

namespace uhttpsharp.Tests
{
    [TestFixture]
    public class HttpMethodProviderCacheTests
    {
        private const string MethodName = "Hello World";

        private static IHttpMethodProvider GetTarget(IHttpMethodProvider child)
        {
            return new HttpMethodProviderCache(child);
        }

        [Test]
        public void Should_Call_Child_With_Right_Parameters()
        {
            // Arrange
            var mock = new Mock<IHttpMethodProvider>();
            var target = GetTarget(mock.Object);

            // Act
            target.Provide(MethodName);

            // Assert
            mock.Verify(m => m.Provide(MethodName), Times.Once);
        }

        [Test]
        public void Should_Return_Same_Child_Value()
        {
            // Arrange
            const HttpMethods expectedMethod = HttpMethods.Post;

            var mock = new Mock<IHttpMethodProvider>();
            var target = GetTarget(mock.Object);

            mock.Setup(m => m.Provide(MethodName)).Returns(expectedMethod);

            // Act
            var actual = target.Provide(MethodName);

            // Assert
            Assert.That(actual, Is.EqualTo(expectedMethod));
        }

        [Test]
        public void Should_Cache_The_Value()
        {
            // Arrange
            var mock = new Mock<IHttpMethodProvider>();
            var target = GetTarget(mock.Object);

            // Act
            target.Provide(MethodName);
            target.Provide(MethodName);
            target.Provide(MethodName);

            // Assert
            mock.Verify(m => m.Provide(MethodName), Times.Once);
        }

    }
}
