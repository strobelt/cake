﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.Common.Tests.Fixtures.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Testing;
using Xunit;

namespace Cake.Common.Tests.Unit.Tools.DotNetCore.Test
{
    public sealed class DotNetCoreTesterTests
    {
        public sealed class TheTestMethod
        {
            [Fact]
            public void Should_Throw_If_Settings_Are_Null()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Project = "./src/*";
                fixture.Settings = null;
                fixture.GivenDefaultToolDoNotExist();

                // When
                var result = Record.Exception(() => fixture.Run());

                // Then
                AssertEx.IsArgumentNullException(result, "settings");
            }

            [Fact]
            public void Should_Throw_If_Process_Was_Not_Started()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Project = "./src/*";
                fixture.GivenProcessCannotStart();

                // When
                var result = Record.Exception(() => fixture.Run());

                // Then
                AssertEx.IsCakeException(result, ".NET Core CLI: Process was not started.");
            }

            [Fact]
            public void Should_Throw_If_Process_Has_A_Non_Zero_Exit_Code()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Project = "./src/*";
                fixture.GivenProcessExitsWithCode(1);

                // When
                var result = Record.Exception(() => fixture.Run());

                // Then
                AssertEx.IsCakeException(result, ".NET Core CLI: Process returned an error (exit code 1).");
            }

            [Fact]
            public void Should_Add_Mandatory_Arguments()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal("test", result.Args);
            }

            [Fact]
            public void Should_Add_Path()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Project = "./test/Project.Tests/*";

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal("test \"./test/Project.Tests/*\"", result.Args);
            }

            [Theory]
            [InlineData("./test/*", "test \"./test/*\"")]
            [InlineData("./test/cake unit tests/", "test \"./test/cake unit tests/\"")]
            [InlineData("./test/cake unit tests/cake core tests", "test \"./test/cake unit tests/cake core tests\"")]
            public void Should_Quote_Project_Path(string text, string expected)
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Project = text;

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal(expected, result.Args);
            }

            [Fact]
            public void Should_Add_RunSettings_Arguments()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Arguments = new[] { "MSTest.DeploymentEnabled=false", "MSTest.MapInconclusiveToFailed=true" }.ToProcessArguments();

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal("test -- MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=true", result.Args);
            }

            [Fact]
            public void Should_Add_Additional_Settings()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Settings.NoBuild = true;
                fixture.Settings.NoRestore = true;
                fixture.Settings.NoLogo = true;
                fixture.Settings.Framework = "dnxcore50";
                fixture.Settings.Configuration = "Release";
                fixture.Settings.Collectors = new[] { "XPlat Code Coverage" };
                fixture.Settings.OutputDirectory = "./artifacts/";
                fixture.Settings.Settings = "./demo.runsettings";
                fixture.Settings.Filter = "Priority = 1";
                fixture.Settings.TestAdapterPath = @"/Working/custom-test-adapter";
#pragma warning disable CS0618
                fixture.Settings.Logger = "trx;LogFileName=/Working/logfile.trx";
#pragma warning restore CS0618
                fixture.Settings.Loggers = new[] { "html;LogFileName=/Working/logfile.html" };
                fixture.Settings.DiagnosticFile = "./artifacts/logging/diagnostics.txt";
                fixture.Settings.ResultsDirectory = "./tests/";
                fixture.Settings.VSTestReportPath = "./tests/TestResults.xml";
                fixture.Settings.Runtime = "win-x64";
                fixture.Settings.Blame = true;
                fixture.Settings.Sources = new[] { "https://api.nuget.org/v3/index.json" };

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal("test --settings \"/Working/demo.runsettings\" --filter \"Priority = 1\" --test-adapter-path \"/Working/custom-test-adapter\" --logger \"trx;LogFileName=/Working/logfile.trx\" --logger \"html;LogFileName=/Working/logfile.html\" --output \"/Working/artifacts\" --framework dnxcore50 --configuration Release --collect \"XPlat Code Coverage\" --diag \"/Working/artifacts/logging/diagnostics.txt\" --no-build --no-restore --nologo --results-directory \"/Working/tests\" --logger trx;LogFileName=\"/Working/tests/TestResults.xml\" --runtime win-x64 --source \"https://api.nuget.org/v3/index.json\" --blame", result.Args);
            }

            [Fact]
            public void Should_Add_Host_Arguments()
            {
                // Given
                var fixture = new DotNetCoreTesterFixture();
                fixture.Settings.DiagnosticOutput = true;

                // When
                var result = fixture.Run();

                // Then
                Assert.Equal("--diagnostics test", result.Args);
            }
        }
    }
}
