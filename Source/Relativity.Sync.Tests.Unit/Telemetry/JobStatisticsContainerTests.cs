using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
    public class JobStatisticsContainerTests
    {
        private JobStatisticsContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new JobStatisticsContainer();
        }

        [Test]
        public void LongTextStreamsCount_ShouldReturnNumberOfLongTextStreams()
        {
            // arrange
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics());

            // act
            int count = _sut.LongTextStreamsCount;

            // assert
            count.Should().Be(1);
        }

        [Test]
        public void LongTextStreamsTotalSizeInBytes_ShouldReturnTotalSizeOfLongTexts()
        {
            // arrange
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 1
            });
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 2
            });

            // act
            long totalSize = _sut.LongTextStreamsTotalSizeInBytes;

            // assert
            totalSize.Should().Be(3);
        }

        [Test]
        public void LargestLongTextStreamStatistics_ShouldReturnLargestLongTextStreamStats()
        {
            // arrange
            var smallerStats = new LongTextStreamStatistics()
            {
                TotalBytesRead = 1
            };
            var biggerStats = new LongTextStreamStatistics()
            {
                TotalBytesRead = 2
            };

            _sut.LongTextStatistics.Add(smallerStats);
            _sut.LongTextStatistics.Add(biggerStats);

            // act
            LongTextStreamStatistics actualStats = _sut.LargestLongTextStreamStatistics;

            // assert
            actualStats.Should().Be(biggerStats);
        }

        [Test]
        public void LargestLongTextStreamStatistics_ShouldReturnNull_WhenNoStatisticsAvailable()
        {
            // act
            LongTextStreamStatistics actualStats = _sut.LargestLongTextStreamStatistics;

            // assert
            actualStats.Should().BeNull();
        }

        [Test]
        public void SmallestLongTextStreamStatistics_ShouldReturnSmallestLongTextStreamStats()
        {
            // arrange
            var smallerStats = new LongTextStreamStatistics()
            {
                TotalBytesRead = 1
            };
            var biggerStats = new LongTextStreamStatistics()
            {
                TotalBytesRead = 2
            };

            _sut.LongTextStatistics.Add(smallerStats);
            _sut.LongTextStatistics.Add(biggerStats);

            // act
            LongTextStreamStatistics actualStats = _sut.SmallestLongTextStreamStatistics;

            // assert
            actualStats.Should().Be(smallerStats);
        }

        [Test]
        public void SmallestLongTextStreamStatistics_ShouldReturnNullWhenNoStatisticsAvailable()
        {
            // act
            LongTextStreamStatistics actualStats = _sut.SmallestLongTextStreamStatistics;

            // assert
            actualStats.Should().BeNull();
        }

        [Test]
        public void CalculateMedianLongTextStreamSize_ShouldReturn0_WhenNoStatisticsAvailable()
        {
            // act
            long median = _sut.CalculateMedianLongTextStreamSize();

            // assert
            median.Should().Be(0);
        }

        [Test]
        public void CalculateMedianLongTextStreamSize_ShouldReturnSingleValue_WhenOnlyOneStatisticsAvailable()
        {
            // arrange
            var stats = new LongTextStreamStatistics()
            {
                TotalBytesRead = 1
            };

            _sut.LongTextStatistics.Add(stats);

            // act
            long median = _sut.CalculateMedianLongTextStreamSize();

            // assert
            median.Should().Be(1);
        }

        [Test]
        public void CalculateMedianLongTextStreamSize_ShouldReturnMean_WhenEvenNumberOfStatisticsAvailable()
        {
            // arrange
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 4
            });
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 9
            });
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 2
            });
            _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = 1
            });

            // act
            long median = _sut.CalculateMedianLongTextStreamSize();

            // assert
            median.Should().Be(3);
        }

        [Test]
        public void CalculateMedianLongTextStreamSize_ShouldReturnMedian_WhenOddNumberOfStatisticsAvailable()
        {
            // arrange
            Enumerable.Range(1, 5).ForEach(x => _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = x
            }));
            _sut.LongTextStatistics.Shuffle();

            // act
            long median = _sut.CalculateMedianLongTextStreamSize();

            // assert
            median.Should().Be(3);
        }

        [Test]
        public void CalculateAverageLongTextStreamSizeAndTime_ShouldReturnPairOf0_WhenNoStatisticsAvailable()
        {
            // act
            Tuple<double, double> average = _sut.CalculateAverageLongTextStreamSizeAndTime(x => true);

            // assert
            average.Item1.Should().Be(0);
            average.Item2.Should().Be(0);
        }

        [Test]
        public void CalculateAverageLongTextStreamSizeAndTime_ShouldReturnAverage_WhenMoreStatisticsAreAvailable()
        {
            // arrange
            Enumerable.Range(1, 3).ForEach(x => _sut.LongTextStatistics.Add(new LongTextStreamStatistics()
            {
                TotalBytesRead = x * 1024 * 1024,
                TotalReadTime = TimeSpan.FromSeconds(x)
            }));

            // act
            Tuple<double, double> average = _sut.CalculateAverageLongTextStreamSizeAndTime(x => true);

            // assert
            average.Item1.Should().Be(2);
            average.Item2.Should().Be(2);
        }
    }
}
