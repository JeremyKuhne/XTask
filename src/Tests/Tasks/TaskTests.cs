// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Tasks
{
    using NSubstitute;
    using Utility;
    using XTask.Tasks;
    using Xunit;

    public class TaskTests
    {
        public class TestTask : Task
        {
            protected override ExitCode ExecuteInternal()
            {
                this.Output("TestOutput");
                return ExitCode.Success;
            }

            protected override string GeneralHelp
            {
                get { return TestGetGeneralHelp(); }
            }

            protected override string OptionDetails
            {
                get { return TestGetOptionDetails(); }
            }

            public virtual string TestGetGeneralHelp()
            {
                return null;
            }

            public virtual string TestGetOptionDetails()
            {
                return null;
            }
        }

        [Fact]
        public void HandlesNullHelp()
        {
            TestTask testTask = Substitute.ForPartsOf<TestTask>();
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            testTask.OutputUsage(interaction);
            testTask.Received(1).TestGetGeneralHelp();
        }

        [Fact]
        public void HandlesNullOptionDetails()
        {
            TestTask testTask = Substitute.ForPartsOf<TestTask>();
            testTask.TestGetGeneralHelp().Returns("GeneralHelp");
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            testTask.OutputUsage(interaction);
            testTask.Received(2).TestGetGeneralHelp();
            testTask.Received(1).TestGetOptionDetails();
        }

        [Fact]
        public void OutputOutputs()
        {
            TestTask testTask = Substitute.ForPartsOf<TestTask>();
            ITaskInteraction interaction = Substitute.For<ITaskInteraction>();
            testTask.Execute(interaction);
            interaction.Received(1).Output("TestOutput");
        }
    }
}
