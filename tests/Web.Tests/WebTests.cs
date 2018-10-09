using Xunit;

namespace Web.Tests
{
    public class WebTests
    {
        [Fact]
        public void Compare1And2_ShouldReturnNotEqual()
        {
            const int value1 = 1;
            const int value2 = 2;
            Assert.NotEqual(value1, value2);
        }

        [Fact]
        public void Sum1Plus1_ShouldReturn2()
        {
            var onePlusOne = 1 + 1;
            var sum = 3;
            Assert.Equal(onePlusOne, sum);
        }
    }
}
