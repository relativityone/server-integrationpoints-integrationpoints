using AutoMapper;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
    [TestFixture]
    public abstract class TestBase
    {
        /// <summary>
        /// All classes that are based on TestBase are executing class SetUp method during OneTimeSetUp.
        /// </summary>
        /// <remarks>
        /// It's important to notice that FixtureSetUp can be overridden. If that's the case implementation of
        /// FixtureSetUp() in derived class should containt call to base implementation. 
        /// </remarks>
        [OneTimeSetUp]
        public virtual void FixtureSetUp()
        {
            Mapper.Initialize(x => x.CreateMissingTypeMaps = true);
            SetUp();
        }

        /// <summary>
        /// This change was made because of frequent failures of Team City builds caused by exceeding 1s timeout.
        /// Time of test setup together with test method itself and test teardown is counted as tests execution time
        /// which in our environment cannot be greate than 1s. This was mainly fault of NSubstitute framework. 
        /// When NSubstitute creates mock object for the first time it takes considerable amount of time. 
        /// Subsequent calls for Substitute.For() are a lot faster thus only execution of first test in fixture was 
        /// causing problems. Now we call SetUp method during Fixtures OneTimeSetUp which doesn't count to 1s limit.
        /// Because of that when tests SetUp is called all mock object creation is almost instantaneous therefore 
        /// if CI build fails because of timeout we can assume that there is something wrong with test.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {

        }

    }
}