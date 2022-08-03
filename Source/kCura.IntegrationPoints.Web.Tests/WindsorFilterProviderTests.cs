using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Filters;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests
{
    [TestFixture, Category("Unit")]
    public class WindsorFilterProviderTests
    {
        private FilterFactoryMock _filterFactoryMock;
        private Mock<HttpActionDescriptor> _actionDescriptorMock;
        private WindsorFilterProvider _sut;

        private readonly HttpConfiguration _configuration = null;

        [SetUp]
        public void SetUp()
        {
            _filterFactoryMock = new FilterFactoryMock();
            _actionDescriptorMock = new Mock<HttpActionDescriptor>();

            _sut = new WindsorFilterProvider(_filterFactoryMock.FilterFactory);
        }

        [Test]
        public void ShouldReturnEmptyFiltersEnumerableWhenNoAttributesArePresent()
        {
            // arrange
            SetupCustomAttributes(Enumerable.Empty<LogApiExceptionFilterAttribute>().ToList());

            // act
            IEnumerable<FilterInfo> filters = _sut.GetFilters(_configuration, _actionDescriptorMock.Object);

            // assert
            filters.Should().BeEmpty("because no attributes were returned by actionDescriptor");
        }

        [Test]
        public void ShouldReturnValidFilterInfo()
        {
            // arrange
            var attributes = new List<LogApiExceptionFilterAttribute> { new LogApiExceptionFilterAttribute() };
            SetupCustomAttributes(attributes);

            // act
            IEnumerable<FilterInfo> filters = _sut.GetFilters(_configuration, _actionDescriptorMock.Object).ToList();

            // assert
            Func<FilterInfo, bool> filterInfoValidator = filterInfo =>
            {
                bool isFilterValid = ReferenceEquals(filterInfo.Instance, _filterFactoryMock.CreatedFilterInstance);
                bool isScopeValid = filterInfo.Scope == FilterScope.Action;

                return isFilterValid && isScopeValid;
            };

            _filterFactoryMock.AttributePassedToFilterFactory.Should().Be(attributes.Single());
            filters.Should().ContainSingle(x => filterInfoValidator(x));
        }

        private void SetupCustomAttributes(IList<LogApiExceptionFilterAttribute> attributes)
        {
            var attributesCollection = new Collection<LogApiExceptionFilterAttribute>(attributes);
            _actionDescriptorMock
                .Setup(x => x.GetCustomAttributes<LogApiExceptionFilterAttribute>(false))
                .Returns(attributesCollection);
        }

        private class FilterFactoryMock
        {
            public LogApiExceptionFilterAttribute AttributePassedToFilterFactory { get; private set; }
            public ExceptionFilter CreatedFilterInstance { get; private set; }

            public ExceptionFilter FilterFactory(LogApiExceptionFilterAttribute attribute)
            {
                if (AttributePassedToFilterFactory != null)
                {
                    Assert.Fail($"Expected only one call to {nameof(FilterFactoryMock)}");
                }

                AttributePassedToFilterFactory = attribute;

                CreatedFilterInstance = new ExceptionFilter(
                    attribute,
                    textSanitizerFactory: () => null,
                    loggerFactory: () => null);

                return CreatedFilterInstance;
            }
        }
    }
}
