using System.Collections.Generic;
using FluentAssertions;
using MonitorLib.Extensions;
using Xunit;

namespace MonitorLib.Tests.Extensions
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void Each_WithValidCollection_ShouldInvokeActionForEachElement()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            var results = new List<int>();

            // Act
            collection.Each(item => results.Add(item * 2));

            // Assert
            results.Should().Equal(2, 4, 6, 8, 10);
        }

        [Fact]
        public void Each_WithEmptyCollection_ShouldNotInvokeAction()
        {
            // Arrange
            var collection = new List<int>();
            var actionInvoked = false;

            // Act
            collection.Each(item => actionInvoked = true);

            // Assert
            actionInvoked.Should().BeFalse();
        }

        [Fact]
        public void Each_WithNullAction_ShouldNotThrow()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act & Assert
            collection.Invoking(c => c.Each(null))
                .Should().NotThrow();
        }

        [Fact]
        public void Each_WithStringCollection_ShouldProcessAllElements()
        {
            // Arrange
            var collection = new List<string> { "a", "b", "c" };
            var result = "";

            // Act
            collection.Each(item => result += item);

            // Assert
            result.Should().Be("abc");
        }

        [Fact]
        public void Each_WithComplexObjects_ShouldProcessCorrectly()
        {
            // Arrange
            var collection = new List<Person>
            {
                new Person { Name = "Alice", Age = 30 },
                new Person { Name = "Bob", Age = 25 }
            };
            var totalAge = 0;

            // Act
            collection.Each(person => totalAge += person.Age);

            // Assert
            totalAge.Should().Be(55);
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
