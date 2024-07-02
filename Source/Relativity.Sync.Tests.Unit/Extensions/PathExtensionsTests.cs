﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Extensions;

namespace Relativity.Sync.Tests.Unit.Extensions
{
    [TestFixture]
    internal class PathExtensionsTests
    {
        [TestCase("C:\\Test\\path1\\path2", "C:\\Test", "path1\\path2")]
        [TestCase("C:\\Test\\path1\\path2", "C:\\Test\\", "path1\\path2")]
        [TestCase("C:\\Test\\path1\\path2", "C:\\", "Test\\path1\\path2")]
        public void MakeRelativeTo_ShouldReturnRelativePath(
            string path, string relativeTo, string expectedRelativePath)
        {
            // Act
            string relativePath = path.MakeRelativeTo(relativeTo);

            // Assert
            relativePath.Should().Be(expectedRelativePath);
        }

        [TestCase("")]
        [TestCase(null)]
        public void MakeRelativeTo_ShouldThrow_WhenPathIsNullOrEmpty(string path)
        {
            // Arrange
            const string relativeTo = "C:\\Test";

            // Act
            Func<string> func = () => path.MakeRelativeTo(relativeTo);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(null)]
        public void MakeRelativeTo_ShouldThrow_WhenRelativeToIsNullOrEmpty(string relativeTo)
        {
            // Arrange
            const string path = "C:\\Test\\path1\\path2";

            // Act
            Func<string> func = () => path.MakeRelativeTo(relativeTo);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }
    }
}